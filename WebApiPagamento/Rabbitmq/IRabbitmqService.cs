namespace WebApiPagamento.Rabbitmq
{
    public interface IRabbitmqService
    {
        Task EnviarMenssagem(string message);

        Task<List<String>> ConsumirMensagem();
    }
}
