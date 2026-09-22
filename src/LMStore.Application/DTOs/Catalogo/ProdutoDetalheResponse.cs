namespace LMStore.Application.DTOs.Catalogo;

public record ProdutoDetalheResponse(
    Guid Id,
    string Nome,
    string Descricao,
    string Tipo,
    string DetalhesEspecificos,
    decimal Preco,
    decimal? PrecoPromocional,
    bool EmPromocao,
    Guid MarcaId,
    string Marca,
    Guid CategoriaId,
    string Categoria,
    string Genero,
    bool Ativo,
    IReadOnlyList<string> Imagens,
    IReadOnlyList<VariacaoResponse> Variacoes);
