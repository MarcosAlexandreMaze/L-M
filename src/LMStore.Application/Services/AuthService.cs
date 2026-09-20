using System.Security.Cryptography;
using System.Text;
using LMStore.Application.Common;
using LMStore.Application.DTOs.Auth;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace LMStore.Application.Services;

public class AuthService(
    IUsuarioRepository usuarioRepository,
    IClienteRepository clienteRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResultado> RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);

        if (await usuarioRepository.ExistePorEmailAsync(email, ct))
            throw new ConflictException("Já existe uma conta cadastrada com este e-mail.");

        var cpf = new Cpf(request.Cpf);

        if (await clienteRepository.ExistePorCpfAsync(cpf, ct))
            throw new ConflictException("Já existe um cliente cadastrado com este CPF.");

        var senhaHash = passwordHasher.Hash(request.Senha);

        // Registro público sempre cria PerfilUsuario.Cliente — nunca aceito de fora.
        var usuario = new Usuario(email, senhaHash, PerfilUsuario.Cliente);
        var cliente = new Cliente(usuario.Id, request.Nome, cpf, request.Telefone, request.DataNascimento);

        usuarioRepository.Adicionar(usuario);
        clienteRepository.Adicionar(cliente);

        var resultado = EmitirTokens(usuario);
        await unitOfWork.SalvarAsync(ct);

        return resultado;
    }

    public async Task<AuthResultado> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);
        var usuario = await usuarioRepository.ObterPorEmailAsync(email, ct);

        // Mensagem idêntica para "e-mail não existe" e "senha errada" de propósito —
        // diferenciar os dois casos permite a um atacante enumerar quais e-mails têm
        // conta cadastrada (username enumeration), então a resposta é sempre a mesma.
        if (usuario is null || !usuario.Ativo || !passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
            throw new DomainException("E-mail ou senha inválidos.");

        var resultado = EmitirTokens(usuario);
        await unitOfWork.SalvarAsync(ct);

        return resultado;
    }

    public async Task<AuthResultado> RefreshAsync(string refreshTokenBruto, CancellationToken ct = default)
    {
        var hashAtual = CalcularHash(refreshTokenBruto);
        var usuario = await usuarioRepository.ObterPorRefreshTokenHashAsync(hashAtual, ct)
            ?? throw new DomainException("Refresh token inválido.");

        if (!usuario.Ativo)
            throw new DomainException("Este usuário está inativo.");

        var novoTokenBruto = GerarTokenAleatorio();
        var novoRefreshToken = usuario.RotacionarRefreshToken(
            hashAtual, CalcularHash(novoTokenBruto), TimeSpan.FromDays(_jwtOptions.RefreshTokenDias));

        await unitOfWork.SalvarAsync(ct);

        return new AuthResultado(
            jwtTokenGenerator.GerarAccessToken(usuario),
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutos),
            novoTokenBruto,
            novoRefreshToken.ExpiraEm);
    }

    private AuthResultado EmitirTokens(Usuario usuario)
    {
        var tokenBruto = GerarTokenAleatorio();
        var refreshToken = usuario.EmitirRefreshToken(CalcularHash(tokenBruto), TimeSpan.FromDays(_jwtOptions.RefreshTokenDias));

        return new AuthResultado(
            jwtTokenGenerator.GerarAccessToken(usuario),
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutos),
            tokenBruto,
            refreshToken.ExpiraEm);
    }

    private static string GerarTokenAleatorio() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string CalcularHash(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}
