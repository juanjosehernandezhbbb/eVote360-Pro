using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AsignacionCandidatoPuestoService : IAsignacionCandidatoPuestoService
    {
        private readonly IGenericRepository<CandidatoPuesto> _asignacionRepo;
        private readonly IGenericRepository<Candidato> _candidatoRepo;
        private readonly IGenericRepository<PuestoElectivo> _puestoRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;
        private readonly IGenericRepository<SolicitudAlianza> _alianzaRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;

        public AsignacionCandidatoPuestoService(
            IGenericRepository<CandidatoPuesto> asignacionRepo,
            IGenericRepository<Candidato> candidatoRepo,
            IGenericRepository<PuestoElectivo> puestoRepo,
            IGenericRepository<PartidoPolitico> partidoRepo,
            IGenericRepository<SolicitudAlianza> alianzaRepo,
            IGenericRepository<Eleccion> eleccionRepo)
        {
            _asignacionRepo = asignacionRepo;
            _candidatoRepo = candidatoRepo;
            _puestoRepo = puestoRepo;
            _partidoRepo = partidoRepo;
            _alianzaRepo = alianzaRepo;
            _eleccionRepo = eleccionRepo;
        }

        // --- MÉTODO PARA FILTRAR CANDIDATOS (NUEVO) ---
        public async Task<IEnumerable<Candidato>> GetCandidatosDisponiblesAsync(int partidoId)
        {
            // 1. Buscamos todas las alianzas aceptadas
            var todasLasAlianzas = await _alianzaRepo.GetAllAsync();

            // 2. Extraemos los IDs de los partidos con los que tenemos alianza activa
            var aliadosIds = todasLasAlianzas
                .Where(a => a.Estado == (int)EstadoSolicitudAlianza.Aceptada &&
                           (a.PartidoSolicitanteId == partidoId || a.PartidoReceptorId == partidoId))
                .Select(a => a.PartidoSolicitanteId == partidoId ? a.PartidoReceptorId : a.PartidoSolicitanteId)
                .ToList();

            // 3. Traemos todos los candidatos y filtramos
            // Solo permitimos: Los de mi propio partido O Los de partidos aliados
            var todosLosCandidatos = await _candidatoRepo.GetAllAsync();

            return todosLosCandidatos.Where(c =>
                c.PartidoPoliticoId == partidoId ||
                aliadosIds.Contains(c.PartidoPoliticoId)
            ).ToList();
        }

        public async Task AsignarAsync(int candidatoId, int puestoElectivoId, int partidoPoliticoId)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: AsignacionCandidatoPuestoService -> AsignarAsync. PartidoID recibido: {partidoPoliticoId}");

            await ValidarEleccionActivaAsync("asignar candidatos a puestos");

            // 1. Validar candidato y puesto
            var candidato = await _candidatoRepo.GetByIdAsync(candidatoId);
            if (candidato == null || !candidato.Estado)
                throw new Exception("El candidato seleccionado no existe o está inactivo.");

            var puesto = await _puestoRepo.GetByIdAsync(puestoElectivoId);
            if (puesto == null || !puesto.Estado)
                throw new Exception("El puesto electivo seleccionado no existe o está inactivo.");

            // 2. Validar partido
            var partidoDirigente = await _partidoRepo.GetByIdAsync(partidoPoliticoId);
            if (partidoDirigente == null)
                throw new Exception($"El partido con ID {partidoPoliticoId} no fue encontrado.");

            if (!partidoDirigente.Estado)
                throw new Exception("Su partido político se encuentra inactivo actualmente.");

            // 3. Reglas de negocio
            bool puestoOcupadoEnPartido = await _asignacionRepo.ExistsAsync(a => a.PuestoElectivoId == puestoElectivoId && a.PartidoPoliticoId == partidoPoliticoId);
            if (puestoOcupadoEnPartido) throw new Exception("Este puesto electivo ya tiene un candidato asignado dentro del partido.");

            bool candidatoOcupadoEnPartido = await _asignacionRepo.ExistsAsync(a => a.CandidatoId == candidatoId && a.PartidoPoliticoId == partidoPoliticoId);
            if (candidatoOcupadoEnPartido) throw new Exception("Este candidato ya está asignado a un puesto dentro del partido.");

            bool esPropio = candidato.PartidoPoliticoId == partidoPoliticoId;

            if (!esPropio)
            {
                bool hayAlianza = await _alianzaRepo.ExistsAsync(a =>
                    a.Estado == (int)EstadoSolicitudAlianza.Aceptada &&
                    ((a.PartidoSolicitanteId == partidoPoliticoId && a.PartidoReceptorId == candidato.PartidoPoliticoId) ||
                     (a.PartidoSolicitanteId == candidato.PartidoPoliticoId && a.PartidoReceptorId == partidoPoliticoId)));

                if (!hayAlianza) throw new Exception("No existe una alianza vigente con el partido de este candidato.");


            }

            var nuevaAsignacion = new CandidatoPuesto
            {
                CandidatoId = candidatoId,
                PuestoElectivoId = puestoElectivoId,
                PartidoPoliticoId = partidoPoliticoId,
                Nombre = $"{candidato.Nombre} - {puesto.Nombre}",
                Estado = true
            };

            await _asignacionRepo.AddAsync(nuevaAsignacion);
        }

        public async Task EliminarAsignacionAsync(int asignacionId, int partidoPoliticoId)
        {
            await ValidarEleccionActivaAsync("eliminar una asignación");

            var asignacion = await _asignacionRepo.GetByIdAsync(asignacionId);

            if (asignacion == null)
                throw new Exception("La asignación no existe.");

            if (asignacion.PartidoPoliticoId != partidoPoliticoId)
                throw new Exception("No tiene permisos.");

            await _asignacionRepo.RemoveAsync(asignacion);
        }

        public async Task<IEnumerable<CandidatoPuestoDto>> ObtenerAsignacionesPorPartidoAsync(int partidoId)
        {
            var todas = await _asignacionRepo.GetAllWithIncludesAsync(
                x => x.Candidato,
                x => x.PuestoElectivo
            );

            var asignacionesDelPartido = todas.Where(a => a.PartidoPoliticoId == partidoId).ToList();

            return asignacionesDelPartido.Select(a => new CandidatoPuestoDto
            {
                Id = a.Id,
                CandidatoId = a.CandidatoId,
                PuestoElectivoId = a.PuestoElectivoId,
                NombreCandidato = a.Candidato?.Nombre ?? "N/A",
                ApellidoCandidato = a.Candidato?.Apellido ?? "N/A",
                NombrePuesto = a.PuestoElectivo?.Nombre ?? "N/A",
                PartidoOrigenCandidato = a.Candidato?.PartidoPolitico?.Nombre ?? "N/A",
                TipoCandidatura = (a.Candidato != null && a.Candidato.PartidoPoliticoId == a.PartidoPoliticoId) ? "Propio" : "Aliado"
            }).ToList();
        }

        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                // Lógica de validación de elección
            }
        }
    }
}