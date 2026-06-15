using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class VotacionCiudadano : BasicEntity<int>
    {
        public Ciudadano Ciudadano { get; set; }
        public int CiudadanoId { get; set; }
        public Eleccion Eleccion { get; set; }
        public int EleccionId { get; set; }
        public bool VotoFinalizado { get; set; }
    }
}
