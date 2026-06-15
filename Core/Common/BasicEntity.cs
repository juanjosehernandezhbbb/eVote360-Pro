using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
  
        public class BasicEntity<TKey>
        {
            public TKey Id { get; set; }
            public required string Nombre { get; set; }
            public required bool Estado { get; set; } = true;
    }
    
}
