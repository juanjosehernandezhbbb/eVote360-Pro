using Persistance.Entities;

namespace Persistance.Repositories
{
    public interface IEleccionRepository : IGenericRepository<Eleccion>
    {
        Task<Eleccion> GetEleccionActivaAsync();
    }
}