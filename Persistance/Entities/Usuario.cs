using Persistance.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistance.Enums;

namespace Persistance.Entities
{
    public class Usuario : BasicEntity<int>  
    {
        public required string Apellido { get; set; }
        public required string Email { get; set; }
        public required string NombreUsuario { get; set; }
        public required string PasswordHash { get; set; }
        public RolUsuario Rol { get; set; } = RolUsuario.Ciudadano;
        public ICollection<DirigentePolitico> dirigentePoliticos { get; set; }

    }
}
