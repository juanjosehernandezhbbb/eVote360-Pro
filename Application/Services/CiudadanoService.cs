using Application.Dtos;
using Application.Interfaces;
using Core.Entities; 
using Core.Enums;   
using Core.Repositories; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CiudadanoService : ICiudadanoService
    {
        private readonly IGenericRepository<Ciudadano> _ciudadanoRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;
        private readonly IGenericRepository<VotacionCiudadano> _votacionCiudadanoRepo;

        public CiudadanoService(
            IGenericRepository<Ciudadano> ciudadanoRepo,
            IGenericRepository<Eleccion> eleccionRepo,
            IGenericRepository<VotacionCiudadano> votacionCiudadanoRepo)
        {
            _ciudadanoRepo = ciudadanoRepo;
            _eleccionRepo = eleccionRepo;
            _votacionCiudadanoRepo = votacionCiudadanoRepo;
        }

        public async Task CrearAsync(CiudadanoDto dto)
        {
            await ValidarEleccionActivaAsync("crear");

            ValidarFormatoCorreo(dto.Email);

            var documentoLimpio = dto.NumeroDocumento.Trim();
            var correoLimpio = dto.Email.Trim().ToLower();

            // Regla: Documento único
            bool existeDocumento = await _ciudadanoRepo.ExistsAsync(c => c.NumeroDocumento == documentoLimpio);
            if (existeDocumento) throw new Exception("Ya existe un ciudadano registrado con este número de documento de identidad.");

            // Regla: Correo único
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

        public async Task ActualizarAsync(CiudadanoDto dto)
        {
            await ValidarEleccionActivaAsync("editar");

            var ciudadanoExistente = await _ciudadanoRepo.GetByIdAsync(dto.Id);
            if (ciudadanoExistente == null) throw new Exception("Ciudadano no encontrado.");

            ValidarFormatoCorreo(dto.Email);

            var documentoLimpio = dto.NumeroDocumento.Trim();
            var correoLimpio = dto.Email.Trim().ToLower();

            // Validar si cambió el correo y si ya existe
            if (!ciudadanoExistente.Email.Equals(correoLimpio, StringComparison.OrdinalIgnoreCase))
            {
                bool existeCorreo = await _ciudadanoRepo.ExistsAsync(c => c.Id != dto.Id && c.Email.ToLower() == correoLimpio);
                if (existeCorreo) throw new Exception("Ya existe un ciudadano registrado con este correo electrónico.");

                ciudadanoExistente.Email = correoLimpio;
            }

            // Validar si cambió el documento
            if (ciudadanoExistente.NumeroDocumento != documentoLimpio)
            {
                bool existeDocumento = await _ciudadanoRepo.ExistsAsync(c => c.Id != dto.Id && c.NumeroDocumento == documentoLimpio);
                if (existeDocumento) throw new Exception("Ya existe un ciudadano registrado con este número de documento de identidad.");

                // REGLA CRÍTICA: No modificar documento si ya participó en una elección
                bool yaVoto = await _votacionCiudadanoRepo.ExistsAsync(vc => vc.CiudadanoId == dto.Id);
                if (yaVoto) throw new Exception("No se puede modificar el número de documento de identidad de este ciudadano porque ya participó en una elección.");

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
                Apellido = c.Apellido, // Revisa que la entidad tenga Apellido
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

        // --- Métodos Privados de Validación ---
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

            // Expresión regular básica para validar correo
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(correo))
            {
                throw new Exception("Debe ingresar un correo electrónico válido.");
            }
        }
    }
}
