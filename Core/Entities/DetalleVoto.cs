using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DetalleVoto
    {

        public int Id { get; set; }
        public Candidato Candidato { get; set; }
        public int? CandidatoId { get; set; }

        public PuestoElectivo PuestoElectivo { get; set; }
        public int PuestoElectivoId { get; set; }
        public int VotoId { get; set; }
        public Voto Voto { get; set; }

    }
}
