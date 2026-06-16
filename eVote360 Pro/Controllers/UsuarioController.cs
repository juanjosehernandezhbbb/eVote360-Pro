using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // ==========================================
        // SECCION: AUTENTICACION (LOGIN / LOGOUT)
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

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Mantiene la persistencia de la sesion
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contrasena incorrectos, o usuario inactivo.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuario");
        }

        // ==========================================
        // SECCION: ADMINISTRACION DE USUARIOS (CRUD)
        // ==========================================

        // GET: Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return View(usuarios);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioDto dto, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("password", "El password es obligatorio.");
            }

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _usuarioService.CrearAsync(dto, password);
                TempData["SuccessMessage"] = "Usuario registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: Usuario/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioDto dto, string? nuevaPassword)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _usuarioService.ActualizarAsync(dto, nuevaPassword);
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: Usuario/CambiarEstado
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