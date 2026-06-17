using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace eVote360_Pro.Models
{
    public class CrearAsignacionViewModel
    {
        public int CandidatoId { get; set; }
        public int PuestoElectivoId { get; set; }
        public IEnumerable<SelectListItem>? Candidatos { get; set; }
        public IEnumerable<SelectListItem>? Puestos { get; set; }
    }
}