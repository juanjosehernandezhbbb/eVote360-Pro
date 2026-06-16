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
    public class CandidatoService : ICandidatoService
    {
        private readonly IGenericRepository<Candidato> _candidatoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<CandidatoPuesto> _candidatoPuestoRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;

        public CandidatoService(
            IGenericRepository<Candidato> candidatoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<CandidatoPuesto> candidatoPuestoRepo,
            IGenericRepository<PartidoPolitico> partidoRepo)
        {
            _candidatoRepo = candidatoRepo;
            _eleccionRepo = eleccionRepo;
            _candidatoPuestoRepo = candidatoPuestoRepo;
            _partidoRepo = partidoRepo;
        }

        public async Task CrearAsync(CandidatoDto dto)
        {
            await ValidarEleccionActivaAsync("crear");

            // Validar que el partido exista y esté activo
            var partido = await _partidoRepo.GetByIdAsync(dto.PartidoPoliticoId);
            if (partido == null || !partido.Estado)
            {
                throw new Exception("El partido político seleccionado no existe o se encuentra inactivo.");
            }

            var nuevoCandidato = new Candidato
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                FotoUrl = dto.FotoUrl,
                PartidoPoliticoId = dto.PartidoPoliticoId,
                Estado = true // Activo por defecto
            };

            await _candidatoRepo.AddAsync(nuevoCandidato);
        }

        public async Task ActualizarAsync(CandidatoDto dto)
        {
            // 1. Validar reglas de negocio
            await ValidarEleccionActivaAsync("editar");

            // 2. Obtener el candidato
            var candidatoExistente = await _candidatoRepo.GetByIdAsync(dto.Id);
            if (candidatoExistente == null) throw new Exception("Candidato no encontrado.");

            // 3. Aplicar cambios a la entidad recuperada (esto marca la entidad como modificada en EF)
            if (candidatoExistente.PartidoPoliticoId != dto.PartidoPoliticoId)
            {
                bool participoEnEleccion = await _candidatoPuestoRepo.ExistsAsync(cp => cp.CandidatoId == dto.Id)
                                           && await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa || e.EstadoEleccion == EstadoEleccion.Finalizada);

                if (participoEnEleccion)
                    throw new Exception("No se puede cambiar el partido político...");

                var nuevoPartido = await _partidoRepo.GetByIdAsync(dto.PartidoPoliticoId);
                if (nuevoPartido == null || !nuevoPartido.Estado)
                    throw new Exception("El nuevo partido político seleccionado no existe o está inactivo.");

                candidatoExistente.PartidoPoliticoId = dto.PartidoPoliticoId;
            }

            candidatoExistente.Nombre = dto.Nombre.Trim();
            candidatoExistente.Apellido = dto.Apellido.Trim();

            if (!string.IsNullOrEmpty(dto.FotoUrl))
            {
                candidatoExistente.FotoUrl = dto.FotoUrl;
            }

            candidatoExistente.Estado = dto.Estado;

            // 4. MANTENER EL UPDATE PERO ASEGURAR QUE EF DETECTE EL CAMBIO
            // Como estamos trabajando sobre la misma instancia que obtuvimos de la DB, 
            // el contexto ya la está rastreando. 
            await _candidatoRepo.UpdateAsync(candidatoExistente);
        }

        public async Task CambiarEstadoAsync(int id, bool nuevoEstado)
        {
            string accion = nuevoEstado ? "activar" : "desactivar";
            await ValidarEleccionActivaAsync(accion);

            var candidatoExistente = await _candidatoRepo.GetByIdAsync(id);
            if (candidatoExistente == null) throw new Exception("Candidato no encontrado.");

            if (candidatoExistente.Estado == nuevoEstado)
            {
                string mensajeEstado = nuevoEstado ? "activo" : "inactivo";
                throw new Exception($"Este candidato ya se encuentra {mensajeEstado}.");
            }

            candidatoExistente.Estado = nuevoEstado;
            await _candidatoRepo.UpdateAsync(candidatoExistente);
        }

        public async Task<IEnumerable<CandidatoDto>> ObtenerPorPartidoAsync(int partidoId)
        {
            var todos = await _candidatoRepo.GetAllAsync();

            var candidatos = (partidoId == 0)
                ? todos
                : todos.Where(c => c.PartidoPoliticoId == partidoId);

            return candidatos.Select(c => new CandidatoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                FotoUrl = c.FotoUrl,
                PartidoPoliticoId = c.PartidoPoliticoId,
                Estado = c.Estado
            }).ToList();
        }

        public async Task<CandidatoDto> ObtenerPorIdAsync(int id)
        {
            var c = await _candidatoRepo.GetByIdAsync(id);
            if (c == null) return null;

            return new CandidatoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                FotoUrl = c.FotoUrl,
                PartidoPoliticoId = c.PartidoPoliticoId,
                Estado = c.Estado
            };
        }


        public async Task<IEnumerable<CandidatoDto>> ObtenerTodosAsync()
        {
            var lista = await _candidatoRepo.GetAllAsync();
            return lista.Select(c => new CandidatoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                FotoUrl = c.FotoUrl,
                PartidoPoliticoId = c.PartidoPoliticoId,
                Estado = c.Estado
            }).ToList();
        }

        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                throw new Exception($"No se puede {accion} un candidato mientras exista una elección activa.");
            }
        }
    }
}
