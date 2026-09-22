namespace LMStore.Application.DTOs.Pedido;

public record EnderecoEntregaResponse(
    string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Estado, string Cep, string Pais);
