using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Repositories;
using eVote360_Pro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eVote360_Pro.Controllers
{
    [Authorize(Roles = "Admin")] // Solo los administradores pueden hacer esto
    public class AsignacionDirigenteController : Controller
    {
        private readonly IAsignacionDirigenteService _asignacionService;
        private readonly IGenericRepository<Usuario> _usuarioRepo;
        private readonly IGenericRepository<PartidoPolitico> _partidoRepo;

        public AsignacionDirigenteController(
            IAsignacionDirigenteService asignacionService,
            IGenericRepository<Usuario> usuarioRepo,
            IGenericRepository<PartidoPolitico> partidoRepo)
        {
            _asignacionService = asignacionService;
            _usuarioRepo = usuarioRepo;
            _partidoRepo = partidoRepo;
        }

        public async Task<IActionResult> Index()
        {
            var asignaciones = await _asignacionService.ObtenerTodosAsync();
            return View(asignaciones);
        }

        public async Task<IActionResult> Create()
        {
            var vm = await CargarListasAsync(new CrearAsignacionDirigenteViewModel());
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CrearAsignacionDirigenteViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm = await CargarListasAsync(vm);
                return View(vm);
            }

            try
            {
                await _asignacionService.AsignarAsync(vm.UsuarioId, vm.PartidoPoliticoId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm = await CargarListasAsync(vm);
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _asignacionService.EliminarAsignacionAsync(id);
            }
            catch (Exception ex)
            {
                // Manejar el error visualmente (puedes usar TempData para mostrar una alerta)
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para llenar los <select>
        private async Task<CrearAsignacionDirigenteViewModel> CargarListasAsync(CrearAsignacionDirigenteViewModel vm)
        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            var partidos = await _partidoRepo.GetAllAsync();

            // Filtramos: Solo usuarios activos que sean dirigentes
            var dirigentes = usuarios
                .Where(u => u.Estado && u.Rol == (int)RolUsuario.DirigentePolitico)
                .ToList();

            // Filtramos: Solo partidos activos
            var partidosActivos = partidos
                .Where(p => p.Estado)
                .ToList();

            vm.UsuariosDirigentes = dirigentes.Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.NombreUsuario})", u.Id.ToString()));
            vm.PartidosPoliticos = partidosActivos.Select(p => new SelectListItem(p.Nombre, p.Id.ToString()));

            return vm;
        }
    }
}