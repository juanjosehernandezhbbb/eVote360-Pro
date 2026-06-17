using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels
{
    public class CrearSolicitudViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un partido receptor")]
        [Display(Name = "Partido a invitar")]
        public int PartidoReceptorId { get; set; }
    }
}