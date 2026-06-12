using Persistance.Entities;

namespace Persistance.Repositories
{
    public interface ICiudadanoRepository : IGenericRepository<Ciudadano>
    {
        Task<Ciudadano?> GetByNumeroDocumentoAsync(string numeroDocumento);
    }
}
