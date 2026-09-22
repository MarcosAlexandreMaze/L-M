using LMStore.Application.DTOs.Admin.Produtos;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

public class AdminProdutoService(
    IProdutoRepository produtoRepository,
    IMarcaRepository marcaRepository,
    ICategoriaRepository categoriaRepository,
    IEstoqueRepository estoqueRepository,
    IUnitOfWork unitOfWork) : IAdminProdutoService
{
    public async Task<ProdutoDetalheResponse> CriarRoupaAsync(CriarRoupaRequest request, CancellationToken ct = default)
    {
        var (marca, categoria) = await ValidarMarcaECategoriaAtivasAsync(request.MarcaId, request.CategoriaId, ct);

        var roupa = new Roupa(
            request.Nome, request.Descricao, marca.Id, categoria.Id, new Dinheiro(request.Preco),
            request.Genero, request.TipoRoupa, request.Material);

        produtoRepository.Adicionar(roupa);
        await unitOfWork.SalvarAsync(ct);

        return roupa.ParaDetalhe(marca.Nome, categoria.Nome);
    }

    public async Task<ProdutoDetalheResponse> CriarTenisAsync(CriarTenisRequest request, CancellationToken ct = default)
    {
        var (marca, categoria) = await ValidarMarcaECategoriaAtivasAsync(request.MarcaId, request.CategoriaId, ct);

        var tenis = new Tenis(
            request.Nome, request.Descricao, marca.Id, categoria.Id, new Dinheiro(request.Preco),
            request.Genero, request.IndicacaoUso, request.TipoPisada);

        produtoRepository.Adicionar(tenis);
        await unitOfWork.SalvarAsync(ct);

        return tenis.ParaDetalhe(marca.Nome, categoria.Nome);
    }

    public async Task<ProdutoDetalheResponse> AtualizarAsync(
        Guid produtoId, AtualizarProdutoRequest request, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.AtualizarDescricao(request.Nome, request.Descricao);
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> AplicarPromocaoAsync(
        Guid produtoId, AplicarPromocaoRequest request, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.AplicarPromocao(new Dinheiro(request.PrecoPromocional));
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> RemoverPromocaoAsync(Guid produtoId, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.RemoverPromocao();
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> AtivarAsync(Guid produtoId, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.Ativar();
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> DesativarAsync(Guid produtoId, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.Desativar();
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> AdicionarVariacaoAsync(
        Guid produtoId, AdicionarVariacaoRequest request, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        var sku = new Sku(request.Sku);

        // Produto.AdicionarVariacao (Etapa 3) só garante unicidade DENTRO do próprio
        // produto — esta é a checagem de unicidade GLOBAL, contra o catálogo inteiro,
        // feita antes de qualquer mutação. O índice único em VariacoesProduto.Sku
        // (Etapa 6) continua sendo o backstop final contra condição de corrida; esta
        // checagem aqui é só pra dar um erro de negócio claro no caso comum.
        if (await produtoRepository.SkuJaExisteAsync(sku, ct))
            throw new ConflictException($"Já existe uma variação com o SKU '{sku.Codigo}' no catálogo.");

        var precoAdicional = request.PrecoAdicional is null ? null : new Dinheiro(request.PrecoAdicional.Value);
        var variacao = produto.AdicionarVariacao(sku, request.CodigoBarras, request.Tamanho, request.Cor, precoAdicional);

        // Produto e Estoque são agregados deliberadamente separados (Etapa 3/4) —
        // AdicionarVariacao não cria o Estoque correspondente sozinho. Essa
        // orquestração entre os dois agregados é responsabilidade da Application.
        estoqueRepository.Adicionar(Estoque.Criar(variacao.Id, request.QuantidadeInicial));

        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> AtivarVariacaoAsync(Guid produtoId, Guid variacaoId, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.AtivarVariacao(variacaoId);
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    public async Task<ProdutoDetalheResponse> DesativarVariacaoAsync(Guid produtoId, Guid variacaoId, CancellationToken ct = default)
    {
        var produto = await ObterOuFalharAsync(produtoId, ct);
        produto.DesativarVariacao(variacaoId);
        await unitOfWork.SalvarAsync(ct);

        return await MapearComNomesAsync(produto, ct);
    }

    private async Task<(Marca Marca, Categoria Categoria)> ValidarMarcaECategoriaAtivasAsync(
        Guid marcaId, Guid categoriaId, CancellationToken ct)
    {
        var marca = await marcaRepository.ObterPorIdAsync(marcaId, ct)
            ?? throw new NotFoundException($"Marca '{marcaId}' não encontrada.");

        if (!marca.Ativa)
            throw new DomainException("Não é possível cadastrar um produto para uma marca inativa.");

        var categoria = await categoriaRepository.ObterPorIdAsync(categoriaId, ct)
            ?? throw new NotFoundException($"Categoria '{categoriaId}' não encontrada.");

        if (!categoria.Ativa)
            throw new DomainException("Não é possível cadastrar um produto para uma categoria inativa.");

        return (marca, categoria);
    }

    private async Task<Produto> ObterOuFalharAsync(Guid produtoId, CancellationToken ct) =>
        await produtoRepository.ObterPorIdAsync(produtoId, ct)
            ?? throw new NotFoundException($"Produto '{produtoId}' não encontrado.");

    private async Task<ProdutoDetalheResponse> MapearComNomesAsync(Produto produto, CancellationToken ct)
    {
        var marca = await marcaRepository.ObterPorIdAsync(produto.MarcaId, ct);
        var categoria = await categoriaRepository.ObterPorIdAsync(produto.CategoriaId, ct);

        return produto.ParaDetalhe(marca?.Nome ?? "—", categoria?.Nome ?? "—");
    }
}
