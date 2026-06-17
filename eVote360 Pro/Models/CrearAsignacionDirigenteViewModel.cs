using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eVote360_Pro.Models
{
    public class CrearAsignacionDirigenteViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un dirigente.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un partido político.")]
        public int PartidoPoliticoId { get; set; }

        // Las listas para llenar los dropdowns
        public IEnumerable<SelectListItem>? UsuariosDirigentes { get; set; }
        public IEnumerable<SelectListItem>? PartidosPoliticos { get; set; }
    }
}