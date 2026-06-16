using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels
{
    public class CandidatoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

                public string FotoUrl { get; set; }

                [Display(Name = "Foto del Candidato")]
        public IFormFile FotoFile { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un partido político")]
        [Display(Name = "Partido Político")]
        public int PartidoPoliticoId { get; set; }

        public bool Estado { get; set; }

                    }
}