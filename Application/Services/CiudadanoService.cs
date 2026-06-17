using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CiudadanoService : ICiudadanoService
    {
        private readonly IGenericRepository<Ciudadano> _ciudadanoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<VotacionCiudadano> _votacionCiudadanoRepo;
        private readonly IOcrService _ocrService;

        public CiudadanoService(
            IGenericRepository<Ciudadano> ciudadanoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<VotacionCiudadano> votacionCiudadanoRepo,
            IOcrService ocrService)
        {
            _ciudadanoRepo = ciudadanoRepo;
            _eleccionRepo = eleccionRepo;
            _votacionCiudadanoRepo = votacionCiudadanoRepo;
            _ocrService = ocrService;
        }

        public async Task<CiudadanoDto> ProcesarOcrAsync(Stream fileStream)
        {
            using var memoriaStream = new MemoryStream();
            await fileStream.CopyToAsync(memoriaStream);

            // ==========================================
            // 1. EXTRACCIÓN GLOBAL (CÉDULA)
            // ==========================================
            memoriaStream.Position = 0;
            var textoCompleto = await _ocrService.ExtraerTextoDeImagenAsync(memoriaStream);

            var matchCedula = Regex.Match(textoCompleto, @"\d{3}-?\d{7}-?\d{1}");
            string cedula = matchCedula.Success ? matchCedula.Value : "";

            if (!string.IsNullOrEmpty(cedula) && !ValidarCedulaDominicana(cedula))
            {
                cedula = "";
            }

            // ==========================================
            // 2. EXTRACCIÓN REGIONAL (NOMBRES Y APELLIDOS)
            // ==========================================
            memoriaStream.Position = 0;
            string letrasPermitidas = "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ ";

            var textoNombresRegion = await _ocrService.ExtraerTextoDeRegionAsync(
                memoriaStream,
                0.02f, 0.81f, 0.45f, 0.17f,
                letrasPermitidas,
                esUnaSolaLinea: false
            );

            var lineasCrudas = textoNombresRegion.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(l => l.Trim())
                                                 .ToList();

            var lineasValidas = new List<string>();

            foreach (var linea in lineasCrudas)
            {
                string lineaCorregida = CorregirErroresOcr(linea);

                var palabras = lineaCorregida.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length == 0) continue;

                // CORREGIDO: Nombre de variable corregido para evitar el error de compilación
                int palabrasCortas = palabras.Count(p => p.Length <= 2 && p != "DE" && p != "DEL" && p != "Y");
                bool tieneVocales = Regex.IsMatch(lineaCorregida, @"[AEIOU]");

                if (!tieneVocales || (palabrasCortas > palabras.Length / 2 && palabras.Length > 1))
                {
                    continue;
                }

                if (lineaCorregida.Length > 2)
                {
                    lineasValidas.Add(lineaCorregida);
                }
            }

            string nombre = "";
            string apellido = "";

            if (lineasValidas.Count > 0) nombre = lineasValidas[0];
            if (lineasValidas.Count > 1) apellido = lineasValidas[1];

            return new CiudadanoDto
            {
                Nombre = nombre,
                Apellido = apellido,
                NumeroDocumento = cedula,
                Email = ""
            };
        }

        public async Task RegistrarAutonomoAsync(CiudadanoDto dto)
        {
            ValidarFormatoCorreo(dto.Email);

            var documentoLimpio = dto.NumeroDocumento.Trim();
            var correoLimpio = dto.Email.Trim().ToLower();

            if (!ValidarCedulaDominicana(documentoLimpio))
                throw new Exception("El número de documento ingresado no es una cédula válida.");

            bool existeDocumento = await _ciudadanoRepo.ExistsAsync(c => c.NumeroDocumento == documentoLimpio);
            if (existeDocumento) throw new Exception("Ya existe un ciudadano registrado con este número de documento.");

            bool existeCorreo = await _ciudadanoRepo.ExistsAsync(c => c.Email.ToLower() == correoLimpio);
            if (existeCorreo) throw new Exception("Ya existe un ciudadano registrado con este correo.");

            var nuevoCiudadano = new Ciudadano
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                NumeroDocumento = documentoLimpio,
                Email = correoLimpio,
                Estado = true
            };

            await _ciudadanoRepo.AddAsync(nuevoCiudadano);
        }

        // --- MÉTODOS DE ADMINISTRACIÓN ORIGINALES ---

        public async Task CrearAsync(CiudadanoDto dto)
        {
            await ValidarEleccionActivaAsync("crear");
            ValidarFormatoCorreo(dto.Email);

            var documentoLimpio = dto.NumeroDocumento.Trim();
            var correoLimpio = dto.Email.Trim().ToLower();

            if (!ValidarCedulaDominicana(documentoLimpio))
                throw new Exception("El número de documento ingresado no es una cédula válida.");

            bool existeDocumento = await _ciudadanoRepo.ExistsAsync(c => c.NumeroDocumento == documentoLimpio);
            if (existeDocumento) throw new Exception("Ya existe un ciudadano registrado con este número de documento de identidad.");

            bool existeCorreo = await _ciudadanoRepo.ExistsAsync(c => c.Email.ToLower() == correoLimpio);
            if (existeCorreo) throw new Exception("Ya existe un ciudadano registrado con este correo electrónico.");

            var nuevoCiudadano = new Ciudadano
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                NumeroDocumento = documentoLimpio,
                Email = correoLimpio,
                Estado = true
            };

            await _ciudadanoRepo.AddAsync(nuevoCiudadano);
        }

        public async Task UpdateAsync(CiudadanoDto dto) => await ActualizarAsync(dto);

        public async Task ActualizarAsync(CiudadanoDto dto)
        {
            await ValidarEleccionActivaAsync("editar");
            var ciudadanoExistente = await _ciudadanoRepo.GetByIdAsync(dto.Id);
            if (ciudadanoExistente == null) throw new Exception("Ciudadano no encontrado.");

            ValidarFormatoCorreo(dto.Email);

            var documentoLimpio = dto.NumeroDocumento.Trim();
            var correoLimpio = dto.Email.Trim().ToLower();

            if (!ciudadanoExistente.Email.Equals(correoLimpio, StringComparison.OrdinalIgnoreCase))
            {
                bool existeCorreo = await _ciudadanoRepo.ExistsAsync(c => c.Id != dto.Id && c.Email.ToLower() == correoLimpio);
                if (existeCorreo) throw new Exception("Ya existe un ciudadano registrado con este correo electrónico.");
                ciudadanoExistente.Email = correoLimpio;
            }

            if (ciudadanoExistente.NumeroDocumento != documentoLimpio)
            {
                if (!ValidarCedulaDominicana(documentoLimpio))
                    throw new Exception("El número de documento ingresado no es una cédula válida.");

                bool existeDocumento = await _ciudadanoRepo.ExistsAsync(c => c.Id != dto.Id && c.NumeroDocumento == documentoLimpio);
                if (existeDocumento) throw new Exception("Ya existe un ciudadano registrado con este número de documento de identidad.");

                bool yaVoto = await _votacionCiudadanoRepo.ExistsAsync(vc => vc.CiudadanoId == dto.Id);
                if (yaVoto) throw new Exception("No se puede modificar el documento de un ciudadano que ya participó en una elección.");

                ciudadanoExistente.NumeroDocumento = documentoLimpio;
            }

            ciudadanoExistente.Nombre = dto.Nombre.Trim();
            ciudadanoExistente.Apellido = dto.Apellido.Trim();
            ciudadanoExistente.Estado = dto.Estado;

            await _ciudadanoRepo.UpdateAsync(ciudadanoExistente);
        }

        public async Task CambiarEstadoAsync(int id, bool nuevoEstado)
        {
            string accion = nuevoEstado ? "activar" : "desactivar";
            await ValidarEleccionActivaAsync(accion);

            var ciudadanoExistente = await _ciudadanoRepo.GetByIdAsync(id);
            if (ciudadanoExistente == null) throw new Exception("Ciudadano no encontrado.");

            if (ciudadanoExistente.Estado == nuevoEstado)
            {
                string mensajeEstado = nuevoEstado ? "activo" : "inactivo";
                throw new Exception($"Este ciudadano ya se encuentra {mensajeEstado}.");
            }

            ciudadanoExistente.Estado = nuevoEstado;
            await _ciudadanoRepo.UpdateAsync(ciudadanoExistente);
        }

        public async Task<IEnumerable<CiudadanoDto>> ObtenerTodosAsync()
        {
            var ciudadanos = await _ciudadanoRepo.GetAllAsync();
            return ciudadanos.Select(c => new CiudadanoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Email = c.Email,
                NumeroDocumento = c.NumeroDocumento,
                Estado = c.Estado
            }).ToList();
        }

        public async Task<CiudadanoDto> ObtenerPorIdAsync(int id)
        {
            var c = await _ciudadanoRepo.GetByIdAsync(id);
            if (c == null) return null;

            return new CiudadanoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Email = c.Email,
                NumeroDocumento = c.NumeroDocumento,
                Estado = c.Estado
            };
        }

        // --- MÉTODOS PRIVADOS DE VALIDACIÓN Y LIMPIEZA ---

        private string CorregirErroresOcr(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string textoLimpio = texto.ToUpper().Trim();

            // 1. REEMPLAZOS BÁSICOS
            textoLimpio = textoLimpio.Replace("0", "O")
                                     .Replace("1", "I")
                                     .Replace("5", "S");

            // 2. EL BLINDAJE PARA "CEBALLO"
            // Este patrón busca:
            // - Empieza con C o G (error común de lectura)
            // - Luego sigue con cualquier combinación de B, 8, R, A (errores de lectura en B y A)
            // - Termina con LL + O, 0 o 9
            textoLimpio = Regex.Replace(textoLimpio, @"\b[CG]E?B[A8R]+LL[O09]\b", "CEBALLO");

            // 3. BLINDAJE PARA "JIMENEZ" (por si acaso)
            textoLimpio = Regex.Replace(textoLimpio, @"\b[HUL]I?MENEZ\b", "JIMENEZ");

            // 4. LIMPIEZA FINAL
            textoLimpio = Regex.Replace(textoLimpio, @"\s{2,}", " ");

            return textoLimpio;
        }

        private bool ValidarCedulaDominicana(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return false;

            string cedulaLimpia = cedula.Replace("-", "").Trim();

            if (cedulaLimpia.Length != 11 || !cedulaLimpia.All(char.IsDigit))
                return false;

            int suma = 0;
            int multiplicador = 1;

            for (int i = 0; i < 10; i++)
            {
                int digito = int.Parse(cedulaLimpia[i].ToString());
                int resultado = digito * multiplicador;

                if (resultado >= 10)
                {
                    resultado = (resultado / 10) + (resultado % 10);
                }

                suma += resultado;
                multiplicador = (multiplicador == 1) ? 2 : 1;
            }

            int digitoVerificador = (10 - (suma % 10)) % 10;
            int ultimoDigitoCedula = int.Parse(cedulaLimpia[10].ToString());

            return digitoVerificador == ultimoDigitoCedula;
        }

        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                throw new Exception($"No se puede {accion} un ciudadano mientras exista una elección activa.");
            }
        }

        private void ValidarFormatoCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) throw new Exception("Debe ingresar un correo electrónico válido.");
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(correo))
            {
                throw new Exception("Debe ingresar un correo electrónico válido.");
            }
        }
    }
}