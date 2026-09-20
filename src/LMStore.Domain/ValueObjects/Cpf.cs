using LMStore.Domain.Exceptions;

namespace LMStore.Domain.ValueObjects;

public sealed record Cpf
{
    private static readonly int[] MultiplicadoresPrimeiroDigito = [10, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] MultiplicadoresSegundoDigito = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

    public string Numero { get; }

    public Cpf(string numero)
    {
        var digitos = ApenasDigitos(numero);

        if (!EhValido(digitos))
            throw new DomainException($"'{numero}' não é um CPF válido.");

        Numero = digitos;
    }

    public string Formatado => $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";

    private static string ApenasDigitos(string numero) =>
        new([.. (numero ?? string.Empty).Where(char.IsDigit)]);

    private static bool EhValido(string cpf)
    {
        // CPFs com todos os dígitos iguais (ex.: 111.111.111-11) passam na fórmula
        // matemática abaixo, então precisam ser rejeitados explicitamente.
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        var primeiroDigito = CalcularDigitoVerificador(cpf[..9], MultiplicadoresPrimeiroDigito);
        var segundoDigito = CalcularDigitoVerificador(cpf[..9] + primeiroDigito, MultiplicadoresSegundoDigito);

        return cpf == cpf[..9] + primeiroDigito.ToString() + segundoDigito.ToString();
    }

    private static int CalcularDigitoVerificador(string baseNumerica, int[] multiplicadores)
    {
        var soma = baseNumerica.Select((digito, indice) => (digito - '0') * multiplicadores[indice]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Formatado;
}
