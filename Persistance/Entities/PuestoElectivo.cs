using Persistance.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class PuestoElectivo : BasicEntity<int>
    {
        public required string Descripcion { get; set; }
        public ICollection<CandidatoPuesto> CandidatoPuestos { get; set; }
        public ICollection<DetalleVoto> Detalles { get; set; }


    }
}
