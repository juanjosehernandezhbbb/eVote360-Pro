using System;

namespace Persistance.Dtos
{
    public class EleccionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }
}
