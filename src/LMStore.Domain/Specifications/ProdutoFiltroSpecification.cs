using System.Linq.Expressions;
using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Specifications;

public class ProdutoFiltroSpecification : ISpecification<Produto>
{
    public Expression<Func<Produto, bool>> Criteria { get; }

    public ProdutoFiltroSpecification(
        string? nome = null,
        Guid? categoriaId = null,
        Guid? marcaId = null,
        GeneroProduto? genero = null,
        decimal? precoMinimo = null,
        decimal? precoMaximo = null,
        bool apenasAtivos = true)
    {
        var precoMin = precoMinimo is null ? null : new Dinheiro(precoMinimo.Value);
        var precoMax = precoMaximo is null ? null : new Dinheiro(precoMaximo.Value);

        Criteria = produto =>
            (!apenasAtivos || produto.Ativo) &&
            (nome == null || produto.Nome.Contains(nome)) &&
            (categoriaId == null || produto.CategoriaId == categoriaId) &&
            (marcaId == null || produto.MarcaId == marcaId) &&
            (genero == null || produto.Genero == genero) &&
            (precoMin == null || produto.Preco >= precoMin) &&
            (precoMax == null || produto.Preco <= precoMax);
    }
}
