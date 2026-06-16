using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Dtos;

namespace Application.Interfaces
{
    public interface ICandidatoService
    {
        Task<IEnumerable<CandidatoDto>> ObtenerTodosAsync();
        Task<IEnumerable<CandidatoDto>> ObtenerPorPartidoAsync(int partidoId);
        Task<CandidatoDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(CandidatoDto dto);
        Task ActualizarAsync(CandidatoDto dto);
        Task CambiarEstadoAsync(int id, bool estado);
    }
}
