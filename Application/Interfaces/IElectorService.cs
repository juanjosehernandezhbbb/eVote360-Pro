using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IElectorService
    {
        Task ValidarDocumentoAsync(string numeroDocumento);
        Task<bool> ValidarIdentidadOcrAsync(string numeroDocumento, Stream imagenCedula);
        Task EnviarCodigoVerificacionAsync(string numeroDocumento);
        Task<bool> ValidarCodigoVerificacionAsync(string numeroDocumento, string codigo);

        // El método unificado que espera la lista de selecciones:
        Task FinalizarVotacionAsync(int ciudadanoId, List<SeleccionVotoDto> selecciones);
    }
}
