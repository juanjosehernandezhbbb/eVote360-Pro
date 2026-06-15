using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class SolicitudAlianzaDto
    {
        public int Id { get; set; }
        public int PartidoSolicitanteId { get; set; }
        public int PartidoReceptorId { get; set; }
        public int Estado { get; set; } 
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

       
        public string NombrePartidoSolicitante { get; set; }
        public string SiglasPartidoSolicitante { get; set; }
        public string NombrePartidoReceptor { get; set; }
        public string SiglasPartidoReceptor { get; set; }
    }
}
