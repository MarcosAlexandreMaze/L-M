using LMStore.Application.DTOs.Cliente;

namespace LMStore.Application.Interfaces;

public interface IClienteService
{
    Task<IReadOnlyList<EnderecoResponse>> ListarEnderecosAsync(Guid usuarioId, CancellationToken ct = default);
    Task<EnderecoResponse> AdicionarEnderecoAsync(Guid usuarioId, EnderecoRequest request, CancellationToken ct = default);
    Task<EnderecoResponse> AtualizarEnderecoAsync(Guid usuarioId, Guid enderecoId, EnderecoRequest request, CancellationToken ct = default);
    Task RemoverEnderecoAsync(Guid usuarioId, Guid enderecoId, CancellationToken ct = default);
    Task<EnderecoResponse> DefinirEnderecoPadraoAsync(Guid usuarioId, Guid enderecoId, CancellationToken ct = default);
}
