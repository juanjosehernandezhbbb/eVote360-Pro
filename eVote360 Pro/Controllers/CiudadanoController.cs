using Application.Dtos;
using Application.Interfaces;
using eVote360_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class CiudadanoController : Controller
    {
        private readonly ICiudadanoService _ciudadanoService;

        public CiudadanoController(ICiudadanoService ciudadanoService)
        {
            _ciudadanoService = ciudadanoService;
        }

        // GET: Ciudadano
        public async Task<IActionResult> Index()
        {
            var ciudadanos = await _ciudadanoService.ObtenerTodosAsync();
            return View(ciudadanos);
        }

        // GET: Ciudadano/Create
        public IActionResult Create()
        {
            return View(new CiudadanoDto());
        }

        // POST: Ciudadano/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CiudadanoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                // Limpieza de NumeroDocumento para eliminar guiones y espacios
                if (!string.IsNullOrEmpty(dto.NumeroDocumento))
                    dto.NumeroDocumento = dto.NumeroDocumento.Replace("-", "").Replace(" ", "");

                await _ciudadanoService.CrearAsync(dto);
                TempData["SuccessMessage"] = "Ciudadano registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: Ciudadano/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ciudadano = await _ciudadanoService.ObtenerPorIdAsync(id);
            if (ciudadano == null)
            {
                TempData["ErrorMessage"] = "Ciudadano no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(ciudadano);
        }

        // POST: Ciudadano/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CiudadanoDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                // Limpieza de NumeroDocumento
                if (!string.IsNullOrEmpty(dto.NumeroDocumento))
                    dto.NumeroDocumento = dto.NumeroDocumento.Replace("-", "").Replace(" ", "");

                await _ciudadanoService.ActualizarAsync(dto);
                TempData["SuccessMessage"] = "Ciudadano actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: Ciudadano/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, bool nuevoEstado)
        {
            try
            {
                await _ciudadanoService.CambiarEstadoAsync(id, nuevoEstado);
                TempData["SuccessMessage"] = nuevoEstado ? "Ciudadano activado exitosamente." : "Ciudadano desactivado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Ciudadano/Registrar
        [AllowAnonymous]
        public IActionResult Registrar()
        {
            return View();
        }

        // POST: Ciudadano/Registrar (Procesar Imagen)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroCiudadanoViewModel vm)
        {
            if (vm.DocumentoImagen == null || vm.DocumentoImagen.Length == 0)
            {
                ModelState.AddModelError("", "Por favor seleccione una imagen válida.");
                return View(vm);
            }

            using (var stream = vm.DocumentoImagen.OpenReadStream())
            {
                var ciudadanoDto = await _ciudadanoService.ProcesarOcrAsync(stream);
                return View("ConfirmarRegistro", ciudadanoDto);
            }
        }

        // POST: Ciudadano/ConfirmarRegistro
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarRegistro(CiudadanoDto dto)
        {
            // Limpieza de NumeroDocumento antes de validar o guardar
            if (!string.IsNullOrEmpty(dto.NumeroDocumento))
            {
                dto.NumeroDocumento = dto.NumeroDocumento.Replace("-", "").Replace(" ", "");
            }

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _ciudadanoService.RegistrarAutonomoAsync(dto);

                TempData["SuccessMessage"] = "¡Registro completado con éxito! Ya estás habilitado para participar cuando inicie el proceso de votación.";

                return RedirectToAction("Login", "Usuario");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }
    }
}