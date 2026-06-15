using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CandidatoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string FotoUrl { get; set; }
        public int PartidoPoliticoId { get; set; }
        public string? PartidoNombre { get; set; } 
        public bool Estado { get; set; }
    }
}