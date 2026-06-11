using Persistance.Common;
using Persistance.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class SolicitudAlianza
    {
        public int Id { get; set; }
        public int PartidoSolicitanteId { get; set; }
        public PartidoPolitico PartidoSolicitante { get; set; }

        public int PartidoReceptorId { get; set; }
        public PartidoPolitico PartidoReceptor { get; set; }
        public EstadoSolicitudAlianza Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

    }
}
