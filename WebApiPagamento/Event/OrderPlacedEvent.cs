namespace WebApiPagamento.NewFolder
{
    public class OrderPlacedEvent
    {
       public int OrderId { get; set; }
       public int UserId { get; set; }
       public int GameId { get; set; }
       public string GameName { get; set; }
       public string UserEmail { get; set; }
       public string UserRole { get; set; }
       public string? UserName { get; set; }
       public decimal Price { get; set; }
    }
}
