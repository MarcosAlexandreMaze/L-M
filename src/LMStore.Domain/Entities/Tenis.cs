using LMStore.Domain.Enums;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public sealed class Tenis : Produto
{
    public TipoPisada? TipoPisada { get; private set; }
    public IndicacaoUso IndicacaoUso { get; }

    public Tenis(
        string nome, string descricao, Guid marcaId, Guid categoriaId, Dinheiro preco,
        GeneroProduto genero, IndicacaoUso indicacaoUso, TipoPisada? tipoPisada = null)
        : base(nome, descricao, marcaId, categoriaId, preco, genero)
    {
        IndicacaoUso = indicacaoUso;
        TipoPisada = tipoPisada;
    }

    public void DefinirTipoDePisada(TipoPisada? tipoPisada) => TipoPisada = tipoPisada;

    public override string DetalhesEspecificos() =>
        TipoPisada is null
            ? $"Indicado para {IndicacaoUso}"
            : $"Indicado para {IndicacaoUso} · Pisada: {TipoPisada}";
}
