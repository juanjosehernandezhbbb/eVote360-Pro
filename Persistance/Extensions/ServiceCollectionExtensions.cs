using Microsoft.Extensions.DependencyInjection;
using Persistance.Repositories;
using Core.Repositories;


namespace Persistance.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistanceRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICiudadanoRepository, CiudadanoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEleccionRepository, EleccionRepository>();
            services.AddScoped<ICodigoVerificacionRepository, CodigoVerificacionRepository>();
            return services;
        }
    }
}
