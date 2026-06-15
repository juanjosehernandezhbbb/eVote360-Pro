using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using Persistance.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EleccionService : IEleccionService
    {
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<PuestoElectivo> _puestoRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;
        private readonly IGenericRepository<CandidatoPuesto> _asignacionRepo;
        private readonly IGenericRepository<DetalleVoto> _detalleVotoRepo;
        private readonly IGenericRepository<VotacionCiudadano> _votacionCiudadanoRepo;

        public EleccionService(
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<PuestoElectivo> puestoRepo,
            IGenericRepository<PartidoPolitico> partidoRepo,
            IGenericRepository<CandidatoPuesto> asignacionRepo,
            IGenericRepository<DetalleVoto> detalleVotoRepo,
            IGenericRepository<VotacionCiudadano> votacionCiudadanoRepo)
        {
            _eleccionRepo = eleccionRepo;
            _puestoRepo = puestoRepo;
            _partidoRepo = partidoRepo;
            _asignacionRepo = asignacionRepo;
            _detalleVotoRepo = detalleVotoRepo;
            _votacionCiudadanoRepo = votacionCiudadanoRepo;
        }

        public async Task CrearAsync(EleccionDto dto)
        {
            // Validar que no haya elección activa
            bool hayActiva = await _eleccionRepo.ExistsAsync(e => e.Estado == true); // Asumiendo que Estado true = Activa en tu BD, o usa el Enum si lo cambiaste
            if (hayActiva) throw new Exception("No se puede crear una nueva elección mientras exista una elección activa.");

            // Validar toda la configuración electoral (Puestos, Partidos, Asignaciones)
            await ValidarConfiguracionElectoralAsync("realizar");

            var nuevaEleccion = new Eleccion
            {
                Nombre = dto.Nombre.Trim(),
                Fecha = dto.Fecha,
                Estado = false 
            };

            await _eleccionRepo.AddAsync(nuevaEleccion);
        }

        public async Task ActivarAsync(int id)
        {
            var eleccion = await _eleccionRepo.GetByIdAsync(id);
            if (eleccion == null) throw new Exception("Elección no encontrada.");

            // Validar que no haya otra elección activa
            bool hayActiva = await _eleccionRepo.ExistsAsync(e => e.Id != id && e.Estado == true);
            if (hayActiva) throw new Exception("No se puede activar esta elección porque ya existe una elección activa.");

            // Validar de nuevo la configuración porque es dinámica en estado Pendiente
            await ValidarConfiguracionElectoralAsync("activar");

            eleccion.Estado = true; // Marca como Activa
            await _eleccionRepo.UpdateAsync(eleccion);
        }

        public async Task FinalizarAsync(int id)
        {
            var eleccion = await _eleccionRepo.GetByIdAsync(id);
            if (eleccion == null) throw new Exception("Elección no encontrada.");

            if (!eleccion.Estado) throw new Exception("Solo se pueden finalizar elecciones activas.");

            eleccion.Estado = false; // Finalizada (Deberías manejar esto con un Enum en la BD idealmente: 1=Pendiente, 2=Activa, 3=Finalizada)
            await _eleccionRepo.UpdateAsync(eleccion);
        }

        public async Task<IEnumerable<ResultadoPuestoDto>> ObtenerResultadosAsync(int eleccionId)
        {
            var detallesVoto = await _detalleVotoRepo.GetAllAsync();
            var votosDeEleccion = detallesVoto.Where(dv => dv.Voto != null && dv.Voto.EleccionId == eleccionId).ToList();

            var resultados = new List<ResultadoPuestoDto>();

            // Agrupar por Puesto Electivo
            var gruposPorPuesto = votosDeEleccion.GroupBy(dv => dv.PuestoElectivoId);

            foreach (var grupoPuesto in gruposPorPuesto)
            {
                int totalVotosPuesto = grupoPuesto.Count();
                var puestoInfo = grupoPuesto.First().PuestoElectivo; // Necesita Include

                var resultadoPuesto = new ResultadoPuestoDto
                {
                    NombrePuesto = puestoInfo != null ? puestoInfo.Nombre : "Desconocido",
                    TotalVotosPuesto = totalVotosPuesto,
                    ResultadosCandidatos = new List<CandidatoResultadoDto>()
                };

                // Agrupar por Candidato (CandidatoId nulo significa "Ninguno")
                var gruposCandidatos = grupoPuesto.GroupBy(dv => dv.CandidatoId);

                foreach (var grupoCandidato in gruposCandidatos)
                {
                    int votosCandidato = grupoCandidato.Count();
                    double porcentaje = totalVotosPuesto > 0 ? ((double)votosCandidato / totalVotosPuesto) * 100 : 0;

                    string nombreOpcion = "Ninguno";
                    string partidoOpcion = "No aplica";

                    if (grupoCandidato.Key != null)
                    {
                        var cand = grupoCandidato.First().Candidato; // Necesita Include
                        if (cand != null)
                        {
                            nombreOpcion = $"{cand.Nombre} {cand.Apellido}";
                            partidoOpcion = cand.PartidoPolitico != null ? cand.PartidoPolitico.Nombre : "Sin Partido";
                        }
                    }

                    resultadoPuesto.ResultadosCandidatos.Add(new CandidatoResultadoDto
                    {
                        CandidatoOpcion = nombreOpcion,
                        PartidoPolitico = partidoOpcion,
                        CantidadVotos = votosCandidato,
                        PorcentajeVotos = Math.Round(porcentaje, 2)
                    });
                }

                // Ordenar de mayor a menor y calcular ganador/empate
                resultadoPuesto.ResultadosCandidatos = resultadoPuesto.ResultadosCandidatos
                    .OrderByDescending(r => r.CantidadVotos)
                    .ToList();

                if (resultadoPuesto.ResultadosCandidatos.Any())
                {
                    var maxVotos = resultadoPuesto.ResultadosCandidatos.First().CantidadVotos;
                    var empatados = resultadoPuesto.ResultadosCandidatos.Count(r => r.CantidadVotos == maxVotos);

                    for (int i = 0; i < resultadoPuesto.ResultadosCandidatos.Count; i++)
                    {
                        if (resultadoPuesto.ResultadosCandidatos[i].CantidadVotos == maxVotos)
                        {
                            // "Ninguno" no puede ser ganador
                            if (resultadoPuesto.ResultadosCandidatos[i].CandidatoOpcion == "Ninguno")
                            {
                                resultadoPuesto.ResultadosCandidatos[i].Resultado = "Opción más votada";
                            }
                            else
                            {
                                resultadoPuesto.ResultadosCandidatos[i].Resultado = empatados > 1 ? "Empate" : "Ganador";
                            }
                        }
                    }
                }

                resultados.Add(resultadoPuesto);
            }

            return resultados;
        }

        // --- MÉTODOS DE LECTURA Y RESUMEN ---
        public async Task<IEnumerable<EleccionDto>> ObtenerTodasAsync()
        {
            var elecciones = await _eleccionRepo.GetAllAsync();
            return elecciones.OrderByDescending(e => e.Fecha).Select(e => new EleccionDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Fecha = e.Fecha,
                EstadoEleccion = e.Estado ? 2 : 1 // Ajuste temporal según tu boolean
            }).ToList();
        }

        public async Task<IEnumerable<EleccionDto>> ObtenerResumenPorAnioAsync(int anio)
        {
            var elecciones = await _eleccionRepo.GetAllAsync();
            var eleccionesAnio = elecciones.Where(e => e.Fecha.Year == anio).OrderByDescending(e => e.Estado).ToList();

            var resumen = new List<EleccionDto>();

            // Para los contadores, necesitamos datos históricos o actuales. 
            // El documento dice usar datos actuales
            var todasAsignaciones = await _asignacionRepo.GetAllAsync();
            var todosVotantes = await _votacionCiudadanoRepo.GetAllAsync();

            foreach (var e in eleccionesAnio)
            {
                // Candidatos reales: agrupamos por CandidatoId para no contar duplicados por alianzas
                int candidatosReales = todasAsignaciones.GroupBy(a => a.CandidatoId).Count();
                int partidosParticipantes = todasAsignaciones.GroupBy(a => a.PartidoPoliticoId).Count();
                int ciudadanosVotaron = todosVotantes.Count(v => v.EleccionId == e.Id && v.VotoFinalizado);

                resumen.Add(new EleccionDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha,
                    EstadoEleccion = e.Estado ? 2 : 3, // Simplificación
                    CantidadPartidosParticipantes = partidosParticipantes,
                    CantidadPuestosDisputados = todasAsignaciones.GroupBy(a => a.PuestoElectivoId).Count(),
                    CantidadCiudadanosVotaron = ciudadanosVotaron
                });
            }

            return resumen;
        }

        // --- VALIDACIÓN CRÍTICA DE CONFIGURACIÓN ---
        private async Task ValidarConfiguracionElectoralAsync(string accion)
        {
            var puestos = (await _puestoRepo.GetAllAsync()).Where(p => p.Estado).ToList();
            if (!puestos.Any()) throw new Exception($"No hay puestos electivos activos para {accion} esta elección.");

            var partidos = (await _partidoRepo.GetAllAsync()).Where(p => p.Estado).ToList();
            if (partidos.Count < 2) throw new Exception($"No hay suficientes partidos políticos para {accion} esta elección.");

            var asignaciones = await _asignacionRepo.GetAllAsync();

            var erroresPartidos = new List<string>();

            foreach (var partido in partidos)
            {
                var asignacionesPartido = asignaciones.Where(a => a.PartidoPoliticoId == partido.Id && a.Estado).ToList();
                var puestosFaltantes = new List<string>();

                foreach (var puesto in puestos)
                {
                    bool tieneCandidato = asignacionesPartido.Any(a => a.PuestoElectivoId == puesto.Id);
                    if (!tieneCandidato)
                    {
                        puestosFaltantes.Add(puesto.Nombre);
                    }
                }

                if (puestosFaltantes.Any())
                {
                    erroresPartidos.Add($"El partido político {partido.Nombre} ({partido.Siglas}) no tiene candidatos activos asignados para los siguientes puestos electivos: {string.Join(", ", puestosFaltantes)}.");
                }
            }

            if (erroresPartidos.Any())
            {
                throw new Exception(string.Join("\n", erroresPartidos));
            }
        }
    }
}
