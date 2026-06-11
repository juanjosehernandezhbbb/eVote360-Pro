using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistance.Common;

namespace Persistance.Entities
{
    public class Candidato : BasicEntity<int>
    {
        public required string Apellido { get; set; }
        public string FotoUrl { get; set; }
        public PartidoPolitico PartidoPolitico { get; set; }
        public int PartidoPoliticoId { get; set; }
        public ICollection<CandidatoPuesto> CandidatoPuestos { get; set; }
        public ICollection<DetalleVoto> Detalles { get; set; }


    }
}
