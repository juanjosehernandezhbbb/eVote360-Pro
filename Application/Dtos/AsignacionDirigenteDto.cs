using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class AsignacionDirigenteDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PartidoPoliticoId { get; set; }

        public string NombreDirigente { get; set; }
        public string NombreUsuario { get; set; }
        public string NombrePartido { get; set; }
        public string SiglasPartido { get; set; }
        public bool EstadoDirigente { get; set; }
        public bool EstadoPartido { get; set; }
    }
}
