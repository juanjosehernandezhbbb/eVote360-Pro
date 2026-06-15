using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Core.Entities;
using Core.Enums;
using Core.Repositories;


namespace Persistance.Repositories
{
    public class EleccionRepository : GenericRepository<Eleccion>, IEleccionRepository
    {
        public EleccionRepository(eVote360ProContext context) : base(context)
        {
        }

        public async Task<Eleccion?> GetEleccionActivaAsync()
        {
            return await _context.Set<Eleccion>()
                .FirstOrDefaultAsync(e => e.Estado);
        }
    }
}