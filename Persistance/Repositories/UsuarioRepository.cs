using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Persistance.Entities;

namespace Persistance.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(eVote360ProContext context) : base(context) { }

        public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Estado);
        }
    }
}
