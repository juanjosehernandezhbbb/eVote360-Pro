using Core.Entities;

namespace Core.Repositories
{
    public interface ICiudadanoRepository : IGenericRepository<Ciudadano>
    {
        Task<Ciudadano?> GetByNumeroDocumentoAsync(string numeroDocumento);
    }
}
