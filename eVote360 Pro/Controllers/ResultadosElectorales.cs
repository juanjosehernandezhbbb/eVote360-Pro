using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    public class ResultadosElectoralesController : Controller
    {
        private readonly IEleccionService _eleccionService;

        public ResultadosElectoralesController(IEleccionService eleccionService)
        {
            _eleccionService = eleccionService;
        }

        // GET: ResultadosElectorales
        // Pantalla principal del modulo que lista todas las elecciones registradas
        public async Task<IActionResult> Index()
        {
            var elecciones = await _eleccionService.ObtenerTodasAsync();
            return View(elecciones);
        }

        // GET: ResultadosElectorales/Ver/5
        // Muestra los graficos y escrutinio de una eleccion especifica
        public async Task<IActionResult> Ver(int id, string nombreEleccion)
        {
            ViewBag.NombreEleccion = nombreEleccion ?? "Detalle de Eleccion";
            ViewBag.EleccionId = id;

            var resultados = await _eleccionService.ObtenerResultadosAsync(id);
            return View(resultados);
        }
    }
}