using Core.Entidade;
using Core.Input;
using Core.Repository;
using WebApiPagamento.Interface;
using WebApiPagamento.NewFolder;
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

      /*  public async Task<List<String>> LerMensagemConsumida()
        {
            return await _rabbitmqService.ConsumirMensagem();
        }
      */
        public async Task<ServiceResposta> AdicionarPagamentoFila()
        {
            OrderPlacedEvent orderPlacedEvent = new OrderPlacedEvent
            {
                OrderId = 3,
                UserId = 4,
                GameId = 5,
                GameName = "Jogo Exemplo",
                UserEmail = "juli@gmail.com"
            };
            await _rabbitmqService.EnviarMensagemFilaOrder(orderPlacedEvent);
            return new ServiceResposta { Sucesso = true, Mensagem = "Pagamento adicionado na fila com sucesso!" };
        }

        public async Task<List<Pagamento>> ReceberPagamentoFila()
        {
            var teste = await _rabbitmqService.TesteAsync();
            return teste;
        }

        public async Task<ServiceResposta> AdicionarPagamento(PagamentoRequisicao pagamentoRequisicao)
        {
            Pagamento pagamento = new Pagamento();
           
            pagamento = pagamento.AddPagamento(pagamentoRequisicao);
            OrderPlacedEvent orderPlaced = new OrderPlacedEvent();
            var id =_pagamentoRepository.Create(pagamento);
            orderPlaced.Price = pagamentoRequisicao.Valor;
            orderPlaced.GameId = pagamentoRequisicao.IdJogo;
            orderPlaced.UserId = pagamentoRequisicao.IdUsuario;
            await _rabbitmqService.EnviarMensagemFilaOrder(orderPlaced);

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
