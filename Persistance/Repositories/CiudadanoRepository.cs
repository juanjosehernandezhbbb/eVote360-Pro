using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Persistance.Entities;

namespace Persistance.Repositories
{
    public class CiudadanoRepository : GenericRepository<Ciudadano>, ICiudadanoRepository
    {
        public CiudadanoRepository(eVote360ProContext context) : base(context) { }

        public async Task<Ciudadano?> GetByNumeroDocumentoAsync(string numeroDocumento)
        {
            return await _context.Ciudadanos
                .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento && c.Estado);
        }
    }
}
