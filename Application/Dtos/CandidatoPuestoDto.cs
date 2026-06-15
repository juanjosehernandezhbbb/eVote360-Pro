using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CandidatoPuestoDto
    {
        public int Id { get; set; }
        public int CandidatoId { get; set; }
        public int PuestoElectivoId { get; set; }
        public int PartidoPoliticoId { get; set; } // Partido que hace la asignación

        // Datos extendidos para la tabla
        public string NombreCandidato { get; set; }
        public string ApellidoCandidato { get; set; }
        public string NombrePuesto { get; set; }
        public string PartidoOrigenCandidato { get; set; }

        // Retornará "Propio" o "Aliado"
        public string TipoCandidatura { get; set; }
    }
}
