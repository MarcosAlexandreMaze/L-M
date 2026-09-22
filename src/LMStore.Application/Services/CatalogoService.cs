using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.DTOs.Common;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.Specifications;

namespace LMStore.Application.Services;

public class CatalogoService(
    IProdutoRepository produtoRepository,
    ICategoriaRepository categoriaRepository,
    IMarcaRepository marcaRepository) : ICatalogoService
{
    public async Task<PagedResult<ProdutoResumoResponse>> BuscarProdutosAsync(
        string? nome, Guid? categoriaId, Guid? marcaId, GeneroProduto? genero,
        decimal? precoMinimo, decimal? precoMaximo, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var especificacao = new ProdutoFiltroSpecification(nome, categoriaId, marcaId, genero, precoMinimo, precoMaximo);
        var (produtos, total) = await produtoRepository.BuscarAsync(especificacao, pagina, tamanhoPagina, ct);

        var (nomesMarcas, nomesCategorias) = await CarregarNomesAsync(ct);

        var itens = produtos
            .Select(p => p.ParaResumo(
                nomesMarcas.GetValueOrDefault(p.MarcaId, "—"),
                nomesCategorias.GetValueOrDefault(p.CategoriaId, "—")))
            .ToList();

        return new PagedResult<ProdutoResumoResponse>(itens, pagina, tamanhoPagina, total);
    }

    public async Task<ProdutoDetalheResponse> ObterProdutoPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var produto = await produtoRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Produto '{id}' não encontrado.");

        var (nomesMarcas, nomesCategorias) = await CarregarNomesAsync(ct);

        return produto.ParaDetalhe(
            nomesMarcas.GetValueOrDefault(produto.MarcaId, "—"),
            nomesCategorias.GetValueOrDefault(produto.CategoriaId, "—"));
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync(CancellationToken ct = default) =>
        [.. (await categoriaRepository.ListarTodasAsync(ct)).Select(c => c.ParaResponse())];

    public async Task<IReadOnlyList<MarcaResponse>> ListarMarcasAsync(CancellationToken ct = default) =>
        [.. (await marcaRepository.ListarTodasAsync(ct)).Select(m => m.ParaResponse())];

    // Catálogos de marca/categoria são tipicamente pequenos (dezenas, não milhares) —
    // carregar tudo de uma vez e montar um dicionário em memória evita N+1 consultas
    // (uma por produto) sem precisar de nenhum método novo de repositório.
    private async Task<(Dictionary<Guid, string> Marcas, Dictionary<Guid, string> Categorias)> CarregarNomesAsync(
        CancellationToken ct)
    {
        var marcas = await marcaRepository.ListarTodasAsync(ct);
        var categorias = await categoriaRepository.ListarTodasAsync(ct);

        return (
            marcas.ToDictionary(m => m.Id, m => m.Nome),
            categorias.ToDictionary(c => c.Id, c => c.Nome));
    }
}
