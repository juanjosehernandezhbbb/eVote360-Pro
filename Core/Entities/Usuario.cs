using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Entities
{
    public class Usuario : BasicEntity<int>  
    {
        public required string Apellido { get; set; }
        public required string Email { get; set; }
        public required string NombreUsuario { get; set; }
        public required string PasswordHash { get; set; }
        // En Core/Entities/Usuario.cs
        public int Rol { get; set; }
        public ICollection<DirigentePolitico> dirigentePoliticos { get; set; }

    }
}
