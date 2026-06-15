using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPartidoPoliticoService
    {
        Task<IEnumerable<PartidoPoliticoDto>> ObtenerTodosAsync();
        Task<PartidoPoliticoDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(PartidoPoliticoDto dto);
        Task ActualizarAsync(PartidoPoliticoDto dto);
        Task CambiarEstadoAsync(int id, bool nuevoEstado);
    }
}
