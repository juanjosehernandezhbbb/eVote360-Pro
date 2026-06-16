using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Core.Enums;

namespace eVote360_Pro.Controllers
{
    public class BaseController : Controller
    {
                protected async Task<bool> IsEleccionActiva(IEleccionService eleccionService)
        {
            var elecciones = await eleccionService.ObtenerTodasAsync();
                        return elecciones.Any(e => e.EstadoEleccion == (int)EstadoEleccion.Activa);
        }   

        protected IActionResult ManejarError(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index");
        }
    }
}