using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Cupom : Entity
{
    public string Codigo { get; }
    public TipoDesconto TipoDesconto { get; }
    public decimal Valor { get; }
    public DateTime DataInicio { get; }
    public DateTime DataFim { get; }
    public Dinheiro ValorMinimoCompra { get; }
    public int LimiteUso { get; }
    public int QuantidadeUtilizada { get; private set; }
    public bool Ativo { get; private set; }

    public Cupom(
        string codigo, TipoDesconto tipoDesconto, decimal valor, DateTime dataInicio, DateTime dataFim,
        Dinheiro valorMinimoCompra, int limiteUso)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("O código do cupom é obrigatório.");

        if (dataFim <= dataInicio)
            throw new DomainException("A data de término deve ser posterior à data de início.");

        if (limiteUso <= 0)
            throw new DomainException("O limite de uso deve ser maior que zero.");

        ValidarValor(tipoDesconto, valor);

        Codigo = codigo.Trim().ToUpperInvariant();
        TipoDesconto = tipoDesconto;
        Valor = valor;
        DataInicio = dataInicio;
        DataFim = dataFim;
        ValorMinimoCompra = valorMinimoCompra;
        LimiteUso = limiteUso;
        QuantidadeUtilizada = 0;
        Ativo = true;
    }

    public (bool Valido, string? Motivo) EstaValidoPara(Dinheiro subtotal, DateTime agora)
    {
        if (!Ativo)
            return (false, "Este cupom está inativo.");

        if (agora < DataInicio)
            return (false, "Este cupom ainda não está válido.");

        if (agora > DataFim)
            return (false, "Este cupom está expirado.");

        if (QuantidadeUtilizada >= LimiteUso)
            return (false, "Este cupom atingiu o limite de utilizações.");

        if (subtotal < ValorMinimoCompra)
            return (false, $"O valor mínimo de compra para este cupom é {ValorMinimoCompra}.");

        return (true, null);
    }

    public Dinheiro CalcularDesconto(Dinheiro subtotal)
    {
        var (valido, motivo) = EstaValidoPara(subtotal, DateTime.UtcNow);

        if (!valido)
            throw new DomainException(motivo!);

        return TipoDesconto switch
        {
            TipoDesconto.Percentual => subtotal.Subtrair(subtotal.AplicarPercentualDeDesconto(Valor)),
            TipoDesconto.Fixo => new Dinheiro(Math.Min(Valor, subtotal.Valor)),
            _ => throw new DomainException($"Tipo de desconto '{TipoDesconto}' não é suportado.")
        };
    }

    public void RegistrarUso()
    {
        if (QuantidadeUtilizada >= LimiteUso)
            throw new DomainException("Este cupom já atingiu o limite de utilizações.");

        QuantidadeUtilizada++;
    }

    public void Ativar()
    {
        if (Ativo)
            throw new DomainException("Este cupom já está ativo.");

        Ativo = true;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Este cupom já está inativo.");

        Ativo = false;
    }

    private static void ValidarValor(TipoDesconto tipoDesconto, decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("O valor do desconto deve ser maior que zero.");

        if (tipoDesconto == TipoDesconto.Percentual && valor > 100)
            throw new DomainException("O desconto percentual não pode ser maior que 100%.");
    }
}
