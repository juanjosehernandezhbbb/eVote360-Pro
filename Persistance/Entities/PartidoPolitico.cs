using Persistance.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Entities
{
    public class PartidoPolitico : BasicEntity<int>
    {
        public required string Descripcion { get; set; }
        public required string Siglas { get; set; }
        public required string LogoUrl { get; set; }
        public ICollection<Candidato> Candidatos { get; set; }
        public ICollection<CandidatoPuesto> CandidatoPuestos { get; set; }
        public ICollection<DirigentePolitico> dirigentePoliticos { get; set; }
        public ICollection<SolicitudAlianza> SolicitudesEnviadas { get; set; }
        public ICollection<SolicitudAlianza> SolicitudesRecibidas { get; set; }




    }
}
