using Core.Input;

namespace WebApiPagamento.Rabbitmq
{
    public interface IRabbitmqService
    {
        Task EnviarMenssagem(PagamentoFila pagamentoFila);
    }
}
