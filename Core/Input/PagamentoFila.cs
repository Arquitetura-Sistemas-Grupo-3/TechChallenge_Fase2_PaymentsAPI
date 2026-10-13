using System;

namespace Core.Input
{

	public class PagamentoFila
	{
		public int IdUsuario { get; set; }
		public int IdJogo { get; set; }
		public decimal Valor { get; set; }
		public DateTime Data { get; set; }
		public int IdPagamento { get; set; }
		public bool Status { get; set; }
    }
}
