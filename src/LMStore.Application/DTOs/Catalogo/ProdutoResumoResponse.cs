namespace LMStore.Application.DTOs.Catalogo;

public record ProdutoResumoResponse(
    Guid Id,
    string Nome,
    string Tipo,
    decimal Preco,
    decimal? PrecoPromocional,
    bool EmPromocao,
    string Marca,
    string Categoria,
    string Genero);
