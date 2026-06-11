using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Common
{
  
        public class BasicEntity<TKey>
        {
            public required TKey Id { get; set; }
            public required string Nombre { get; set; }
            public required bool Estado { get; set; } = true;
    }
    
}
