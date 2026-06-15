using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAlianzaPoliticaService
    {
        Task<IEnumerable<SolicitudAlianzaDto>> ObtenerSolicitudesPendientesAsync(int partidoId);
        Task<IEnumerable<SolicitudAlianzaDto>> ObtenerSolicitudesRealizadasAsync(int partidoId);
        Task<IEnumerable<SolicitudAlianzaDto>> ObtenerAlianzasVigentesAsync(int partidoId);

        Task CrearSolicitudAsync(int partidoSolicitanteId, int partidoReceptorId);
        Task AceptarSolicitudAsync(int solicitudId, int partidoUsuarioId);
        Task RechazarSolicitudAsync(int solicitudId, int partidoUsuarioId);
        Task EliminarSolicitudAsync(int solicitudId, int partidoUsuarioId);
        Task EliminarAlianzaVigenteAsync(int alianzaId, int partidoUsuarioId);
    }
}
