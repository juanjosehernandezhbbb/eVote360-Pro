using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Repositories;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PuestoElectivoService : IPuestoElectivoService
    {
        private readonly IGenericRepository<PuestoElectivo> _puestoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<CandidatoPuesto> _candidatoPuestoRepo;

        public PuestoElectivoService(
            IGenericRepository<PuestoElectivo> puestoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<CandidatoPuesto> candidatoPuestoRepo)
        {
            _puestoRepo = puestoRepo;
            _eleccionRepo = eleccionRepo;
            _candidatoPuestoRepo = candidatoPuestoRepo;
        }

        public async Task CrearAsync(PuestoElectivoDto dto)
        {
            await ValidarEleccionActivaAsync("crear");

            var nombreLimpio = dto.Nombre.Trim();

            bool existeNombre = await _puestoRepo.ExistsAsync(p => p.Nombre.Trim().ToLower() == nombreLimpio.ToLower());
            if (existeNombre)
            {
                throw new Exception("Ya existe un puesto electivo registrado con este nombre.");
            }

            var nuevoPuesto = new PuestoElectivo
            {
                Nombre = nombreLimpio,
                Descripcion = dto.Descripcion.Trim(),
                Estado = true 
            };

            await _puestoRepo.AddAsync(nuevoPuesto);
        }

        public async Task ActualizarAsync(PuestoElectivoDto dto)
        {
            await ValidarEleccionActivaAsync("editar");

            var puestoExistente = await _puestoRepo.GetByIdAsync(dto.Id);
            if (puestoExistente == null) throw new Exception("Puesto no encontrado.");

            var nombreLimpio = dto.Nombre.Trim();

            // Validar si el nombre cambió y si ya existe otro con ese nombre
            if (!puestoExistente.Nombre.Equals(nombreLimpio, StringComparison.OrdinalIgnoreCase))
            {
                bool existeNombre = await _puestoRepo.ExistsAsync(p => p.Id != dto.Id && p.Nombre.Trim().ToLower() == nombreLimpio.ToLower());
                if (existeNombre)
                {
                    throw new Exception("Ya existe un puesto electivo registrado con este nombre.");
                }

               
                bool participoEnEleccion = puestoExistente.Detalles != null && puestoExistente.Detalles.Any(); 
                if (participoEnEleccion)
                {
                    throw new Exception("No se puede modificar el nombre de este puesto electivo porque ya fue utilizado en una elección.");
                }

                puestoExistente.Nombre = nombreLimpio;
            }

            puestoExistente.Descripcion = dto.Descripcion.Trim();
            puestoExistente.Estado = dto.Estado;

            await _puestoRepo.UpdateAsync(puestoExistente);
        }

        public async Task CambiarEstadoAsync(int id, bool nuevoEstado)
        {
            string accion = nuevoEstado ? "activar" : "desactivar";
            await ValidarEleccionActivaAsync(accion);

            var puestoExistente = await _puestoRepo.GetByIdAsync(id);
            if (puestoExistente == null) throw new Exception("Puesto no encontrado.");

            if (puestoExistente.Estado == nuevoEstado)
            {
                string mensajeEstado = nuevoEstado ? "activo" : "inactivo";
                throw new Exception($"Este puesto electivo ya se encuentra {mensajeEstado}.");
            }

            
            if (!nuevoEstado)
            {
                bool tieneCandidatos = await _candidatoPuestoRepo.ExistsAsync(cp => cp.PuestoElectivoId == id && cp.Candidato.Estado == true);
                if (tieneCandidatos)
                {
                    throw new Exception("No se puede desactivar este puesto electivo porque tiene candidatos activos asignados.");
                }
            }

            puestoExistente.Estado = nuevoEstado;
            await _puestoRepo.UpdateAsync(puestoExistente);
        }

        // Métodos de lectura (Get)
        public async Task<IEnumerable<PuestoElectivoDto>> ObtenerTodosAsync()
        {
            var puestos = await _puestoRepo.GetAllAsync();
            
            return puestos.Select(p => new PuestoElectivoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Estado = p.Estado
            }).ToList();
        }

        public async Task<PuestoElectivoDto> ObtenerPorIdAsync(int id)
        {
            var p = await _puestoRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new PuestoElectivoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Estado = p.Estado
            };
        }

        // Metodo privado reutilizable
        private async Task ValidarEleccionActivaAsync(string accion)
        {
            bool hayEleccionActiva = await _eleccionRepo.ExistsAsync(e => e.EstadoEleccion == EstadoEleccion.Activa);
            if (hayEleccionActiva)
            {
                throw new Exception($"No se puede {accion} un puesto electivo mientras exista una elección activa.");
            }
        }
    }
}
