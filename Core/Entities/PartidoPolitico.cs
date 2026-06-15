using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class PartidoPolitico : BasicEntity<int>
    {
        public required string Descripcion { get; set; }
        public required string Siglas { get; set; }
        public required string LogoUrl { get; set; }
        public ICollection<Candidato> Candidatos { get; set; } = new List<Candidato>();
        public ICollection<CandidatoPuesto> CandidatoPuestos { get; set; } = new List<CandidatoPuesto>();
        public ICollection<DirigentePolitico> dirigentePoliticos { get; set; } = new List<DirigentePolitico>();
        public ICollection<SolicitudAlianza> SolicitudesEnviadas { get; set; } = new List<SolicitudAlianza>();
        public ICollection<SolicitudAlianza> SolicitudesRecibidas { get; set; } = new List<SolicitudAlianza>();




    }
}
