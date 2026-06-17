using Application.Interfaces;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    [Authorize]
    public class AlianzaController : Controller
    {
        private readonly IAlianzaPoliticaService _alianzaService;
        private readonly IPartidoPoliticoService _partidoService;

        public AlianzaController(
            IAlianzaPoliticaService alianzaService,
            IPartidoPoliticoService partidoService)
        {
            _alianzaService = alianzaService;
            _partidoService = partidoService;
        }

        // ==========================================
        // MÉTODO ACTUALIZADO: Leer desde los Claims
        // ==========================================
        private int? ObtenerMiPartidoId()
        {
            if (User.IsInRole("Admin"))
            {
                return null;
            }

            var claimPartido = User.FindFirst("PartidoId");
            if (claimPartido != null && int.TryParse(claimPartido.Value, out int id) && id > 0)
            {
                return id;
            }
            return null;
        }

        // GET: Alianza
        public async Task<IActionResult> Index()
        {
            int? miPartidoId = ObtenerMiPartidoId();
            int idParaConsultas = miPartidoId ?? 0;

            var vm = new AlianzasDashboardViewModel
            {
                MiPartidoId = idParaConsultas,
                AlianzasVigentes = await _alianzaService.ObtenerAlianzasVigentesAsync(idParaConsultas),
                SolicitudesPendientes = await _alianzaService.ObtenerSolicitudesPendientesAsync(idParaConsultas),
                SolicitudesRealizadas = await _alianzaService.ObtenerSolicitudesRealizadasAsync(idParaConsultas)
            };

            return View(vm);
        }

        // GET: Alianza/Create
        public async Task<IActionResult> Create()
        {
            var todosLosPartidos = await _partidoService.ObtenerTodosAsync();
            int? miPartidoId = ObtenerMiPartidoId();

            var partidosFiltrados = todosLosPartidos
                .Where(p => p.Estado == true)
                .Where(p => miPartidoId == null || p.Id != miPartidoId)
                .ToList();

            ViewBag.PartidosDisponibles = partidosFiltrados;
            return View(new CrearSolicitudViewModel());
        }

        // POST: Alianza/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearSolicitudViewModel vm)
        {
            int? miPartidoId = ObtenerMiPartidoId();

            if (!ModelState.IsValid)
            {
                var todosLosPartidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = miPartidoId == null
                    ? todosLosPartidos.Where(p => p.Estado == true).ToList()
                    : todosLosPartidos.Where(p => p.Id != miPartidoId && p.Estado == true).ToList();
                return View(vm);
            }

            try
            {
                // Si es un Dirigente (miPartidoId tiene valor), forzamos que el Solicitante sea él.
                // Si es Admin (miPartidoId es null), necesitarás adaptar tu lógica si quieres que el Admin cree alianzas por otros.
                // Por ahora asumo que el Admin no crea alianzas, solo los dirigentes.
                int idSolicitante = miPartidoId ?? throw new Exception("Un administrador no puede solicitar alianzas en nombre de un partido sin especificarlo.");

                await _alianzaService.CrearSolicitudAsync(idSolicitante, vm.PartidoReceptorId);
                TempData["SuccessMessage"] = "Solicitud de alianza enviada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                var todosLosPartidos = await _partidoService.ObtenerTodosAsync();
                ViewBag.PartidosDisponibles = miPartidoId == null
                    ? todosLosPartidos.Where(p => p.Estado == true).ToList()
                    : todosLosPartidos.Where(p => p.Id != miPartidoId && p.Estado == true).ToList();
                return View(vm);
            }
        }

        // POST: Alianza/Aceptar
        [HttpPost]
        public async Task<IActionResult> Aceptar(int id)
        {
            try
            {
                await _alianzaService.AceptarSolicitudAsync(id, ObtenerMiPartidoId() ?? 0);
                TempData["SuccessMessage"] = "Alianza aceptada.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Alianza/Rechazar
        [HttpPost]
        public async Task<IActionResult> Rechazar(int id)
        {
            try
            {
                await _alianzaService.RechazarSolicitudAsync(id, ObtenerMiPartidoId() ?? 0);
                TempData["SuccessMessage"] = "Solicitud rechazada.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Alianza/EliminarVigente
        [HttpPost]
        public async Task<IActionResult> EliminarVigente(int id)
        {
            try
            {
                await _alianzaService.EliminarAlianzaVigenteAsync(id, ObtenerMiPartidoId() ?? 0);
                TempData["SuccessMessage"] = "Alianza terminada.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}