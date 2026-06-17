using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IPartidoPoliticoService _partidoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAsignacionDirigenteService _asignacionService; // <-- NUEVO: Inyectamos tu servicio

        public UsuarioController(
            IUsuarioService usuarioService,
            IPartidoPoliticoService partidoService,
            IHttpContextAccessor httpContextAccessor,
            IAsignacionDirigenteService asignacionService) // <-- NUEVO: Lo añadimos al constructor
        {
            _usuarioService = usuarioService;
            _partidoService = partidoService;
            _httpContextAccessor = httpContextAccessor;
            _asignacionService = asignacionService;
        }

        // ==========================================
        // LOGIN (GET Y POST)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos.";
                return View();
            }

            var usuario = await _usuarioService.ValidarCredencialesAsync(email, password);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            bool isAdmin = usuario.Rol == 1;

            // =========================================================================
            // NUEVA LÓGICA: Si es Dirigente, buscamos su partido en la tabla de asignaciones
            // =========================================================================
            if (!isAdmin)
            {
                var asignaciones = await _asignacionService.ObtenerTodosAsync();
                var asignacionDelUsuario = asignaciones.FirstOrDefault(a => a.UsuarioId == usuario.Id);

                if (asignacionDelUsuario == null)
                {
                    ViewBag.Error = "El usuario existe, pero no tiene un partido político asignado."; //
                    return View();
                }

                // Le incrustamos el ID del partido al DTO para que el resto de tu código funcione intacto
                usuario.PartidoPoliticoId = asignacionDelUsuario.PartidoPoliticoId;
            }
            // =========================================================================

            if (usuario.PartidoPoliticoId.HasValue)
            {
                HttpContext.Session.SetInt32("PartidoId", usuario.PartidoPoliticoId.Value);
            }

            string nombreRol = isAdmin ? "Admin" : "Dirigente";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("PartidoId", usuario.PartidoPoliticoId?.ToString() ?? "0")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _httpContextAccessor.HttpContext?.Session.Clear();
            return RedirectToAction("Login", "Usuario");
        }

        // ==========================================
        // CRUD USUARIOS (MANTENIDO IGUAL)
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return View(usuarios);
        }

        public async Task<IActionResult> Create()
        {
            var partidos = await _partidoService.ObtenerTodosAsync();
            ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioDto dto, string password)
        {
            if (string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("password", "El password es obligatorio.");

            if (!ModelState.IsValid)
            {
                var partidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
                return View(dto);
            }

            try
            {
                await _usuarioService.CrearAsync(dto, password);
                TempData["SuccessMessage"] = "Usuario registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var partidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            var partidos = await _partidoService.ObtenerTodosAsync();
            ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioDto dto, string? nuevaPassword)
        {
            Debug.WriteLine($"DEBUG EDIT: Usuario: {dto.NombreUsuario}, Recibiendo PartidoPoliticoId: {dto.PartidoPoliticoId}");

            if (!ModelState.IsValid)
            {
                var partidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
                return View(dto);
            }

            try
            {
                await _usuarioService.ActualizarAsync(dto, nuevaPassword);
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar: " + ex.Message);
                var partidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = partidos.Where(p => p.Estado == true).ToList();
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, bool nuevoEstado)
        {
            try
            {
                await _usuarioService.CambiarEstadoAsync(id, nuevoEstado);
                TempData["SuccessMessage"] = "Estado del usuario actualizado.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}