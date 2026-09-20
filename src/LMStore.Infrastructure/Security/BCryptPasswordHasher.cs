using LMStore.Application.Interfaces;

namespace LMStore.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    // Custo 12 é o padrão recomendado atualmente (2^12 iterações) — equilíbrio entre
    // resistir a força bruta e não pesar demais no login legítimo. BCrypt já embute
    // um salt aleatório por hash, então duas senhas iguais geram hashes diferentes.
    private const int FatorDeCusto = 12;

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, FatorDeCusto);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
