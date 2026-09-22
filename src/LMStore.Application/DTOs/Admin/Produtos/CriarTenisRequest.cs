using LMStore.Domain.Enums;

namespace LMStore.Application.DTOs.Admin.Produtos;

public record CriarTenisRequest(
    string Nome,
    string Descricao,
    Guid MarcaId,
    Guid CategoriaId,
    decimal Preco,
    GeneroProduto Genero,
    IndicacaoUso IndicacaoUso,
    TipoPisada? TipoPisada);
