using System.Linq.Expressions;

namespace LMStore.Domain.Specifications;

// Expression<Func<T,bool>>, não Func<T,bool> — precisa ser uma árvore de expressão,
// não um delegate compilado, para o EF Core conseguir traduzir o filtro para SQL e
// executar no banco. Um Func<> forçaria trazer a tabela inteira para a memória antes
// de filtrar, o que anula completamente o propósito de um filtro combinável.
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
}
