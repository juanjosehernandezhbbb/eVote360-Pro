using System;

namespace Persistance.Dtos
{
    public class CodigoVerificacionDto
    {
        public int Id { get; set; }
        public int CiudadanoId { get; set; }
        public int EleccionId { get; set; }
        public string Codigo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
    }
}
