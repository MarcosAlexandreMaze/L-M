namespace LMStore.Application.DTOs.Auth;

// Propositalmente sem campo "Perfil": registro público só pode criar Cliente. Isso
// bloqueia mass assignment de privilégio por construção — não existe campo no payload
// que um cliente malicioso possa preencher para virar Administrador.
public record RegistrarClienteRequest(
    string Nome,
    string Email,
    string Senha,
    string Cpf,
    string Telefone,
    DateOnly DataNascimento);
