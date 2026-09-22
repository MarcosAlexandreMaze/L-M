using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class ClienteRepository(LMStoreDbContext context) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObterPorIdComEnderecosAsync(Guid id, CancellationToken ct = default) =>
        context.Clientes.Include(c => c.Enderecos).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default) =>
        context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId, ct);

    public Task<bool> ExistePorCpfAsync(Cpf cpf, CancellationToken ct = default) =>
        context.Clientes.AnyAsync(c => c.Cpf == cpf, ct);

    // Inclui Enderecos de propósito: sem o Include, a coleção nunca é carregada e
    // sempre aparece vazia em memória (o campo privado começa como lista vazia e o EF
    // só o preenche se for explicitamente pedido) — bug real, achado testando a tela
    // administrativa de clientes, que mostra a quantidade de endereços cadastrados.
    public async Task<IReadOnlyList<Cliente>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default) =>
        await context.Clientes
            .Include(c => c.Enderecos)
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

    public void Adicionar(Cliente cliente) => context.Clientes.Add(cliente);
}
