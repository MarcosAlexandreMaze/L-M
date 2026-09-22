using LMStore.Domain.Enums;

namespace LMStore.Application.DTOs.Admin.Produtos;

public record CriarRoupaRequest(
    string Nome,
    string Descricao,
    Guid MarcaId,
    Guid CategoriaId,
    decimal Preco,
    GeneroProduto Genero,
    TipoRoupa TipoRoupa,
    string Material);
