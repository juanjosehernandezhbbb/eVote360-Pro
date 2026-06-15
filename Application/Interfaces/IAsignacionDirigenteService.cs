using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAsignacionDirigenteService
    {
        Task<IEnumerable<AsignacionDirigenteDto>> ObtenerTodosAsync();
        Task AsignarAsync(int usuarioId, int partidoPoliticoId);
        Task EliminarAsignacionAsync(int asignacionId);
    }
}