using Core.Entidade;
using Core.Input;
using Core.Repository;
using WebApiPagamento.Interface;
using WebApiPagamento.Rabbitmq;

namespace WebApiPagamento.Service
{
    public class PagamentoService : IPagamentoService
    {

        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly IRabbitmqService _rabbitmqService;

        public PagamentoService(IPagamentoRepository pagamentoRepository, IRabbitmqService rabbitmqService)
        {
            _pagamentoRepository = pagamentoRepository;
            _rabbitmqService = rabbitmqService;
        }

        public async Task<List<String>> LerMensagemConsumida()
        {
            return await _rabbitmqService.ConsumirMensagem();
        }

        public async Task<ServiceResposta> AdicionarPagamento(PagamentoRequisicao pagamentoRequisicao)
        {
            Pagamento pagamento = new Pagamento();
            _rabbitmqService.EnviarMenssagem("Approved");
            pagamento = pagamento.AddPagamento(pagamentoRequisicao);
            _pagamentoRepository.Create(pagamento);

            return new ServiceResposta { Sucesso = true, Mensagem = "Pagamento adicionado com sucesso!" };

        }

        public async Task<ServiceResposta> AtualizarPagamento(PagamentoAtualizacao pagamentoAtualizacao)
        {
            Pagamento pagamento = new Pagamento();

            pagamento = pagamento.AtualizarPagamento(pagamentoAtualizacao);

            _pagamentoRepository.Update(pagamento);

            return new ServiceResposta { Sucesso = true, Mensagem = "Pagamento atualizado com sucesso!" };
        }

        public async Task<ServiceResposta> DeletarPagamento(int id)
        {
            if (id > 0)
            {
                _pagamentoRepository.Delete(id);
                return new ServiceResposta { Sucesso = true, Mensagem = "Pagamento deletado com sucesso!" };
            }
            else
            {
                return new ServiceResposta { Sucesso = false, Mensagem = "Erro ao deletar pagamento." };
            }
        }

        public async Task<List<PagamentoResposta>> GetAll()
        {
            return await _pagamentoRepository.ListarPagamento();
        }

        public async Task<PagamentoResposta?> GetById(int id)
        {
            return await _pagamentoRepository.ListarPorId(id);
        }
    }
}
