using Persistance.Common;
using Persistance.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class Eleccion : BasicEntity<int>
    {
        public required DateTime Fecha { get; set; }

        // Navegaciones
        public required ICollection<Voto> Votos { get; set; }
        public required ICollection<CodigoVerificacion> CodigoVerificacion { get; set; }
        public required ICollection<VotacionCiudadano> VotacionesCiudadanos { get; set; }
        public EstadoEleccion EstadoEleccion { get; internal set; }
    }
}