using Core.Entities;

namespace Core.Repositories
{
    public interface ICodigoVerificacionRepository : IGenericRepository<CodigoVerificacion>
    {
        Task<CodigoVerificacion?> GetCodigoVigenteAsync(int ciudadanoId, int eleccionId, string codigo);
    }
}
