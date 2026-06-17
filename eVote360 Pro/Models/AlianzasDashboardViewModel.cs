using Application.Dtos;
using System.Collections.Generic;

namespace Application.ViewModels
{
    public class AlianzasDashboardViewModel
    {
        public IEnumerable<SolicitudAlianzaDto> AlianzasVigentes { get; set; }
        public IEnumerable<SolicitudAlianzaDto> SolicitudesPendientes { get; set; }
        public IEnumerable<SolicitudAlianzaDto> SolicitudesRealizadas { get; set; }
        public int MiPartidoId { get; set; }
    }
}