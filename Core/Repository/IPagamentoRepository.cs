using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entidade;
using Core.Input;

namespace Core.Repository
{
    public interface IPagamentoRepository : IRepository<Pagamento>   
    {
        public Task<List<PagamentoResposta>> ListarPagamento();

        public Task<PagamentoResposta> ListarPorId(int id);

    }
}
