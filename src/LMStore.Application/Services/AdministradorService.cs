using LMStore.Application.DTOs.Admin.Administradores;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

// Só alcançável via [Authorize(Roles = "Administrador")] no controller — ao contrário
// do registro público (Etapa 8), que só pode criar Cliente, este é o único caminho da
// API pra criar outro Administrador, e exige já estar autenticado como um. O primeiro
// administrador do sistema vem de um seed de desenvolvimento (ver DbSeeder) — não há,
// de propósito, nenhum jeito público de se autopromover a admin.
public class AdministradorService(
    IUsuarioRepository usuarioRepository,
    IAdministradorRepository administradorRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IAdministradorService
{
    public async Task<AdministradorResponse> CriarAsync(CriarAdministradorRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);

        if (await usuarioRepository.ExistePorEmailAsync(email, ct))
            throw new ConflictException("Já existe uma conta cadastrada com este e-mail.");

        var senhaHash = passwordHasher.Hash(request.Senha);
        var usuario = new Usuario(email, senhaHash, PerfilUsuario.Administrador);
        var administrador = new Administrador(usuario.Id, request.Nome, request.Cargo);

        usuarioRepository.Adicionar(usuario);
        administradorRepository.Adicionar(administrador);
        await unitOfWork.SalvarAsync(ct);

        return new AdministradorResponse(administrador.Id, administrador.Nome, administrador.Cargo, email.Endereco);
    }
}
