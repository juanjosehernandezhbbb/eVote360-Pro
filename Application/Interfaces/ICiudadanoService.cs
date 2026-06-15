using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICiudadanoService
    {
        Task<IEnumerable<CiudadanoDto>> ObtenerTodosAsync();
        Task<CiudadanoDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(CiudadanoDto dto);
        Task ActualizarAsync(CiudadanoDto dto);
        Task CambiarEstadoAsync(int id, bool nuevoEstado);
    }
}
