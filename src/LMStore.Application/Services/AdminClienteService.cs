using LMStore.Application.DTOs.Admin.Clientes;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;

namespace LMStore.Application.Services;

// Só leitura, de propósito — o briefing pede "visualizar clientes" para o
// administrador, não editar dados de outra pessoa em nome dela.
public class AdminClienteService(IClienteRepository clienteRepository) : IAdminClienteService
{
    public async Task<IReadOnlyList<ClienteAdminResponse>> ListarAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var clientes = await clienteRepository.ListarAsync(pagina, tamanhoPagina, ct);
        return [.. clientes.Select(ParaResponse)];
    }

    public async Task<ClienteAdminResponse> ObterPorIdAsync(Guid clienteId, CancellationToken ct = default)
    {
        var cliente = await clienteRepository.ObterPorIdComEnderecosAsync(clienteId, ct)
            ?? throw new NotFoundException($"Cliente '{clienteId}' não encontrado.");

        return ParaResponse(cliente);
    }

    private static ClienteAdminResponse ParaResponse(Cliente cliente) => new(
        cliente.Id, cliente.Nome, cliente.Cpf.Formatado, cliente.Telefone, cliente.DataNascimento,
        cliente.Status.ToString(), cliente.Enderecos.Count, cliente.CriadoEm);
}
