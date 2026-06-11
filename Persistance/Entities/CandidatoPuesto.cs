using Persistance.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class CandidatoPuesto : BasicEntity<int>
    {
        public required int CandidatoId { get; set; }
        public Candidato Candidato { get; set; }
        public required int PuestoElectivoId { get; set; }
        public PuestoElectivo PuestoElectivo { get; set; }
        public required int PartidoPoliticoId { get; set; }
        public PartidoPolitico PartidoPolitico { get; set; }
    }
}
