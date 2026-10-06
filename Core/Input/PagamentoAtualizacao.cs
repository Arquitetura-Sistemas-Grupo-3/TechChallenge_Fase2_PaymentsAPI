using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Input
{
    public class PagamentoAtualizacao
    {
        public int Id { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "O campo IdUsuario deve ser maior que zero.")]
        public int IdUsuario { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "O campo IdJogo deve ser maior que zero.")]
        public int IdJogo { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "O campo valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public bool Status { get; set; }
    }
}
