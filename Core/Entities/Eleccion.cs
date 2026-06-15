using Core.Common;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Eleccion : BasicEntity<int>
    {
        public required DateTime Fecha { get; set; }

        // Navegaciones
        public ICollection<Voto> Votos { get; set; } = new List<Voto>();
        public ICollection<CodigoVerificacion> CodigoVerificacion { get; set; } = new List<CodigoVerificacion>();
        public ICollection<VotacionCiudadano> VotacionesCiudadanos { get; set; } = new List<VotacionCiudadano>();
        public EstadoEleccion EstadoEleccion { get; internal set; }
    }
}