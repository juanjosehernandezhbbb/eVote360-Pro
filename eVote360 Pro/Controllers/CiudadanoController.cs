using Application.Dtos;
using Application.Interfaces;
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
                await _ciudadanoService.CrearAsync(dto);
                TempData["SuccessMessage"] = "Ciudadano registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Captura las excepciones de tu CiudadanoService (Ej: Correo duplicado, Elección activa)
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
    }
}