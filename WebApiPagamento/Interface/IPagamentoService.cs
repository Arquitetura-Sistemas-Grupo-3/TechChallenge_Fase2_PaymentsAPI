using Core.Entidade;
using Core.Input;

namespace WebApiPagamento.Interface
{
    public interface IPagamentoService
    {
        public Task<List<PagamentoResposta>> GetAll();

        public Task<ServiceResposta> AdicionarPagamento(PagamentoRequisicao pagamentoRequisicao);

        public Task<ServiceResposta> AtualizarPagamento(PagamentoAtualizacao pagamentoAtualizacao);

        public Task<ServiceResposta> DeletarPagamento(int id);

        public Task<PagamentoResposta?> GetById(int id);

        public Task<ServiceResposta> AdicionarPagamentoFila();

        public Task<List<Pagamento>> ReceberPagamentoFila();

      /*  public Task<List<String>> LerMensagemConsumida();
      */
    }
}
