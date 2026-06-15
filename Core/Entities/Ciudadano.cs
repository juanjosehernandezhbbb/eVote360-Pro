using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Ciudadano : BasicEntity<int>
    {
        public required string Apellido { get; set; }
        public required string Email { get; set; }
        public required string NumeroDocumento { get; set; }
        public ICollection<CodigoVerificacion> CodigosDeVerificacion { get; set; } = new List<CodigoVerificacion>();
        public ICollection<Voto> VotosCollection { get; set; } = new List<Voto>();
        public ICollection<VotacionCiudadano> VotacionesCiudadanos { get; set; } = new List<VotacionCiudadano>();



    }
}
