using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entidade
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime Data { get; set; }
    }
}
