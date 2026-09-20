using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public partial class Categoria : Entity
{
    public string Nome { get; private set; }
    public string Slug { get; private set; }
    public Guid? CategoriaPaiId { get; private set; }
    public bool Ativa { get; private set; }

    public Categoria(string nome, Guid? categoriaPaiId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da categoria é obrigatório.");

        Nome = nome.Trim();
        Slug = GerarSlug(Nome);
        CategoriaPaiId = categoriaPaiId;
        Ativa = true;
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome da categoria é obrigatório.");

        Nome = novoNome.Trim();
        Slug = GerarSlug(Nome);
    }

    public void MoverPara(Guid? novaCategoriaPaiId)
    {
        if (novaCategoriaPaiId == Id)
            throw new DomainException("Uma categoria não pode ser sua própria categoria pai.");

        CategoriaPaiId = novaCategoriaPaiId;
    }

    public void Ativar()
    {
        if (Ativa)
            throw new DomainException("Esta categoria já está ativa.");

        Ativa = true;
    }

    public void Desativar()
    {
        if (!Ativa)
            throw new DomainException("Esta categoria já está inativa.");

        Ativa = false;
    }

    private static string GerarSlug(string nome)
    {
        var semAcentos = RemoverAcentos(nome.Trim().ToLowerInvariant());
        var slug = CaracteresNaoAlfanumericos().Replace(semAcentos, "-").Trim('-');

        if (string.IsNullOrEmpty(slug))
            throw new DomainException($"Não foi possível gerar um identificador (slug) a partir de '{nome}'.");

        return slug;
    }

    private static string RemoverAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var semMarcas = decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string([.. semMarcas]).Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex CaracteresNaoAlfanumericos();
}
