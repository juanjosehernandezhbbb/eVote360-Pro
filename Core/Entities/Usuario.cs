using Core.Common;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Usuario : BasicEntity<int>
    {
        public required string Apellido { get; set; }
        public required string Email { get; set; }
        public required string NombreUsuario { get; set; }
        public required string PasswordHash { get; set; }
        public int Rol { get; set; }
        public int? PartidoPoliticoId { get; set; }

        public ICollection<DirigentePolitico> dirigentePoliticos { get; set; }
    }
}