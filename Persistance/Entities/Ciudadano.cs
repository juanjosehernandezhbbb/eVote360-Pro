using Persistance.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class Ciudadano : BasicEntity<int>
    {
        public required string Apellido { get; set; }
        public required string Email { get; set; }
        public required string NumeroDocumento { get; set; }
        public required ICollection<CodigoVerificacion> CodigosDeVerificacion { get; set; }
        public required ICollection<Voto> VotosCollection { get; set; }
        public required ICollection<VotacionCiudadano> VotacionesCiudadanos { get; set; }



    }
}
