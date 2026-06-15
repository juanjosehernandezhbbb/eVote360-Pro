using System;

namespace Persistance.Dtos
{
    public class EleccionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public int EstadoEleccion { get; set; }
        public int CantidadPartidosParticipantes { get; set; }
        public int CantidadPuestosDisputados { get; set; }
        public int CantidadCiudadanosVotaron { get; set; }
    }
}
