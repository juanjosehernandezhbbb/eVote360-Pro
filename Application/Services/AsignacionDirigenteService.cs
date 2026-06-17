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
    public class AsignacionDirigenteService : IAsignacionDirigenteService
    {
        private readonly IGenericRepository<DirigentePolitico> _asignacionRepo;
        private readonly IGenericRepository<Usuario> _usuarioRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;

        public AsignacionDirigenteService(
            IGenericRepository<DirigentePolitico> asignacionRepo,
            IGenericRepository<Usuario> usuarioRepo,
            IGenericRepository<PartidoPolitico> partidoRepo)
        {
            _asignacionRepo = asignacionRepo;
            _usuarioRepo = usuarioRepo;
            _partidoRepo = partidoRepo;
        }

        public async Task<IEnumerable<AsignacionDirigenteDto>> ObtenerTodosAsync()
        {
            // CAMBIO CLAVE: Usamos GetAllWithIncludesAsync para que traiga la información de las tablas vinculadas
            var asignaciones = await _asignacionRepo.GetAllWithIncludesAsync(
                x => x.Usuario,
                x => x.PartidoPolitico
            );

            return asignaciones.Select(a => new AsignacionDirigenteDto
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId,
                PartidoPoliticoId = a.PartidoPoliticoId,
                NombreDirigente = a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}" : "N/A",
                NombreUsuario = a.Usuario != null ? a.Usuario.NombreUsuario : "N/A",
                NombrePartido = a.PartidoPolitico != null ? a.PartidoPolitico.Nombre : "N/A",
                SiglasPartido = a.PartidoPolitico != null ? a.PartidoPolitico.Siglas : "N/A",
                EstadoDirigente = a.Usuario != null && a.Usuario.Estado,
                EstadoPartido = a.PartidoPolitico != null && a.PartidoPolitico.Estado
            }).ToList();
        }

        public async Task AsignarAsync(int usuarioId, int partidoPoliticoId)
        {
            // 1. Validar que el usuario existe, está activo y es Dirigente
            var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
            if (usuario == null || !usuario.Estado || usuario.Rol != (int)RolUsuario.DirigentePolitico)
            {
                throw new Exception("El usuario seleccionado no existe, está inactivo o no tiene el rol de Dirigente Político.");
            }

            // 2. Validar que el partido existe y está activo
            var partido = await _partidoRepo.GetByIdAsync(partidoPoliticoId);
            if (partido == null || !partido.Estado)
            {
                throw new Exception("El partido político seleccionado no existe o se encuentra inactivo.");
            }

            // 3. Validar que el usuario no tenga ya un partido asignado
            bool usuarioOcupado = await _asignacionRepo.ExistsAsync(a => a.UsuarioId == usuarioId);
            if (usuarioOcupado) throw new Exception("Este dirigente político ya tiene un partido asignado.");

            // 4. Validar que el partido no tenga ya un dirigente asignado
            bool partidoOcupado = await _asignacionRepo.ExistsAsync(a => a.PartidoPoliticoId == partidoPoliticoId);
            if (partidoOcupado) throw new Exception("Este partido político ya tiene un dirigente asignado.");

            var nuevaAsignacion = new DirigentePolitico
            {
                UsuarioId = usuarioId,
                PartidoPoliticoId = partidoPoliticoId
            };

            await _asignacionRepo.AddAsync(nuevaAsignacion);
        }

        public async Task EliminarAsignacionAsync(int asignacionId)
        {
            var asignacion = await _asignacionRepo.GetByIdAsync(asignacionId);
            if (asignacion == null) throw new Exception("Asignación no encontrada.");

            // CORRECCIÓN: Implementación del método de eliminación usando tu repositorio
            await _asignacionRepo.RemoveAsync(asignacion);
        }
    }
}