using System.ComponentModel.DataAnnotations;

namespace eVote360_Pro.ViewModels
{
    public class CiudadanoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [Display(Name = "Número de Documento")]
        public string NumeroDocumento { get; set; }

        public bool Estado { get; set; } = true;
    }
}