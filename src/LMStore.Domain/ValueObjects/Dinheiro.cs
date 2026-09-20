using System.Globalization;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.ValueObjects;

public sealed record Dinheiro
{
    public decimal Valor { get; }

    public Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new DomainException("O valor monetário não pode ser negativo.");

        Valor = Math.Round(valor, 2, MidpointRounding.ToEven);
    }

    public static Dinheiro Zero => new(0);

    public Dinheiro Somar(Dinheiro outro) => new(Valor + outro.Valor);

    public Dinheiro Subtrair(Dinheiro outro)
    {
        var resultado = Valor - outro.Valor;
        return new Dinheiro(resultado < 0 ? 0 : resultado);
    }

    public Dinheiro MultiplicarPor(int quantidade)
    {
        if (quantidade < 0)
            throw new DomainException("A quantidade não pode ser negativa.");

        return new Dinheiro(Valor * quantidade);
    }

    public Dinheiro AplicarPercentualDeDesconto(decimal percentual)
    {
        if (percentual is < 0 or > 100)
            throw new DomainException("O percentual de desconto deve estar entre 0 e 100.");

        return new Dinheiro(Valor - (Valor * percentual / 100));
    }

    public static bool operator >(Dinheiro a, Dinheiro b) => a.Valor > b.Valor;
    public static bool operator <(Dinheiro a, Dinheiro b) => a.Valor < b.Valor;
    public static bool operator >=(Dinheiro a, Dinheiro b) => a.Valor >= b.Valor;
    public static bool operator <=(Dinheiro a, Dinheiro b) => a.Valor <= b.Valor;

    public override string ToString() => Valor.ToString("C2", new CultureInfo("pt-BR"));
}
