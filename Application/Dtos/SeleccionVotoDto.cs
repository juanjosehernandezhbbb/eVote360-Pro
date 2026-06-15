using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class SeleccionVotoDto
    {
        public int PuestoElectivoId { get; set; }
        public int? CandidatoId { get; set; } 
    }
}
