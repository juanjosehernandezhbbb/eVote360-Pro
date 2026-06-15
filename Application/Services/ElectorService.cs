using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ElectorService : IElectorService
    {
        private readonly IGenericRepository<Ciudadano> _ciudadanoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<CodigoVerificacion> _codigoRepo;
        private readonly IGenericRepository<VotacionCiudadano> _votacionCiudadanoRepo;
        private readonly IGenericRepository<PuestoElectivo> _puestoRepo;
        private readonly IGenericRepository<CandidatoPuesto> _asignacionRepo;
        private readonly IGenericRepository<Voto> _votoRepo;
        private readonly IOcrService _ocrService;
        private readonly IEmailService _emailService;

        public ElectorService(
            IGenericRepository<Ciudadano> ciudadanoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<CodigoVerificacion> codigoRepo,
            IGenericRepository<VotacionCiudadano> votacionCiudadanoRepo,
            IGenericRepository<PuestoElectivo> puestoRepo,
            IGenericRepository<CandidatoPuesto> asignacionRepo,
            IGenericRepository<Voto> votoRepo,
            IOcrService ocrService,
            IEmailService emailService)
        {
            _ciudadanoRepo = ciudadanoRepo;
            _eleccionRepo = eleccionRepo;
            _codigoRepo = codigoRepo;
            _votacionCiudadanoRepo = votacionCiudadanoRepo;
            _puestoRepo = puestoRepo;
            _asignacionRepo = asignacionRepo;
            _votoRepo = votoRepo;
            _ocrService = ocrService;
            _emailService = emailService;
        }

        public async Task ValidarDocumentoAsync(string numeroDocumento)
        {
            // 1. Validar que exista una elección activa
            var eleccionActiva = await ObtenerEleccionActivaAsync();

            // 2. Validar que el ciudadano exista y esté activo
            var ciudadanos = await _ciudadanoRepo.GetAllAsync();
            var ciudadano = ciudadanos.FirstOrDefault(c => c.NumeroDocumento == numeroDocumento);

            if (ciudadano == null) throw new Exception("No existe un ciudadano registrado con este número de documento."); 
            if (!ciudadano.Estado) throw new Exception("Este ciudadano se encuentra inactivo y no puede participar en el proceso de votación."); 

            // 3. Validar que no haya votado ya
            bool yaVoto = await _votacionCiudadanoRepo.ExistsAsync(vc => vc.CiudadanoId == ciudadano.Id && vc.EleccionId == eleccionActiva.Id);
            if (yaVoto) throw new Exception("Ya ha ejercido su derecho al voto."); 
        }

        public async Task<bool> ValidarIdentidadOcrAsync(string numeroDocumento, Stream imagenCedula)
        {
            await ValidarDocumentoAsync(numeroDocumento); // Re-validar estado base

            // Llamar al servicio de OCR
            string textoExtraido = await _ocrService.ExtraerTextoDeImagenAsync(imagenCedula); 

            // Limpiar el texto extraído (quitar espacios, guiones, saltos de línea)
            string textoLimpio = new string(textoExtraido.Where(char.IsLetterOrDigit).ToArray());

            if (!textoLimpio.Contains(numeroDocumento))
            {
                throw new Exception("Los datos extraídos de la foto no coinciden con los datos previamente ingresados por el elector."); 
            }

            return true;
        }

        public async Task EnviarCodigoVerificacionAsync(string numeroDocumento)
        {
            var eleccionActiva = await ObtenerEleccionActivaAsync();
            var ciudadanos = await _ciudadanoRepo.GetAllAsync();
            var ciudadano = ciudadanos.FirstOrDefault(c => c.NumeroDocumento == numeroDocumento);

            if (string.IsNullOrEmpty(ciudadano.Email))
                throw new Exception("Este ciudadano no tiene un correo electrónico registrado. No es posible continuar con la verificación de identidad."); 

            // Generar código de 6 dígitos
            string codigo = new Random().Next(100000, 999999).ToString(); 

            var nuevoCodigo = new CodigoVerificacion
            {
                CiudadanoId = ciudadano.Id,
                EleccionId = eleccionActiva.Id,
                Codigo = codigo,
                FechaGeneracion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddMinutes(5), 
                Usado = false
            }; 

            await _codigoRepo.AddAsync(nuevoCodigo);

            // Enviar correo
            string asunto = "Código de verificación para votar";
            string cuerpo = $"Hola {ciudadano.Nombre},<br/><br/>Su código de verificación para continuar con el proceso de votación es: <b>{codigo}</b><br/><br/>Este código tendrá una vigencia de 5 minutos.<br/>Si usted no inició este proceso, ignore este mensaje."; 

            await _emailService.EnviarCorreoAsync(ciudadano.Email, asunto, cuerpo);
        }

        public async Task<bool> ValidarCodigoVerificacionAsync(string numeroDocumento, string codigo)
        {
            var eleccionActiva = await ObtenerEleccionActivaAsync();
            var ciudadanos = await _ciudadanoRepo.GetAllAsync();
            var ciudadano = ciudadanos.FirstOrDefault(c => c.NumeroDocumento == numeroDocumento);

            var codigos = await _codigoRepo.GetAllAsync();
            var codigoBD = codigos.Where(c => c.CiudadanoId == ciudadano.Id && c.EleccionId == eleccionActiva.Id)
                                  .OrderByDescending(c => c.FechaGeneracion)
                                  .FirstOrDefault();

            if (codigoBD == null || codigoBD.Codigo != codigo)
                throw new Exception("El código de verificación ingresado no es válido."); 

            if (codigoBD.Usado)
                throw new Exception("Este código de verificación ya fue utilizado.");

            if (DateTime.Now > codigoBD.FechaExpiracion)
                throw new Exception("El código de verificación ha expirado. Solicite un nuevo código para continuar.");

            codigoBD.Usado = true;
            await _codigoRepo.UpdateAsync(codigoBD);

            return true;
        }

        // Fíjate que unifiqué los métodos de votación de la interfaz en uno solo para guardar la boleta completa
        public async Task FinalizarVotacionAsync(int ciudadanoId, List<SeleccionVotoDto> selecciones)
        {
            var eleccionActiva = await ObtenerEleccionActivaAsync();

            bool yaVoto = await _votacionCiudadanoRepo.ExistsAsync(vc => vc.CiudadanoId == ciudadanoId && vc.EleccionId == eleccionActiva.Id);
            if (yaVoto) throw new Exception("Ya ha ejercido su derecho al voto."); 
            // Validar que haya seleccionado una opción por cada puesto disponible
            var puestosActivos = (await _puestoRepo.GetAllAsync()).Where(p => p.Estado).ToList();
            var puestosFaltantes = puestosActivos.Where(p => !selecciones.Any(s => s.PuestoElectivoId == p.Id)).ToList();

            if (puestosFaltantes.Any())
            {
                var nombresFaltantes = string.Join(", ", puestosFaltantes.Select(p => p.Nombre));
                throw new Exception($"Debe completar su selección para los siguientes puestos electivos: {nombresFaltantes}."); 
            }

            // 1. Crear el registro principal del Voto
            var nuevoVoto = new Voto
            {
                CiudadanoId = ciudadanoId,
                EleccionId = eleccionActiva.Id,
                FechaVoto = DateTime.Now
            };

            await _votoRepo.AddAsync(nuevoVoto);


            // 2. Marcar al ciudadano como que ya votó
            var votacionCiudadano = new VotacionCiudadano
            {
                CiudadanoId = ciudadanoId,
                EleccionId = eleccionActiva.Id,
                VotoFinalizado = true,
                Nombre = "Registro de Voto",
                Estado = true
            };
            await _votacionCiudadanoRepo.AddAsync(votacionCiudadano);

            // 3. Enviar correo de resumen
            await EnviarResumenVotacionAsync(ciudadanoId, eleccionActiva.Id, selecciones); 
        }

        // --- Métodos Privados ---

        private async Task<Eleccion> ObtenerEleccionActivaAsync()
        {
            var elecciones = await _eleccionRepo.GetAllAsync();
            var eleccionActiva = elecciones.FirstOrDefault(e => e.Estado == true);

            if (eleccionActiva == null)
            {
                throw new Exception("No hay ningún proceso electoral en estos momentos.");
            }
            return eleccionActiva;
        }

        private async Task EnviarResumenVotacionAsync(int ciudadanoId, int eleccionId, List<SeleccionVotoDto> selecciones)
        {
            var ciudadano = await _ciudadanoRepo.GetByIdAsync(ciudadanoId);
            var eleccion = await _eleccionRepo.GetByIdAsync(eleccionId);
            var asignaciones = await _asignacionRepo.GetAllAsync();
            var puestos = await _puestoRepo.GetAllAsync();

            var sb = new StringBuilder();
            sb.Append($"Hola {ciudadano.Nombre},<br/><br/>Su proceso de votación ha sido completado correctamente.<br/><br/><b>Resumen de selección:</b><br/>"); 
            sb.Append($"Elección: {eleccion.Nombre}<br/><br/>"); 

            foreach (var seleccion in selecciones)
            {
                var puesto = puestos.FirstOrDefault(p => p.Id == seleccion.PuestoElectivoId);
                string nombreCandidato = "Ninguno";
                string nombrePartido = "No aplica";

                if (seleccion.CandidatoId.HasValue)
                {
                    var asignacion = asignaciones.FirstOrDefault(a => a.CandidatoId == seleccion.CandidatoId && a.PuestoElectivoId == seleccion.PuestoElectivoId);
                    if (asignacion != null && asignacion.Candidato != null)
                    {
                        nombreCandidato = $"{asignacion.Candidato.Nombre} {asignacion.Candidato.Apellido}"; 
                        nombrePartido = asignacion.PartidoPolitico != null ? asignacion.PartidoPolitico.Nombre : "Sin Partido"; 
                    }
                }

                sb.Append($"Puesto: {puesto?.Nombre}<br/>"); 
                sb.Append($"Selección: {nombreCandidato}<br/>");
                sb.Append($"Partido: {nombrePartido}<br/><br/>"); 
            }

            sb.Append("Gracias por ejercer su derecho al voto."); 

            await _emailService.EnviarCorreoAsync(ciudadano.Email, "Resumen de su participación electoral", sb.ToString()); 
        }
    }
}