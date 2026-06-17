using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360_Pro.ViewModels
{
    public class RegistroCiudadanoViewModel : CiudadanoViewModel
    {
        [Display(Name = "Foto de Documento de Identidad")]
        [Required(ErrorMessage = "Por favor, suba la imagen de su documento.")]
        public IFormFile DocumentoImagen { get; set; }
    }
}