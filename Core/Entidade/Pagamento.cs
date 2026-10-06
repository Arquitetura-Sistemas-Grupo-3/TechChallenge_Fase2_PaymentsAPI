using Core.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entidade
{
    public class Pagamento : BaseEntity
    {

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdJogo { get; set; }

        public decimal Valor { get; set; }

        public bool Status { get; set; }    

        public Pagamento AddPagamento(PagamentoRequisicao pagamentoRequisicao)
        {
            return new Pagamento
            {
                IdUsuario = pagamentoRequisicao.IdUsuario,
                IdJogo = pagamentoRequisicao.IdJogo,
                Valor = pagamentoRequisicao.Valor,
                Status = pagamentoRequisicao.Status,
                Data = DateTime.Now
            };
        }

        public Pagamento AtualizarPagamento(PagamentoAtualizacao pagamentoAtualizacao)
        {
            return new Pagamento
            {
                Id = pagamentoAtualizacao.Id,
                IdUsuario = pagamentoAtualizacao.IdUsuario,
                IdJogo = pagamentoAtualizacao.IdJogo,
                Valor = pagamentoAtualizacao.Valor,
                Status = pagamentoAtualizacao.Status,
                Data = DateTime.Now
            };
        }
    }
}
