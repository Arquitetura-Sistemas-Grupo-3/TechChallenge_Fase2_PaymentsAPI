using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entidade;
using Core.Input;
using Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repository
{
    public class PagamentoRepository : EFRepository<Pagamento>, IPagamentoRepository
    {
        public PagamentoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<PagamentoResposta>> ListarPagamento()
        {
            return await _dbSet.Select(p => new PagamentoResposta
            {
                Id = p.Id,
                Valor = p.Valor,
                Data = p.Data,
                IdJogo = p.IdJogo,
                IdUsuario = p.IdUsuario,
                Status = p.Status
            }).ToListAsync();
        }

        public async Task<PagamentoResposta> ListarPorId(int id)
        {
            return await _dbSet.Where(p => p.Id == id).Select(p => new PagamentoResposta
            {
                Id = p.Id,
                Data = p.Data,
                Valor = p.Valor,
                IdJogo = p.IdJogo,
                IdUsuario = p.IdUsuario,
                Status = p.Status
            }).FirstOrDefaultAsync();
        }
    }
}
