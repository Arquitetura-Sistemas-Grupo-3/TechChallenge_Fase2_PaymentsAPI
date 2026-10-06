using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Input
{
    public class PagamentoResposta
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdJogo { get; set; }
        public decimal Valor { get; set; }
        public bool Status { get; set; }

        public DateTime Data { get; set; }
    }
}
