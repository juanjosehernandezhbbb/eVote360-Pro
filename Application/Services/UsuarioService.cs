using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepo;
        private readonly IGenericRepository<DirigentePolitico> _dirigenteRepo;

        public UsuarioService(
            IGenericRepository<Usuario> usuarioRepo,
            IGenericRepository<DirigentePolitico> dirigenteRepo)
        {
            _usuarioRepo = usuarioRepo;
            _dirigenteRepo = dirigenteRepo;
        }

        public async Task CrearAsync(UsuarioDto dto, string password)
        {
            var usernameLimpio = dto.NombreUsuario.Trim().ToLower();

            bool existeUsuario = await _usuarioRepo.ExistsAsync(u => u.NombreUsuario.ToLower() == usernameLimpio);
            if (existeUsuario) throw new Exception("Ya existe un usuario registrado con este nombre de usuario.");

            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Email = dto.Email.Trim(),
                NombreUsuario = usernameLimpio,
                PasswordHash = HashearPassword(password),
                Rol = dto.Rol,
                Estado = true
            };

            await _usuarioRepo.AddAsync(nuevoUsuario);
        }

        public async Task ActualizarAsync(UsuarioDto dto, string? nuevaPassword)
        {
            var usuarioExistente = await _usuarioRepo.GetByIdAsync(dto.Id);
            if (usuarioExistente == null) throw new Exception("Usuario no encontrado.");

            var usernameLimpio = dto.NombreUsuario.Trim().ToLower();

            if (usuarioExistente.NombreUsuario.ToLower() != usernameLimpio)
            {
                bool existeUsuario = await _usuarioRepo.ExistsAsync(u => u.Id != dto.Id && u.NombreUsuario.ToLower() == usernameLimpio);
                if (existeUsuario) throw new Exception("Ya existe un usuario registrado con este nombre de usuario.");
                usuarioExistente.NombreUsuario = usernameLimpio;
            }

            if (usuarioExistente.Rol == (int)RolUsuario.DirigentePolitico && dto.Rol == (int)RolUsuario.Administrador)
            {
                bool tienePartidoAsignado = await _dirigenteRepo.ExistsAsync(d => d.UsuarioId == dto.Id);
                if (tienePartidoAsignado) throw new Exception("No se puede cambiar el rol de este usuario a Administrador porque tiene un partido politico asignado. Elimine la asignacion primero.");
            }

            usuarioExistente.Nombre = dto.Nombre.Trim();
            usuarioExistente.Apellido = dto.Apellido.Trim();
            usuarioExistente.Email = dto.Email.Trim();
            usuarioExistente.Rol = dto.Rol;
            usuarioExistente.Estado = dto.Estado;

            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                usuarioExistente.PasswordHash = HashearPassword(nuevaPassword);
            }

            await _usuarioRepo.UpdateAsync(usuarioExistente);
        }

        // ===================================================================
        // METODO CORREGIDO: CambiarEstadoAsync
        // ===================================================================
        public async Task CambiarEstadoAsync(int id, bool nuevoEstado)
        {
            var usuarioExistente = await _usuarioRepo.GetByIdAsync(id);
            if (usuarioExistente == null) throw new Exception("Usuario no encontrado.");

            if (usuarioExistente.Estado == nuevoEstado) throw new Exception($"El usuario ya esta {(nuevoEstado ? "activo" : "inactivo")}.");

            if (!nuevoEstado)
            {
                // 1. Validacion para Dirigentes Politicos
                if (usuarioExistente.Rol == (int)RolUsuario.DirigentePolitico)
                {
                    bool tienePartido = await _dirigenteRepo.ExistsAsync(d => d.UsuarioId == id);
                    if (tienePartido) throw new Exception("No se puede desactivar este dirigente porque tiene un partido politico asignado.");
                }

                // 2. Validacion para Administradores (Aqui estaba el error)
                if (usuarioExistente.Rol == (int)RolUsuario.Administrador)
                {
                    var adminsActivos = (await _usuarioRepo.GetAllAsync()).Count(u => u.Rol == (int)RolUsuario.Administrador && u.Estado == true);
                    if (adminsActivos <= 1) throw new Exception("No se puede desactivar a este usuario porque es el unico Administrador activo en el sistema.");
                }
            }

            usuarioExistente.Estado = nuevoEstado;
            await _usuarioRepo.UpdateAsync(usuarioExistente);
        }

        public async Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync()
        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            return usuarios.Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                NombreUsuario = u.NombreUsuario,
                Rol = (int)u.Rol,
                Estado = u.Estado
            }).ToList();
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(int id)
        {
            var u = await _usuarioRepo.GetByIdAsync(id);
            if (u == null) return null;

            return new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                NombreUsuario = u.NombreUsuario,
                Rol = u.Rol,
                Estado = u.Estado
            };
        }

        public async Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password)
        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            var passwordHash = HashearPassword(password);

            var usuario = usuarios.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == email.Trim().ToLower() &&
                u.PasswordHash == passwordHash &&
                u.Estado == true);

            if (usuario == null) return null;

            return new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            };
        }

        private string HashearPassword(string textoPlano)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}