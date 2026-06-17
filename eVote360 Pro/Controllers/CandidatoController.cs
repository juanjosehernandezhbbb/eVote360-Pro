using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Application.Interfaces;
using Application.ViewModels;
using Application.Dtos;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace eVote360_Pro.Controllers
{
    [Authorize]
    public class CandidatoController : Controller
    {
        private readonly ICandidatoService _candidatoService;
        private readonly IPartidoPoliticoService _partidoService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CandidatoController(
            ICandidatoService candidatoService,
            IPartidoPoliticoService partidoService,
            IWebHostEnvironment webHostEnvironment)
        {
            _candidatoService = candidatoService;
            _partidoService = partidoService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // MÉTODO AUXILIAR PARA OBTENER EL PARTIDO DEL USUARIO LOGUEADO
        // ==========================================
        private int? ObtenerPartidoIdUsuarioActual()
        {
            var claimPartido = User.FindFirst("PartidoId");
            if (claimPartido != null && int.TryParse(claimPartido.Value, out int id) && id > 0)
            {
                return id;
            }
            return null;
        }

        // GET: Candidato
        public async Task<IActionResult> Index()
        {
            var candidatos = await _candidatoService.ObtenerTodosAsync();

            // FILTRO DE AISLAMIENTO: Si es Dirigente, solo ve a los suyos
            if (User.IsInRole("Dirigente"))
            {
                int? miPartidoId = ObtenerPartidoIdUsuarioActual();
                candidatos = candidatos.Where(c => c.PartidoPoliticoId == miPartidoId).ToList();
            }

            return View(candidatos);
        }

        // GET: Candidato/Create
        public async Task<IActionResult> Create()
        {
            var partidos = await _partidoService.ObtenerTodosAsync();

            // FILTRO DE VISTA: El Dirigente solo puede seleccionarse a sí mismo en el DropDown
            if (User.IsInRole("Dirigente"))
            {
                int? miPartidoId = ObtenerPartidoIdUsuarioActual();
                partidos = partidos.Where(p => p.Id == miPartidoId).ToList();
            }

            ViewBag.Partidos = partidos;
            return View();
        }

        // POST: Candidato/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CandidatoViewModel vm)
        {
            ModelState.Remove("FotoUrl");

            // SEGURIDAD: Forzamos el ID del partido si es un Dirigente (evita hackeo del HTML)
            if (User.IsInRole("Dirigente"))
            {
                vm.PartidoPoliticoId = ObtenerPartidoIdUsuarioActual().Value;
            }

            if (!ModelState.IsValid)
            {
                var partidos = await _partidoService.ObtenerTodosAsync();
                if (User.IsInRole("Dirigente")) partidos = partidos.Where(p => p.Id == ObtenerPartidoIdUsuarioActual()).ToList();
                ViewBag.Partidos = partidos;

                return View(vm);
            }

            try
            {
                string nombreArchivoUnico = null;

                if (vm.FotoFile != null && vm.FotoFile.Length > 0)
                {
                    string carpetaSubidas = Path.Combine(_webHostEnvironment.WebRootPath, "images", "candidatos");
                    if (!Directory.Exists(carpetaSubidas)) Directory.CreateDirectory(carpetaSubidas);

                    nombreArchivoUnico = $"{Guid.NewGuid()}{Path.GetExtension(vm.FotoFile.FileName)}";
                    string rutaCompletaArchivo = Path.Combine(carpetaSubidas, nombreArchivoUnico);

                    using (var fileStream = new FileStream(rutaCompletaArchivo, FileMode.Create))
                    {
                        await vm.FotoFile.CopyToAsync(fileStream);
                    }
                }

                var dto = new CandidatoDto
                {
                    Nombre = vm.Nombre,
                    Apellido = vm.Apellido,
                    PartidoPoliticoId = vm.PartidoPoliticoId,
                    FotoUrl = nombreArchivoUnico != null ? $"/images/candidatos/{nombreArchivoUnico}" : "/images/default-avatar.png"
                };

                await _candidatoService.CrearAsync(dto);
                TempData["SuccessMessage"] = "Candidato registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al registrar: {ex.Message}");
                var partidos = await _partidoService.ObtenerTodosAsync();
                if (User.IsInRole("Dirigente")) partidos = partidos.Where(p => p.Id == ObtenerPartidoIdUsuarioActual()).ToList();
                ViewBag.Partidos = partidos;
                return View(vm);
            }
        }

        // GET: Candidato/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var candidato = await _candidatoService.ObtenerPorIdAsync(id);
            if (candidato == null) return NotFound();

            // SEGURIDAD: Un Dirigente no puede entrar a editar un candidato de otro partido
            if (User.IsInRole("Dirigente") && candidato.PartidoPoliticoId != ObtenerPartidoIdUsuarioActual())
            {
                return RedirectToAction("AccesoDenegado", "Home"); // O simplemente Unauthorized();
            }

            var partidos = await _partidoService.ObtenerTodosAsync();
            if (User.IsInRole("Dirigente"))
            {
                int? miPartidoId = ObtenerPartidoIdUsuarioActual();
                partidos = partidos.Where(p => p.Id == miPartidoId).ToList();
            }
            ViewBag.Partidos = partidos;

            var vm = new CandidatoViewModel
            {
                Id = candidato.Id,
                Nombre = candidato.Nombre,
                Apellido = candidato.Apellido,
                PartidoPoliticoId = candidato.PartidoPoliticoId,
                FotoUrl = candidato.FotoUrl
            };

            return View(vm);
        }

        // POST: Candidato/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CandidatoViewModel vm)
        {
            ModelState.Remove("FotoUrl");
            ModelState.Remove("FotoFile");

            // SEGURIDAD: Forzamos el ID del partido si es un Dirigente
            if (User.IsInRole("Dirigente"))
            {
                vm.PartidoPoliticoId = ObtenerPartidoIdUsuarioActual().Value;
            }

            if (!ModelState.IsValid)
            {
                var partidos = await _partidoService.ObtenerTodosAsync();
                if (User.IsInRole("Dirigente")) partidos = partidos.Where(p => p.Id == ObtenerPartidoIdUsuarioActual()).ToList();
                ViewBag.Partidos = partidos;
                return View(vm);
            }

            try
            {
                if (vm.FotoFile != null && vm.FotoFile.Length > 0)
                {
                    string carpetaSubidas = Path.Combine(_webHostEnvironment.WebRootPath, "images", "candidatos");
                    if (!Directory.Exists(carpetaSubidas)) Directory.CreateDirectory(carpetaSubidas);

                    string nombreArchivoUnico = $"{Guid.NewGuid()}{Path.GetExtension(vm.FotoFile.FileName)}";
                    string rutaCompleta = Path.Combine(carpetaSubidas, nombreArchivoUnico);

                    using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await vm.FotoFile.CopyToAsync(fileStream);
                    }
                    vm.FotoUrl = $"/images/candidatos/{nombreArchivoUnico}";
                }

                var dto = new CandidatoDto
                {
                    Id = vm.Id,
                    Nombre = vm.Nombre,
                    Apellido = vm.Apellido,
                    PartidoPoliticoId = vm.PartidoPoliticoId,
                    FotoUrl = vm.FotoUrl,
                    Estado = true
                };

                await _candidatoService.ActualizarAsync(dto);
                TempData["SuccessMessage"] = "Candidato actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content("ERROR DETECTADO: " + ex.Message + "\n\nDETALLES: " + ex.StackTrace);
            }
        }

        // POST: Candidato/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, bool estado)
        {
            // SEGURIDAD: Un Dirigente no puede desactivar candidatos de otro partido
            if (User.IsInRole("Dirigente"))
            {
                var candidato = await _candidatoService.ObtenerPorIdAsync(id);
                if (candidato == null || candidato.PartidoPoliticoId != ObtenerPartidoIdUsuarioActual())
                {
                    return Unauthorized();
                }
            }

            await _candidatoService.CambiarEstadoAsync(id, estado);
            return RedirectToAction(nameof(Index));
        }
    }
}