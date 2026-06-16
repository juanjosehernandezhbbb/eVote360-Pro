using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class PartidoPoliticoController : Controller
    {
        private readonly IPartidoPoliticoService _partidoService;

        public PartidoPoliticoController(IPartidoPoliticoService partidoService)
        {
            _partidoService = partidoService;
        }

        // GET: PartidoPolitico
        public async Task<IActionResult> Index()
        {
            var partidos = await _partidoService.ObtenerTodosAsync();
            return View(partidos);
        }

        // GET: PartidoPolitico/Create
        public IActionResult Create()
        {
            return View(new PartidoPoliticoDto());
        }

        // POST: PartidoPolitico/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartidoPoliticoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _partidoService.CrearAsync(dto);
                TempData["SuccessMessage"] = "Partido político registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: PartidoPolitico/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var partido = await _partidoService.ObtenerPorIdAsync(id);
            if (partido == null)
            {
                TempData["ErrorMessage"] = "Partido político no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(partido);
        }

        // POST: PartidoPolitico/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartidoPoliticoDto dto)
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
                await _partidoService.ActualizarAsync(dto);
                TempData["SuccessMessage"] = "Partido político actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: PartidoPolitico/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, bool nuevoEstado)
        {
            try
            {
                await _partidoService.CambiarEstadoAsync(id, nuevoEstado);
                TempData["SuccessMessage"] = nuevoEstado ? "Partido político activado con éxito." : "Partido político desactivado con éxito.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}