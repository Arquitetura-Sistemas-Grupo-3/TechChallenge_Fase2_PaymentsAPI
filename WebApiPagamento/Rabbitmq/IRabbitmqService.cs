using Core.Input;
using WebApiPagamento.NewFolder;
using Core.Entidade;

namespace WebApiPagamento.Rabbitmq
{
    public interface IRabbitmqService
    {
  
        Task EnviarMensagemFilaOrder(OrderPlacedEvent orderPlacedEvent);

        Task<List<Pagamento>> TesteAsync();
    }
}
