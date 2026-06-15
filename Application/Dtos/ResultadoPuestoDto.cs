using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ResultadoPuestoDto
    {
        public string NombrePuesto { get; set; }
        public int TotalVotosPuesto { get; set; }
        public List<CandidatoResultadoDto> ResultadosCandidatos { get; set; } = new List<CandidatoResultadoDto>();
    }
}
