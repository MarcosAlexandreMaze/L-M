namespace LMStore.Domain.Interfaces;

public interface IUnitOfWork
{
    // Fecha a transação implícita do EF Core (todas as mudanças rastreadas viram UM
    // conjunto de INSERT/UPDATE/DELETE). Adicionar/Remover nos repositórios só marca
    // intenção em memória — nada chega ao banco até este método ser chamado.
    Task<int> SalvarAsync(CancellationToken ct = default);

    // Para operações que precisam combinar um ExecuteUpdateAsync atômico (que executa
    // na hora, fora do change tracking) com um SalvarAsync subsequente na MESMA
    // transação de banco — o caso de uso concreto é a reserva de estoque no checkout.
    Task ExecutarEmTransacaoAsync(Func<Task> operacao, CancellationToken ct = default);
}
