using System;
using System.IO;
using System.Threading.Tasks;
using Tesseract;
using Application.Interfaces;

namespace Persistance.Ocr
{
    public class TesseractOcrService : IOcrService
    {
        private async Task<string> ProcesarConFiltro(Stream imagenStream, bool soloNumeros)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (imagenStream.CanSeek) imagenStream.Position = 0;

                    string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                    using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);

                    engine.SetVariable("user_defined_dpi", "300");
                    engine.SetVariable("preserve_interword_spaces", "1");

                    if (soloNumeros)
                    {
                        engine.SetVariable("tessedit_char_whitelist", "0123456789-");
                    }

                    engine.DefaultPageSegMode = PageSegMode.Auto;

                    using var ms = new MemoryStream();
                    imagenStream.CopyTo(ms);

                    using var pix = Pix.LoadFromMemory(ms.ToArray());
                    using var grayPix = pix.ConvertRGBToGray();

                    using var page = engine.Process(grayPix);
                    return page.GetText() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error crítico en OCR Global: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        public async Task<string> ExtraerTextoDeImagenAsync(Stream imagenStream)
        {
            return await ProcesarConFiltro(imagenStream, soloNumeros: false);
        }

        public async Task<string> ExtraerNumerosDeImagenAsync(Stream imagenStream)
        {
            return await ProcesarConFiltro(imagenStream, soloNumeros: true);
        }

        public async Task<string> ExtraerTextoDeRegionAsync(
            Stream imagenStream,
            float xPct,
            float yPct,
            float widthPct,
            float heightPct,
            string whitelist = null,
            bool esUnaSolaLinea = true)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (imagenStream.CanSeek) imagenStream.Position = 0;

                    string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                    using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);

                    engine.SetVariable("user_defined_dpi", "300");
                    engine.SetVariable("preserve_interword_spaces", "1");
                    engine.SetVariable("tessedit_do_invert", "0");

                    if (!string.IsNullOrEmpty(whitelist))
                    {
                        engine.SetVariable("tessedit_char_whitelist", whitelist);
                    }

                    engine.DefaultPageSegMode = esUnaSolaLinea ? PageSegMode.SingleLine : PageSegMode.SingleBlock;

                    using var ms = new MemoryStream();
                    imagenStream.CopyTo(ms);

                    using var pixOriginal = Pix.LoadFromMemory(ms.ToArray());
                    using var pixScaled = pixOriginal.Scale(2.0f, 2.0f);
                    using var grayPix = pixScaled.ConvertRGBToGray();

                    // Calculamos coordenadas directamente sobre la escala de grises limpia
                    int x = (int)(grayPix.Width * xPct);
                    int y = (int)(grayPix.Height * yPct);
                    int w = (int)(grayPix.Width * widthPct);
                    int h = (int)(grayPix.Height * heightPct);

                    x = Math.Max(0, Math.Min(x, grayPix.Width - 1));
                    y = Math.Max(0, Math.Min(y, grayPix.Height - 1));
                    w = Math.Min(w, grayPix.Width - x);
                    h = Math.Min(h, grayPix.Height - y);

                    var rect = new Tesseract.Rect(x, y, w, h);

                    using var page = engine.Process(grayPix, rect);
                    return page.GetText()?.Trim() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error crítico en OCR Regional: {ex.Message}");
                    return string.Empty;
                }
            });
        }
    }
}