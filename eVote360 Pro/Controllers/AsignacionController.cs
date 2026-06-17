using Application.Interfaces;
using eVote360_Pro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization; // Necesario para la seguridad
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    [Authorize] // Bloquea todo el controlador a usuarios no logueados
    public class AsignacionController : Controller
    {
        private readonly IAsignacionCandidatoPuestoService _service;
        private readonly ICandidatoService _candidatoService;
        private readonly IPuestoElectivoService _puestoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AsignacionController(
            IAsignacionCandidatoPuestoService service,
            ICandidatoService candidatoService,
            IPuestoElectivoService puestoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _candidatoService = candidatoService;
            _puestoService = puestoService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int ObtenerPartidoId()
        {
            // 1. Intento principal: Leer de la Sesión (lo que ya tenías)
            var id = _httpContextAccessor.HttpContext?.Session.GetInt32("PartidoId");

            if (id.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Valor obtenido de SESIÓN: {id}");
                return id.Value;
            }

            // 2. Respaldo (Fallback): Leer de las Claims del usuario autenticado
            // Esto es "a prueba de fallos" porque las Claims viajan en la cookie de autenticación
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("PartidoId");

            if (claim != null && int.TryParse(claim.Value, out int claimId))
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Sesión vacía. Valor recuperado de CLAIMS: {claimId}");
                return claimId;
            }

            // 3. Si llega aquí, realmente no hay datos
            System.Diagnostics.Debug.WriteLine("DEBUG: ERROR CRÍTICO. Ni Sesión ni Claims tienen PartidoId.");
            return 0;
        }

        [Authorize(Roles = "Admin,Dirigente")]
        public async Task<IActionResult> Index()
        {
            int partidoId = ObtenerPartidoId();

            // Validación de seguridad: si no hay partido en sesión, fuera
            if (partidoId == 0) return RedirectToAction("Login", "Usuario");

            var asignaciones = await _service.ObtenerAsignacionesPorPartidoAsync(partidoId);
            return View(asignaciones);
        }

        [Authorize(Roles = "Admin,Dirigente")]
        public async Task<IActionResult> Create()
        {
            int partidoId = ObtenerPartidoId();

            // --- USAMOS EL FILTRO ---
            var candidatos = await _service.GetCandidatosDisponiblesAsync(partidoId);
            var puestos = await _puestoService.ObtenerTodosAsync();

            var vm = new CrearAsignacionViewModel
            {
                Candidatos = candidatos.Select(c => new SelectListItem($"{c.Nombre} {c.Apellido}", c.Id.ToString())),
                Puestos = puestos.Select(p => new SelectListItem(p.Nombre, p.Id.ToString()))
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Dirigente")]
        public async Task<IActionResult> Create(CrearAsignacionViewModel vm)
        {
            int partidoId = ObtenerPartidoId();
            if (partidoId == 0) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid)
            {
                // --- RECARGAMOS CON FILTRO SI HAY ERROR DE VALIDACIÓN ---
                var candidatos = await _service.GetCandidatosDisponiblesAsync(partidoId);
                var puestos = await _puestoService.ObtenerTodosAsync();
                vm.Candidatos = candidatos.Select(c => new SelectListItem($"{c.Nombre} {c.Apellido}", c.Id.ToString()));
                vm.Puestos = puestos.Select(p => new SelectListItem(p.Nombre, p.Id.ToString()));
                return View(vm);
            }

            try
            {
                await _service.AsignarAsync(vm.CandidatoId, vm.PuestoElectivoId, partidoId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                // --- RECARGAMOS CON FILTRO SI HAY ERROR DE LÓGICA ---
                var candidatos = await _service.GetCandidatosDisponiblesAsync(partidoId);
                var puestos = await _puestoService.ObtenerTodosAsync();
                vm.Candidatos = candidatos.Select(c => new SelectListItem($"{c.Nombre} {c.Apellido}", c.Id.ToString()));
                vm.Puestos = puestos.Select(p => new SelectListItem(p.Nombre, p.Id.ToString()));
                return View(vm);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Dirigente")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int partidoId = ObtenerPartidoId();
                if (partidoId == 0) return RedirectToAction("Login", "Usuario");

                await _service.EliminarAsignacionAsync(id, partidoId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Esto mostrará el error en la consola o podrías pasarlo a un TempData
                System.Diagnostics.Debug.WriteLine($"Error al eliminar: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}