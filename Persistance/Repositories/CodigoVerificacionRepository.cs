using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Core.Entities;
using Core.Repositories;


namespace Persistance.Repositories
{
    public class CodigoVerificacionRepository : GenericRepository<CodigoVerificacion>, ICodigoVerificacionRepository
    {
        public CodigoVerificacionRepository(eVote360ProContext context) : base(context) { }

        public async Task<CodigoVerificacion?> GetCodigoVigenteAsync(int ciudadanoId, int eleccionId, string codigo)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<CodigoVerificacion>()
                .FirstOrDefaultAsync(c => c.CiudadanoId == ciudadanoId
                    && c.EleccionId == eleccionId
                    && c.Codigo == codigo
                    && !c.Usado
                    && c.FechaExpiracion > now);
        }
    }
}
