using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class PartidoPoliticoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Siglas { get; set; }
        public string LogoUrl { get; set; }
        public bool Estado { get; set; }
    }
}
