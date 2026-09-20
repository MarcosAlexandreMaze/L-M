using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public abstract class Produto : Entity
{
    private readonly List<string> _imagens = [];
    private readonly List<VariacaoProduto> _variacoes = [];

    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Guid MarcaId { get; }
    public Guid CategoriaId { get; private set; }
    public Dinheiro Preco { get; private set; }
    public Dinheiro? PrecoPromocional { get; private set; }
    public GeneroProduto Genero { get; }
    public bool Ativo { get; private set; }
    public IReadOnlyList<string> Imagens => _imagens.AsReadOnly();
    public IReadOnlyList<VariacaoProduto> Variacoes => _variacoes.AsReadOnly();

    protected Produto(
        string nome, string descricao, Guid marcaId, Guid categoriaId, Dinheiro preco, GeneroProduto genero)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição do produto é obrigatória.");

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        MarcaId = marcaId;
        CategoriaId = categoriaId;
        Preco = preco;
        Genero = genero;
        Ativo = true;
    }

    public abstract string DetalhesEspecificos();

    public bool EstaEmPromocao() => PrecoPromocional is not null;

    public Dinheiro PrecoVigente() => PrecoPromocional ?? Preco;

    public void AplicarPromocao(Dinheiro precoPromocional)
    {
        if (precoPromocional >= Preco)
            throw new DomainException("O preço promocional deve ser menor que o preço cheio.");

        PrecoPromocional = precoPromocional;
    }

    public void RemoverPromocao() => PrecoPromocional = null;

    public void AlterarPreco(Dinheiro novoPreco)
    {
        if (PrecoPromocional is not null && novoPreco <= PrecoPromocional)
            throw new DomainException("O novo preço deve ser maior que o preço promocional vigente.");

        Preco = novoPreco;
    }

    public void AtualizarDescricao(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição do produto é obrigatória.");

        Nome = nome.Trim();
        Descricao = descricao.Trim();
    }

    public void MoverParaCategoria(Guid categoriaId) => CategoriaId = categoriaId;

    public void AdicionarImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("A URL da imagem não pode ser vazia.");

        _imagens.Add(url.Trim());
    }

    public void RemoverImagem(string url) => _imagens.Remove(url);

    public VariacaoProduto AdicionarVariacao(
        Sku sku, string? codigoBarras, string tamanho, string cor, Dinheiro? precoAdicional = null)
    {
        if (_variacoes.Any(v => v.Sku == sku))
            throw new ConflictException($"Já existe uma variação com o SKU '{sku}' neste produto.");

        var variacao = new VariacaoProduto(Id, sku, codigoBarras, tamanho, cor, precoAdicional);
        _variacoes.Add(variacao);

        return variacao;
    }

    public void RemoverVariacao(Guid variacaoId)
    {
        var variacao = BuscarVariacaoOuFalhar(variacaoId);
        _variacoes.Remove(variacao);
    }

    public void AtivarVariacao(Guid variacaoId) => BuscarVariacaoOuFalhar(variacaoId).Ativar();

    public void DesativarVariacao(Guid variacaoId) => BuscarVariacaoOuFalhar(variacaoId).Desativar();

    public void Ativar()
    {
        if (Ativo)
            throw new DomainException("Este produto já está ativo.");

        Ativo = true;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Este produto já está inativo.");

        Ativo = false;
    }

    private VariacaoProduto BuscarVariacaoOuFalhar(Guid variacaoId) =>
        _variacoes.FirstOrDefault(v => v.Id == variacaoId)
            ?? throw new NotFoundException($"Variação '{variacaoId}' não encontrada para este produto.");
}
