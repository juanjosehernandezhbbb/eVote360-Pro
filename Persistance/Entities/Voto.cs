using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class Voto
    {
        public required int Id { get; set; }
        public Ciudadano Ciudadano { get; set; }
        public int CiudadanoId { get; set; }

        public Eleccion Eleccion { get; set; }
        public int EleccionId { get; set; }

        public DateTime FechaVoto { get; set; }

        public int? CandidatoId { get; set; }
        public Candidato Candidato { get; set; }

        public ICollection<DetalleVoto> Detalles { get; set; }
    }
}
