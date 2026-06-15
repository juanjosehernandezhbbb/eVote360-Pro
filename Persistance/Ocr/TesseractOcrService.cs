using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Tesseract;

namespace Persistance.Ocr
{
    public class TesseractOcrService : IOcrService
    {
        public async Task<string> ExtraerTextoDeImagenAsync(Stream imagenStream)
        {
            
            return await Task.Run(() =>
            {
               
                string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

                using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);

                using var ms = new MemoryStream();
                imagenStream.CopyTo(ms);
                byte[] imageBytes = ms.ToArray();

                using var img = Pix.LoadFromMemory(imageBytes);

                using var page = engine.Process(img);

                return page.GetText();
            });
        }
    }
}