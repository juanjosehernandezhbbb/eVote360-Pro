using Application.Dtos;
using Persistance.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEleccionService
    {
        Task<IEnumerable<EleccionDto>> ObtenerTodasAsync();
        Task<IEnumerable<EleccionDto>> ObtenerResumenPorAnioAsync(int anio);
        Task CrearAsync(EleccionDto dto);
        Task ActivarAsync(int id);
        Task FinalizarAsync(int id);
        Task<IEnumerable<ResultadoPuestoDto>> ObtenerResultadosAsync(int eleccionId);
    }
}
