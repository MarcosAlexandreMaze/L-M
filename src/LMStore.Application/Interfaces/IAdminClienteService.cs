using LMStore.Application.DTOs.Admin.Clientes;

namespace LMStore.Application.Interfaces;

public interface IAdminClienteService
{
    Task<IReadOnlyList<ClienteAdminResponse>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<ClienteAdminResponse> ObterPorIdAsync(Guid clienteId, CancellationToken ct = default);
}
