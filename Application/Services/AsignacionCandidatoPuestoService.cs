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

        public async Task AsignarAsync(int candidatoId, int puestoElectivoId, int partidoPoliticoId)
        {
            await ValidarEleccionActivaAsync("asignar candidatos a puestos");

            // 1. Validar que el candidato y el puesto existan y estén activos
            var candidato = await _candidatoRepo.GetByIdAsync(candidatoId);
            if (candidato == null || !candidato.Estado) throw new Exception("El candidato seleccionado no existe o está inactivo.");

            var puesto = await _puestoRepo.GetByIdAsync(puestoElectivoId);
            if (puesto == null || !puesto.Estado) throw new Exception("El puesto electivo seleccionado no existe o está inactivo.");

            // 2. Validar que el partido del dirigente esté activo
            var partidoDirigente = await _partidoRepo.GetByIdAsync(partidoPoliticoId);
            if (partidoDirigente == null || !partidoDirigente.Estado) throw new Exception("Su partido político se encuentra inactivo.");

            // 3. Regla: Un partido no puede tener más de un candidato asignado al mismo puesto
            bool puestoOcupadoEnPartido = await _asignacionRepo.ExistsAsync(a => a.PuestoElectivoId == puestoElectivoId && a.PartidoPoliticoId == partidoPoliticoId);
            if (puestoOcupadoEnPartido) throw new Exception("Este puesto electivo ya tiene un candidato asignado dentro del partido.");

            // 4. Regla: Un candidato no puede aspirar a más de un puesto dentro del mismo partido
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

 
                var todasLasAsignaciones = await _asignacionRepo.GetAllAsync();
                var asignacionOrigen = todasLasAsignaciones.FirstOrDefault(a => a.CandidatoId == candidatoId && a.PartidoPoliticoId == candidato.PartidoPoliticoId);

                if (asignacionOrigen == null) throw new Exception("Este candidato aliado no tiene un puesto asignado en su partido de origen.");

                if (asignacionOrigen.PuestoElectivoId != puestoElectivoId) throw new Exception("Este candidato en su partido de origen aspira a un puesto diferente al seleccionado.");
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
            if (asignacion == null) throw new Exception("La asignación seleccionada no existe o ya fue eliminada.");

            if (asignacion.PartidoPoliticoId != partidoPoliticoId) throw new Exception("No tiene permisos para eliminar esta asignación.");

            throw new NotImplementedException("Falta método RemoveAsync en el repositorio genérico para borrar la relación de CandidatoPuesto.");
        }

        public async Task<IEnumerable<CandidatoPuestoDto>> ObtenerAsignacionesPorPartidoAsync(int partidoId)
        {
            var todas = await _asignacionRepo.GetAllAsync();
            var asignacionesDelPartido = todas.Where(a => a.PartidoPoliticoId == partidoId).ToList();

            return asignacionesDelPartido.Select(a => new CandidatoPuestoDto
            {
                Id = a.Id,
                CandidatoId = a.CandidatoId,
                PuestoElectivoId = a.PuestoElectivoId,
                PartidoPoliticoId = a.PartidoPoliticoId,

                NombreCandidato = a.Candidato != null ? a.Candidato.Nombre : "N/A",
                ApellidoCandidato = a.Candidato != null ? a.Candidato.Apellido : "N/A",
                NombrePuesto = a.PuestoElectivo != null ? a.PuestoElectivo.Nombre : "N/A",
                PartidoOrigenCandidato = a.Candidato != null && a.Candidato.PartidoPolitico != null ? a.Candidato.PartidoPolitico.Nombre : "N/A",

                TipoCandidatura = (a.Candidato != null && a.Candidato.PartidoPoliticoId == a.PartidoPoliticoId) ? "Propio" : "Aliado"
            }).ToList();
        }

        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                throw new Exception($"No se puede {accion} mientras exista una elección activa.");
            }
        }
    }
}
