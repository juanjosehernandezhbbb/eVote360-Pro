using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class CodigoVerificacion
    {
        public int Id { get; set; }
        public int CiudadanoId { get; set; }
        public Ciudadano Ciudadano { get; set; }

        public int EleccionId { get; set; }
        public Eleccion Eleccion { get; set; }

        public string Codigo { get; set; }  // 6 dígitos

        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; }

    }
}
