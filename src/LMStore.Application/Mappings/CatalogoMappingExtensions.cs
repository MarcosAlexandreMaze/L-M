using LMStore.Application.DTOs.Catalogo;
using LMStore.Domain.Entities;

namespace LMStore.Application.Mappings;

// Mapeamento manual, sem AutoMapper — decisão do plano: manter explícito o que está
// sendo exposto na API, sem "mágica" de reflexão por trás (mesmo raciocínio usado para
// não adotar ASP.NET Core Identity na Etapa 8).
public static class CatalogoMappingExtensions
{
    public static ProdutoResumoResponse ParaResumo(this Produto produto, string nomeMarca, string nomeCategoria) => new(
        produto.Id,
        produto.Nome,
        produto is Roupa ? "Roupa" : "Tenis",
        produto.Preco.Valor,
        produto.PrecoPromocional?.Valor,
        produto.EstaEmPromocao(),
        nomeMarca,
        nomeCategoria,
        produto.Genero.ToString());

    public static ProdutoDetalheResponse ParaDetalhe(this Produto produto, string nomeMarca, string nomeCategoria) => new(
        produto.Id,
        produto.Nome,
        produto.Descricao,
        produto is Roupa ? "Roupa" : "Tenis",
        produto.DetalhesEspecificos(),
        produto.Preco.Valor,
        produto.PrecoPromocional?.Valor,
        produto.EstaEmPromocao(),
        produto.MarcaId,
        nomeMarca,
        produto.CategoriaId,
        nomeCategoria,
        produto.Genero.ToString(),
        produto.Ativo,
        produto.Imagens,
        [.. produto.Variacoes.Select(v => v.ParaResponse())]);

    public static VariacaoResponse ParaResponse(this VariacaoProduto variacao) => new(
        variacao.Id, variacao.Sku.Codigo, variacao.CodigoBarras, variacao.Tamanho, variacao.Cor, variacao.Ativa);

    public static CategoriaResponse ParaResponse(this Categoria categoria) => new(
        categoria.Id, categoria.Nome, categoria.Slug, categoria.CategoriaPaiId, categoria.Ativa);

    public static MarcaResponse ParaResponse(this Marca marca) => new(marca.Id, marca.Nome, marca.Ativa);
}
