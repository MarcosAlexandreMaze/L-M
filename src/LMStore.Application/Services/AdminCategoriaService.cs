using LMStore.Application.DTOs.Admin.Categorias;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;

namespace LMStore.Application.Services;

public class AdminCategoriaService(ICategoriaRepository categoriaRepository, IUnitOfWork unitOfWork) : IAdminCategoriaService
{
    public async Task<CategoriaResponse> CriarAsync(CriarCategoriaRequest request, CancellationToken ct = default)
    {
        if (request.CategoriaPaiId is not null)
            await ValidarPaiExisteAsync(request.CategoriaPaiId.Value, ct);

        var categoria = new Categoria(request.Nome, request.CategoriaPaiId);
        categoriaRepository.Adicionar(categoria);
        await unitOfWork.SalvarAsync(ct);

        return categoria.ParaResponse();
    }

    public async Task<CategoriaResponse> RenomearAsync(
        Guid categoriaId, RenomearCategoriaRequest request, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(categoriaId, ct);
        categoria.Renomear(request.Nome);
        await unitOfWork.SalvarAsync(ct);

        return categoria.ParaResponse();
    }

    public async Task<CategoriaResponse> MoverAsync(
        Guid categoriaId, MoverCategoriaRequest request, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(categoriaId, ct);

        if (request.NovaCategoriaPaiId is not null)
        {
            await ValidarPaiExisteAsync(request.NovaCategoriaPaiId.Value, ct);
            await ValidarSemCicloAsync(categoriaId, request.NovaCategoriaPaiId.Value, ct);
        }

        categoria.MoverPara(request.NovaCategoriaPaiId);
        await unitOfWork.SalvarAsync(ct);

        return categoria.ParaResponse();
    }

    public async Task<CategoriaResponse> AtivarAsync(Guid categoriaId, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(categoriaId, ct);
        categoria.Ativar();
        await unitOfWork.SalvarAsync(ct);

        return categoria.ParaResponse();
    }

    public async Task<CategoriaResponse> DesativarAsync(Guid categoriaId, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(categoriaId, ct);
        categoria.Desativar();
        await unitOfWork.SalvarAsync(ct);

        return categoria.ParaResponse();
    }

    private async Task ValidarPaiExisteAsync(Guid categoriaPaiId, CancellationToken ct)
    {
        _ = await categoriaRepository.ObterPorIdAsync(categoriaPaiId, ct)
            ?? throw new NotFoundException($"Categoria pai '{categoriaPaiId}' não encontrada.");
    }

    // Caminha da NOVA categoria pai proposta em direção à raiz da árvore; se o caminho
    // encontrar a própria categoria sendo movida, mover para lá a tornaria ancestral
    // de si mesma (ciclo). Precisa carregar a árvore inteira porque uma única instância
    // de Categoria só enxerga o próprio pai direto — não dá pra detectar isso olhando
    // só a entidade em memória (lacuna documentada desde a Etapa 3/6, fechada aqui).
    private async Task ValidarSemCicloAsync(Guid categoriaId, Guid novaCategoriaPaiId, CancellationToken ct)
    {
        var todas = await categoriaRepository.ListarTodasAsync(ct);
        var porId = todas.ToDictionary(c => c.Id);

        Guid? atualId = novaCategoriaPaiId;

        while (atualId is not null)
        {
            if (atualId == categoriaId)
                throw new DomainException("Não é possível mover uma categoria para dentro da própria subárvore.");

            atualId = porId.TryGetValue(atualId.Value, out var categoriaAtual) ? categoriaAtual.CategoriaPaiId : null;
        }
    }

    private async Task<Categoria> ObterOuFalharAsync(Guid categoriaId, CancellationToken ct) =>
        await categoriaRepository.ObterPorIdAsync(categoriaId, ct)
            ?? throw new NotFoundException($"Categoria '{categoriaId}' não encontrada.");
}
