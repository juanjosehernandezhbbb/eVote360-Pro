using Persistance.Entities;

namespace Persistance.Repositories
{
    public interface ICodigoVerificacionRepository : IGenericRepository<CodigoVerificacion>
    {
        Task<CodigoVerificacion?> GetCodigoVigenteAsync(int ciudadanoId, int eleccionId, string codigo);
    }
}
