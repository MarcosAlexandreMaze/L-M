using LMStore.Application.DTOs.Cliente;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

public class ClienteService(IClienteRepository clienteRepository, IUnitOfWork unitOfWork) : IClienteService
{
    public async Task<IReadOnlyList<EnderecoResponse>> ListarEnderecosAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var cliente = await ObterClienteComEnderecosOuFalharAsync(usuarioId, ct);
        return [.. cliente.Enderecos.Select(ParaResponse)];
    }

    public async Task<EnderecoResponse> AdicionarEnderecoAsync(
        Guid usuarioId, EnderecoRequest request, CancellationToken ct = default)
    {
        var cliente = await ObterClienteComEnderecosOuFalharAsync(usuarioId, ct);

        var endereco = cliente.AdicionarEndereco(
            request.Apelido, new Cep(request.Cep), request.Logradouro, request.Numero, request.Complemento,
            request.Bairro, request.Cidade, request.Estado, request.Pais);

        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(endereco);
    }

    public async Task<EnderecoResponse> AtualizarEnderecoAsync(
        Guid usuarioId, Guid enderecoId, EnderecoRequest request, CancellationToken ct = default)
    {
        var cliente = await ObterClienteComEnderecosOuFalharAsync(usuarioId, ct);

        cliente.AtualizarEndereco(
            enderecoId, request.Apelido, new Cep(request.Cep), request.Logradouro, request.Numero,
            request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Pais);

        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(cliente.Enderecos.First(e => e.Id == enderecoId));
    }

    public async Task RemoverEnderecoAsync(Guid usuarioId, Guid enderecoId, CancellationToken ct = default)
    {
        var cliente = await ObterClienteComEnderecosOuFalharAsync(usuarioId, ct);

        cliente.RemoverEndereco(enderecoId);
        await unitOfWork.SalvarAsync(ct);
    }

    public async Task<EnderecoResponse> DefinirEnderecoPadraoAsync(
        Guid usuarioId, Guid enderecoId, CancellationToken ct = default)
    {
        var cliente = await ObterClienteComEnderecosOuFalharAsync(usuarioId, ct);

        cliente.DefinirEnderecoPadrao(enderecoId);
        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(cliente.Enderecos.First(e => e.Id == enderecoId));
    }

    private async Task<Cliente> ObterClienteComEnderecosOuFalharAsync(Guid usuarioId, CancellationToken ct)
    {
        var cliente = await clienteRepository.ObterPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("Cliente não encontrado para este usuário.");

        return await clienteRepository.ObterPorIdComEnderecosAsync(cliente.Id, ct)
            ?? throw new NotFoundException("Cliente não encontrado.");
    }

    private static EnderecoResponse ParaResponse(Endereco endereco) => new(
        endereco.Id, endereco.Apelido, endereco.Cep.Formatado, endereco.Logradouro, endereco.Numero,
        endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Pais, endereco.Padrao);
}
