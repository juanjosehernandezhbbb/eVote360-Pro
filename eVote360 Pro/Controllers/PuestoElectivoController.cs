using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class PuestoElectivoController : Controller
    {
        private readonly IPuestoElectivoService _puestoService;

        public PuestoElectivoController(IPuestoElectivoService puestoService)
        {
            _puestoService = puestoService;
        }

        // GET: PuestoElectivo
        public async Task<IActionResult> Index()
        {
            var puestos = await _puestoService.ObtenerTodosAsync();
            return View(puestos);
        }

        // GET: PuestoElectivo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PuestoElectivo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PuestoElectivoDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _puestoService.CrearAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: PuestoElectivo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var puesto = await _puestoService.ObtenerPorIdAsync(id);
            if (puesto == null) return NotFound();

            return View(puesto);
        }

        // POST: PuestoElectivo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PuestoElectivoDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _puestoService.ActualizarAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: PuestoElectivo/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool estado)
        {
            try
            {
                await _puestoService.CambiarEstadoAsync(id, estado);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}