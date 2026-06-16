using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Persistance.Dtos;
using System;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    public class EleccionController : Controller
    {
        private readonly IEleccionService _eleccionService;

        public EleccionController(IEleccionService eleccionService)
        {
            _eleccionService = eleccionService;
        }

        // GET: Eleccion
        public async Task<IActionResult> Index(int? anio)
        {
            int anioFiltro = anio ?? DateTime.Now.Year;
            ViewBag.AnioActual = anioFiltro;

            var elecciones = await _eleccionService.ObtenerResumenPorAnioAsync(anioFiltro);
            return View(elecciones);
        }

        // GET: Eleccion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eleccion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EleccionDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _eleccionService.CrearAsync(dto);
                TempData["SuccessMessage"] = "Eleccion creada exitosamente en estado pendiente.";
                return RedirectToAction(nameof(Index), new { anio = dto.Fecha.Year });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: Eleccion/Activar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id, int anio)
        {
            try
            {
                await _eleccionService.ActivarAsync(id);
                TempData["SuccessMessage"] = "La eleccion ha sido activada de manera exitosa. El proceso de votacion esta abierto.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { anio = anio });
        }

        // POST: Eleccion/Finalizar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id, int anio)
        {
            try
            {
                await _eleccionService.FinalizarAsync(id);
                TempData["SuccessMessage"] = "La eleccion ha sido finalizada correctamente. El escrutinio de votos esta cerrado.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { anio = anio });
        }
    }
}