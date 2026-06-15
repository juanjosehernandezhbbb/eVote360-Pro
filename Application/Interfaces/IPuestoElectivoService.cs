using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Dtos;

namespace Application.Interfaces
{
    public interface IPuestoElectivoService
    {
        Task<IEnumerable<PuestoElectivoDto>> ObtenerTodosAsync();
        Task<PuestoElectivoDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(PuestoElectivoDto dto);
        Task ActualizarAsync(PuestoElectivoDto dto);
        Task CambiarEstadoAsync(int id, bool estado);
    }
}
