using Core.Input;
using Microsoft.AspNetCore.Mvc;
using WebApiPagamento.Interface;

namespace WebApiPagamento.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PagamentoController : Controller
    {
        private readonly IPagamentoService _pagamentoService;

        public PagamentoController(IPagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpGet]
        public async Task<IActionResult> ListarPagamento()
        {
            var pagamentos = await _pagamentoService.GetAll();

            return Ok(pagamentos);
        }

        [HttpGet("receberpagamentofila")]
        public async Task<IActionResult> ReceberPagamentoFila()
        {
            var pagamentos = await _pagamentoService.ReceberPagamentoFila();
            return Ok(pagamentos);
        }

        [HttpGet("addpagamentofila")]
        public async Task<IActionResult> AdicionarPagamentoFila()
        {
            var resposta = await _pagamentoService.AdicionarPagamentoFila();
            if (resposta.Sucesso)
                return Ok(new { Sucesso = true, Mensagem = "Pagamento adicionado na fila com sucesso!" });
            else
                return BadRequest(new { Sucesso = false, Mensagem = "Erro ao adicionar pagamento na fila." });
        }
        [HttpPost]
        public async Task<IActionResult> AdicionarPagamento([FromBody] PagamentoRequisicao pagamentoRequisicao)
        {
            if (TryValidateModel(pagamentoRequisicao))
            {
                var resposta = await _pagamentoService.AdicionarPagamento(pagamentoRequisicao);

                if (resposta.Sucesso)
                    return Ok(new { Sucesso = true, Mensagem = "Pagamento adicionado com sucesso!" });
                else
                    return BadRequest(new { Sucesso = false, Mensagem = "Erro ao adicionar pagamento." });
            }
           return BadRequest(new { Sucesso = false, Mensagem = "Erro ao adicionar pagamento. Modelo inválido." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ListarPagamentoPorId(int id)
        {
            var resposta = await _pagamentoService.GetById(id);

            return Ok(resposta);
        }

        [HttpPut]
        public async Task<IActionResult> AtualizarPagamento([FromBody] PagamentoAtualizacao pagamentoAtualizacao)
        {
            var resposta = await _pagamentoService.AtualizarPagamento(pagamentoAtualizacao);
            if (resposta.Sucesso)
                return Ok(new { Sucesso = true, Mensagem = "Pagamento atualizado com sucesso!" });
            else
                return BadRequest(new { Sucesso = false, Mensagem = "Erro ao atualizar pagamento." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletarPagamento(int id)
        {
            var resposta = await _pagamentoService.DeletarPagamento(id);
            if (resposta.Sucesso)
                return Ok(new { Sucesso = true, Mensagem = "Pagamento deletado com sucesso!" });
            else
                return BadRequest(new { Sucesso = false, Mensagem = "Erro ao deletar pagamento." });
        }
    }
}
