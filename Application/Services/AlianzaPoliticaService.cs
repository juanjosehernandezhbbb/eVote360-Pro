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
    public class AlianzaPoliticaService : IAlianzaPoliticaService
    {
        private readonly IGenericRepository<SolicitudAlianza> _alianzaRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;
        private readonly IGenericRepository<CandidatoPuesto> _candidatoPuestoRepo;

        public AlianzaPoliticaService(
            IGenericRepository<SolicitudAlianza> alianzaRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<PartidoPolitico> partidoRepo,
            IGenericRepository<CandidatoPuesto> candidatoPuestoRepo)
        {
            _alianzaRepo = alianzaRepo;
            _eleccionRepo = eleccionRepo;
            _partidoRepo = partidoRepo;
            _candidatoPuestoRepo = candidatoPuestoRepo;
        }

        public async Task CrearSolicitudAsync(int partidoSolicitanteId, int partidoReceptorId)
        {
            await ValidarEleccionActivaAsync("crear una solicitud de alianza");

            if (partidoSolicitanteId == partidoReceptorId)
                throw new Exception("No puede crear una solicitud de alianza hacia su propio partido político.");

            var partidoReceptor = await _partidoRepo.GetByIdAsync(partidoReceptorId);
            if (partidoReceptor == null || !partidoReceptor.Estado)
                throw new Exception("No puede crear una solicitud de alianza con un partido político inactivo.");

            // Validar que no exista alianza vigente
            bool existeAlianza = await _alianzaRepo.ExistsAsync(a =>
                a.Estado == (int)EstadoSolicitudAlianza.Aceptada &&
                ((a.PartidoSolicitanteId == partidoSolicitanteId && a.PartidoReceptorId == partidoReceptorId) ||
                 (a.PartidoSolicitanteId == partidoReceptorId && a.PartidoReceptorId == partidoSolicitanteId)));

            if (existeAlianza) throw new Exception("Ya existe una alianza vigente con este partido político.");

            // Validar que no exista solicitud pendiente enviada por el solicitante
            bool existePendienteEnviada = await _alianzaRepo.ExistsAsync(a =>
                a.Estado == (int)EstadoSolicitudAlianza.Pendiente &&
                a.PartidoSolicitanteId == partidoSolicitanteId && a.PartidoReceptorId == partidoReceptorId);

            if (existePendienteEnviada) throw new Exception("Ya existe una solicitud de alianza pendiente enviada a este partido político.");

            // Validar que no exista solicitud pendiente enviada por el receptor
            bool existePendienteRecibida = await _alianzaRepo.ExistsAsync(a =>
                a.Estado == (int)EstadoSolicitudAlianza.Pendiente &&
                a.PartidoSolicitanteId == partidoReceptorId && a.PartidoReceptorId == partidoSolicitanteId);

            if (existePendienteRecibida) throw new Exception("Ya existe una solicitud de alianza pendiente enviada por este partido político.");

            var nuevaSolicitud = new SolicitudAlianza
            {
                PartidoSolicitanteId = partidoSolicitanteId,
                PartidoReceptorId = partidoReceptorId,
                Estado = (int)EstadoSolicitudAlianza.Pendiente,
                FechaSolicitud = DateTime.Now
            };

            await _alianzaRepo.AddAsync(nuevaSolicitud);
        }

        public async Task AceptarSolicitudAsync(int solicitudId, int partidoUsuarioId)
        {
            await ValidarEleccionActivaAsync("aceptar una solicitud de alianza");

            var solicitud = await _alianzaRepo.GetByIdAsync(solicitudId);
            if (solicitud == null) throw new Exception("La solicitud de alianza seleccionada no existe.");

            if (solicitud.PartidoReceptorId != partidoUsuarioId)
                throw new Exception("No tiene permisos para responder esta solicitud de alianza.");

            if (solicitud.Estado != (int)EstadoSolicitudAlianza.Pendiente)
                throw new Exception("Esta solicitud de alianza ya fue respondida.");

            bool existeAlianza = await _alianzaRepo.ExistsAsync(a =>
                a.Estado == (int)EstadoSolicitudAlianza.Aceptada &&
                ((a.PartidoSolicitanteId == solicitud.PartidoSolicitanteId && a.PartidoReceptorId == solicitud.PartidoReceptorId) ||
                 (a.PartidoSolicitanteId == solicitud.PartidoReceptorId && a.PartidoReceptorId == solicitud.PartidoSolicitanteId)));

            if (existeAlianza) throw new Exception("Ya existe una alianza vigente con este partido político.");

            solicitud.Estado = (int)EstadoSolicitudAlianza.Aceptada;
            solicitud.FechaRespuesta = DateTime.Now;

            await _alianzaRepo.UpdateAsync(solicitud);
        }

        public async Task RechazarSolicitudAsync(int solicitudId, int partidoUsuarioId)
        {
            await ValidarEleccionActivaAsync("rechazar una solicitud de alianza");

            var solicitud = await _alianzaRepo.GetByIdAsync(solicitudId);
            if (solicitud == null) throw new Exception("La solicitud de alianza seleccionada no existe.");

            if (solicitud.PartidoReceptorId != partidoUsuarioId)
                throw new Exception("No tiene permisos para responder esta solicitud de alianza.");

            if (solicitud.Estado != (int)EstadoSolicitudAlianza.Pendiente)
                throw new Exception("Esta solicitud de alianza ya fue respondida.");

            solicitud.Estado = (int)EstadoSolicitudAlianza.Rechazada;
            solicitud.FechaRespuesta = DateTime.Now;

            await _alianzaRepo.UpdateAsync(solicitud);
        }

        public async Task EliminarSolicitudAsync(int solicitudId, int partidoUsuarioId)
        {
            await ValidarEleccionActivaAsync("eliminar una solicitud de alianza");

            var solicitud = await _alianzaRepo.GetByIdAsync(solicitudId);
            if (solicitud == null) throw new Exception("La solicitud de alianza seleccionada no existe o ya fue eliminada.");

            if (solicitud.PartidoSolicitanteId != partidoUsuarioId)
                throw new Exception("No tiene permisos para eliminar esta solicitud de alianza.");

            if (solicitud.Estado == (int)EstadoSolicitudAlianza.Aceptada)
                throw new Exception("No se puede eliminar una solicitud aceptada porque ya generó una alianza vigente. Para terminarla debe eliminar la alianza desde el listado de alianzas vigentes.");

            throw new NotImplementedException("Falta método RemoveAsync en el repositorio genérico.");
        }

        public async Task EliminarAlianzaVigenteAsync(int alianzaId, int partidoUsuarioId)
        {
            await ValidarEleccionActivaAsync("eliminar una alianza política");

            var alianza = await _alianzaRepo.GetByIdAsync(alianzaId);
            if (alianza == null || alianza.Estado != (int)EstadoSolicitudAlianza.Aceptada)
                throw new Exception("La alianza política seleccionada no existe o ya fue eliminada.");

            if (alianza.PartidoSolicitanteId != partidoUsuarioId && alianza.PartidoReceptorId != partidoUsuarioId)
                throw new Exception("No tiene permisos para eliminar esta alianza política.");

            // Regla: No se puede eliminar si hay candidatos aliados asignados
            int elOtroPartidoId = alianza.PartidoSolicitanteId == partidoUsuarioId ? alianza.PartidoReceptorId : alianza.PartidoSolicitanteId;

            // Busca si hay algún candidato del partidoUsuarioId ocupando un puesto en elOtroPartidoId (o viceversa)
            bool hayAsignacionesCruzadas = await _candidatoPuestoRepo.ExistsAsync(cp =>
                (cp.PartidoPoliticoId == partidoUsuarioId && cp.Candidato.PartidoPoliticoId == elOtroPartidoId) ||
                (cp.PartidoPoliticoId == elOtroPartidoId && cp.Candidato.PartidoPoliticoId == partidoUsuarioId));

            if (hayAsignacionesCruzadas)
                throw new Exception("No se puede eliminar esta alianza porque existen candidatos aliados asignados entre estos partidos. Primero deben eliminarse las asignaciones correspondientes.");

            // Si tu repositorio no tiene DeleteAsync/RemoveAsync, debes crearlo.
            throw new NotImplementedException("Falta método RemoveAsync en el repositorio genérico.");
        }

        // --- Métodos de Lectura ---
        public async Task<IEnumerable<SolicitudAlianzaDto>> ObtenerSolicitudesPendientesAsync(int partidoId)
        {
            var todas = await _alianzaRepo.GetAllAsync();
            return todas.Where(a => a.PartidoReceptorId == partidoId && a.Estado == (int)EstadoSolicitudAlianza.Pendiente)
                        .Select(MapearADto).ToList();
        }

        public async Task<IEnumerable<SolicitudAlianzaDto>> ObtenerSolicitudesRealizadasAsync(int partidoId)
        {
            var todas = await _alianzaRepo.GetAllAsync();
            return todas.Where(a => a.PartidoSolicitanteId == partidoId)
                        .Select(MapearADto).ToList();
        }

        public async Task<IEnumerable<SolicitudAlianzaDto>> ObtenerAlianzasVigentesAsync(int partidoId)
        {
            var todas = await _alianzaRepo.GetAllAsync();
            return todas.Where(a => (a.PartidoSolicitanteId == partidoId || a.PartidoReceptorId == partidoId) && a.Estado == (int)EstadoSolicitudAlianza.Aceptada)
                        .Select(MapearADto).ToList();
        }

        private SolicitudAlianzaDto MapearADto(SolicitudAlianza a)
        {
            return new SolicitudAlianzaDto
            {
                Id = a.Id,
                PartidoSolicitanteId = a.PartidoSolicitanteId,
                PartidoReceptorId = a.PartidoReceptorId,
                Estado = a.Estado,
                FechaSolicitud = a.FechaSolicitud,
                FechaRespuesta = a.FechaRespuesta,
                // Nota: Tu repositorio necesitaría usar .Include() para llenar estos nombres
                NombrePartidoSolicitante = a.PartidoSolicitante != null ? a.PartidoSolicitante.Nombre : "N/A",
                SiglasPartidoSolicitante = a.PartidoSolicitante != null ? a.PartidoSolicitante.Siglas : "N/A",
                NombrePartidoReceptor = a.PartidoReceptor != null ? a.PartidoReceptor.Nombre : "N/A",
                SiglasPartidoReceptor = a.PartidoReceptor != null ? a.PartidoReceptor.Siglas : "N/A"
            };
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
