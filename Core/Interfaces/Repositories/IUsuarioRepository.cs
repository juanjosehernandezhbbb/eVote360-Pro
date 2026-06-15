using Core.Entities;

namespace Core.Repositories
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
    }
}
