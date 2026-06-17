using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    public class VotacionController : Controller
    {
        private readonly IElectorService _electorService;
        private readonly IGenericRepository<Ciudadano> _ciudadanoRepo;
        private readonly IGenericRepository<PuestoElectivo> _puestoRepo;
        private readonly IGenericRepository<CandidatoPuesto> _asignacionRepo;
        private readonly IGenericRepository<Eleccion> _eleccionRepo;

        public VotacionController(
            IElectorService electorService,
            IGenericRepository<Ciudadano> ciudadanoRepo,
            IGenericRepository<PuestoElectivo> puestoRepo,
            IGenericRepository<CandidatoPuesto> asignacionRepo,
            IGenericRepository<Eleccion> eleccionRepo)
        {
            _electorService = electorService;
            _ciudadanoRepo = ciudadanoRepo;
            _puestoRepo = puestoRepo;
            _asignacionRepo = asignacionRepo;
            _eleccionRepo = eleccionRepo;
        }

        // ==========================================
        // PASO 1: Identificación (Ingreso de Cédula)
        // ==========================================
        [HttpGet]
        public IActionResult Iniciar()
        {
            HttpContext.Session.Clear(); // Limpiar rastro previo por seguridad
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iniciar(string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                ViewBag.Error = "Debe ingresar su número de documento.";
                return View();
            }

            try
            {
                // Valida que exista elección, ciudadano activo y que no haya votado
                await _electorService.ValidarDocumentoAsync(numeroDocumento);

                // Guardamos el documento temporalmente en sesión
                HttpContext.Session.SetString("ElectorDocumento", numeroDocumento);

                // Redirigir directamente al OTP o flujo alternativo
                await _electorService.EnviarCodigoVerificacionAsync(numeroDocumento);
                return RedirectToAction(nameof(VerificarCodigo));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // ==========================================
        // PASO 2: Verificación de Código OTP
        // ==========================================
        [HttpGet]
        public IActionResult VerificarCodigo()
        {
            string cedula = HttpContext.Session.GetString("ElectorDocumento");
            if (string.IsNullOrEmpty(cedula)) return RedirectToAction(nameof(Iniciar));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarCodigo(string codigo)
        {
            string cedula = HttpContext.Session.GetString("ElectorDocumento");
            if (string.IsNullOrEmpty(cedula)) return RedirectToAction(nameof(Iniciar));

            try
            {
                await _electorService.ValidarCodigoVerificacionAsync(cedula, codigo);

                // Si el código es válido, buscamos el ID real del ciudadano para la boleta
                var ciudadanos = await _ciudadanoRepo.GetAllAsync();
                var ciudadano = ciudadanos.FirstOrDefault(c => c.NumeroDocumento == cedula);

                HttpContext.Session.SetInt32("ElectorId", ciudadano.Id);
                HttpContext.Session.SetString("ElectorAutorizado", "true");

                return RedirectToAction(nameof(BoletaElectoral));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // ==========================================
        // PASO 3: Renderizado de la Boleta Electoral
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> BoletaElectoral()
        {
            // Validar que el usuario pasó los filtros anteriores
            if (HttpContext.Session.GetString("ElectorAutorizado") != "true")
            {
                return RedirectToAction(nameof(Iniciar));
            }

            // Obtener puestos activos
            var puestos = (await _puestoRepo.GetAllAsync()).Where(p => p.Estado).ToList();

            // Obtener las asignaciones de candidatos vigentes INCLUYENDO sus datos relacionados
            var asignaciones = (await _asignacionRepo.GetAllWithIncludesAsync(
                                    a => a.Candidato,
                                    a => a.PartidoPolitico
                                ))
                                .Where(a => a.Estado)
                                .ToList();

            // Pasamos los datos a la vista para estructurar las secciones por puesto
            ViewBag.Puestos = puestos;
            ViewBag.Asignaciones = asignaciones;

            return View();
        }

        // ==========================================
        // PASO 4: Procesar y Finalizar el Voto
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVoto(List<SeleccionVotoDto> selecciones)
        {
            if (HttpContext.Session.GetString("ElectorAutorizado") != "true")
            {
                return RedirectToAction(nameof(Iniciar));
            }

            int? ciudadanoId = HttpContext.Session.GetInt32("ElectorId");
            if (!ciudadanoId.HasValue) return RedirectToAction(nameof(Iniciar));

            try
            {
                // Invoca la lógica unificada de tu servicio
                await _electorService.FinalizarVotacionAsync(ciudadanoId.Value, selecciones);

                // Destruir sesión para evitar recargas de página o fraudes
                HttpContext.Session.Clear();

                return RedirectToAction(nameof(VotoExitoso));
            }
            catch (Exception ex)
            {
                TempData["ErrorVotacion"] = ex.Message;
                return RedirectToAction(nameof(BoletaElectoral));
            }
        }

        [HttpGet]
        public IActionResult VotoExitoso()
        {
            return View();
        }

        // ==========================================
        // PASO OPCIONAL: Validación por OCR
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> VerificarOcr(IFormFile fotoCedula)
        {
            string cedula = HttpContext.Session.GetString("ElectorDocumento");
            if (string.IsNullOrEmpty(cedula) || fotoCedula == null) return BadRequest();

            try
            {
                using (var stream = fotoCedula.OpenReadStream())
                {
                    bool esValido = await _electorService.ValidarIdentidadOcrAsync(cedula, stream);
                    if (esValido)
                    {
                        await _electorService.EnviarCodigoVerificacionAsync(cedula);
                        return RedirectToAction(nameof(VerificarCodigo));
                    }
                }
                ViewBag.Error = "La verificación de la cédula falló.";
                return View("Iniciar");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Iniciar");
            }
        }
    }
}