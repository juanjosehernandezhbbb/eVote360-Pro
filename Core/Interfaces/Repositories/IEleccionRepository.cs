using Core.Entities;

namespace Core.Repositories
{
    public interface IEleccionRepository : IGenericRepository<Eleccion>
    {
        Task<Eleccion?> GetEleccionActivaAsync();
    }
}