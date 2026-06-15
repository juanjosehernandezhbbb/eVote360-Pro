using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CandidatoResultadoDto
    {
        public string CandidatoOpcion { get; set; } 
        public string PartidoPolitico { get; set; } 
        public int CantidadVotos { get; set; }
        public double PorcentajeVotos { get; set; }
        public string Resultado { get; set; } 
    }
}
