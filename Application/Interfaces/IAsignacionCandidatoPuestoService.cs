using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAsignacionCandidatoPuestoService
    {
        Task<IEnumerable<CandidatoPuestoDto>> ObtenerAsignacionesPorPartidoAsync(int partidoId);
        Task AsignarAsync(int candidatoId, int puestoElectivoId, int partidoPoliticoId);
        Task EliminarAsignacionAsync(int asignacionId, int partidoPoliticoId);
    }
}
