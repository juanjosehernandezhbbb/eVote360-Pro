using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DirigentePolitico
    {
        public int Id { get; set; }
        public Usuario Usuario { get; set; }
        public int UsuarioId { get; set; }
        public PartidoPolitico PartidoPolitico { get; set; }
        public int PartidoPoliticoId { get; set; }

       
    }
}
