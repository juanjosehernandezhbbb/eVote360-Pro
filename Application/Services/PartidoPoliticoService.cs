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
    public class PartidoPoliticoService : IPartidoPoliticoService
    {
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<Candidato> _candidatoRepo;
        private readonly IGenericRepository<DirigentePolitico> _dirigenteRepo;
        private readonly IGenericRepository<CandidatoPuesto> _candidatoPuestoRepo;

        public PartidoPoliticoService(
            IGenericRepository<PartidoPolitico> partidoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<Candidato> candidatoRepo,
            IGenericRepository<DirigentePolitico> dirigenteRepo,
            IGenericRepository<CandidatoPuesto> candidatoPuestoRepo)
        {
            _partidoRepo = partidoRepo;
            _eleccionRepo = eleccionRepo;
            _candidatoRepo = candidatoRepo;
            _dirigenteRepo = dirigenteRepo;
            _candidatoPuestoRepo = candidatoPuestoRepo;
        }

        public async Task CrearAsync(PartidoPoliticoDto dto)
        {
            await ValidarEleccionActivaAsync("crear");

            var siglasLimpias = dto.Siglas.Trim().ToUpper(); // Regla: Siglas preferiblemente en mayúscula

            bool existeSiglas = await _partidoRepo.ExistsAsync(p => p.Siglas.ToUpper() == siglasLimpias);
            if (existeSiglas) throw new Exception("Ya existe un partido político registrado con estas siglas.");

            var nuevoPartido = new PartidoPolitico
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? "" : dto.Descripcion.Trim(),
                Siglas = siglasLimpias,
                LogoUrl = dto.LogoUrl,
                Estado = true // Por defecto activo
            };

            await _partidoRepo.AddAsync(nuevoPartido);
        }

        public async Task ActualizarAsync(PartidoPoliticoDto dto)
        {
            await ValidarEleccionActivaAsync("editar");

            var partidoExistente = await _partidoRepo.GetByIdAsync(dto.Id);
            if (partidoExistente == null) throw new Exception("Partido político no encontrado.");

            var siglasLimpias = dto.Siglas.Trim().ToUpper();

            // Verificar si el partido ya participó en una elección (si tiene candidatos asignados y hubo elección activa/finalizada)
            // Lógica simplificada: Verificamos si tiene asignaciones que hayan participado
            bool participoEnEleccion = await _candidatoPuestoRepo.ExistsAsync(cp => cp.PartidoPoliticoId == dto.Id)
                                       && await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa || e.EstadoEleccion == EstadoEleccion.Finalizada);

            // Validar cambios críticos si ya participó
            if (participoEnEleccion)
            {
                if (!partidoExistente.Siglas.Equals(siglasLimpias, StringComparison.OrdinalIgnoreCase) ||
                    !partidoExistente.Nombre.Equals(dto.Nombre.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(dto.LogoUrl) && partidoExistente.LogoUrl != dto.LogoUrl))
                {
                    throw new Exception("No se pueden modificar los datos principales (Nombre, Siglas, Logo) de este partido político porque ya participó en una elección.");
                }
            }
            else
            {
                // Si no ha participado, podemos cambiar las siglas pero validando que no existan
                if (!partidoExistente.Siglas.Equals(siglasLimpias, StringComparison.OrdinalIgnoreCase))
                {
                    bool existeSiglas = await _partidoRepo.ExistsAsync(p => p.Id != dto.Id && p.Siglas.ToUpper() == siglasLimpias);
                    if (existeSiglas) throw new Exception("Ya existe un partido político registrado con estas siglas.");

                    partidoExistente.Siglas = siglasLimpias;
                }

                partidoExistente.Nombre = dto.Nombre.Trim();
                // Si mandan un nuevo logo en la edición, lo actualizamos. Si no, conserva el actual.
                if (!string.IsNullOrEmpty(dto.LogoUrl))
                {
                    partidoExistente.LogoUrl = dto.LogoUrl;
                }
            }

            // La descripción y el estado siempre se pueden editar si no hay elección activa
            partidoExistente.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? "" : dto.Descripcion.Trim();
            partidoExistente.Estado = dto.Estado;

            await _partidoRepo.UpdateAsync(partidoExistente);
        }

        public async Task CambiarEstadoAsync(int id, bool nuevoEstado)
        {
            string accion = nuevoEstado ? "activar" : "desactivar";
            await ValidarEleccionActivaAsync(accion);

            var partidoExistente = await _partidoRepo.GetByIdAsync(id);
            if (partidoExistente == null) throw new Exception("Partido político no encontrado.");

            if (partidoExistente.Estado == nuevoEstado)
            {
                string mensajeEstado = nuevoEstado ? "activo" : "inactivo";
                throw new Exception($"Este partido político ya se encuentra {mensajeEstado}.");
            }

            // Validaciones fuertes al desactivar
            if (!nuevoEstado)
            {
                // Regla: No desactivar si tiene candidatos activos
                bool tieneCandidatosActivos = await _candidatoRepo.ExistsAsync(c => c.PartidoPoliticoId == id && c.Estado == true);
                if (tieneCandidatosActivos) throw new Exception("No se puede desactivar este partido político porque tiene candidatos activos registrados.");

                // Regla: No desactivar si tiene dirigente asignado
                bool tieneDirigente = await _dirigenteRepo.ExistsAsync(d => d.PartidoPoliticoId == id);
                if (tieneDirigente) throw new Exception("No se puede desactivar este partido político porque tiene un dirigente político asignado.");
            }

            partidoExistente.Estado = nuevoEstado;
            await _partidoRepo.UpdateAsync(partidoExistente);
        }

        public async Task<IEnumerable<PartidoPoliticoDto>> ObtenerTodosAsync()
        {
            var partidos = await _partidoRepo.GetAllAsync();
            return partidos.Select(p => new PartidoPoliticoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Siglas = p.Siglas,
                LogoUrl = p.LogoUrl,
                Estado = p.Estado
            }).ToList();
        }

        public async Task<PartidoPoliticoDto> ObtenerPorIdAsync(int id)
        {
            var p = await _partidoRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new PartidoPoliticoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Siglas = p.Siglas,
                LogoUrl = p.LogoUrl,
                Estado = p.Estado
            };
        }

        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                throw new Exception($"No se puede {accion} un partido político mientras exista una elección activa.");
            }
        }
    }
}
