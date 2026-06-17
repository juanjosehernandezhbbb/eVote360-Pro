using System.IO;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtraerTextoDeImagenAsync(Stream imagenStream);

        Task<string> ExtraerNumerosDeImagenAsync(Stream imagenStream);

        Task<string> ExtraerTextoDeRegionAsync(
            Stream imagenStream,
            float xPct,
            float yPct,
            float widthPct,
            float heightPct,
            string whitelist = null,
            bool esUnaSolaLinea = true);
    }
}