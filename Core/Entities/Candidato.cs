using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;

namespace Core.Entities
{
    public class Candidato : BasicEntity<int>
    {
        public required string Apellido { get; set; }
        public string FotoUrl { get; set; }
        public PartidoPolitico PartidoPolitico { get; set; }
        public int PartidoPoliticoId { get; set; }
        public ICollection<CandidatoPuesto> CandidatoPuestos { get; set; } = new List<CandidatoPuesto>();
        public ICollection<DetalleVoto> Detalles { get; set; } = new List<DetalleVoto>();


    }
}
