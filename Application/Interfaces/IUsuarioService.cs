using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync();
        Task<UsuarioDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(UsuarioDto dto, string password); 
        Task ActualizarAsync(UsuarioDto dto, string? nuevaPassword);
        Task CambiarEstadoAsync(int id, bool nuevoEstado);
    }
}
