namespace LMStore.Application.DTOs.Catalogo;

public record CategoriaResponse(Guid Id, string Nome, string Slug, Guid? CategoriaPaiId, bool Ativa);
