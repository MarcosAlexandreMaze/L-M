# L&M Store API

API de e-commerce (C#/.NET 10) para uma loja de roupas fitness e tênis. Cobre o ciclo completo do negócio — catálogo com variações e estoque real, carrinho, cupons, checkout com máquina de estados, pagamento abstraído por gateway e administração — construída em Clean Architecture com regras de negócio reais no domínio, não CRUD superficial.

Este projeto tem um segundo objetivo, tão importante quanto o primeiro: servir de material de estudo de POO, SOLID e Clean Architecture em C#. Vários comentários no código explicam o *porquê* de uma decisão, não o óbvio do *o quê*.

## Sumário

- [Arquitetura](#arquitetura)
- [Stack técnica](#stack-técnica)
- [Regras de negócio centrais](#regras-de-negócio-centrais)
- [Como rodar localmente (LocalDB)](#como-rodar-localmente-localdb)
- [Como rodar com Docker](#como-rodar-com-docker)
- [Usando o Swagger](#usando-o-swagger)
- [Explorando o banco de dados](#explorando-o-banco-de-dados)
- [Testes](#testes)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Segurança](#segurança)

## Arquitetura

Clean Architecture em 4 camadas, com dependência apontando sempre para dentro (o Domain não depende de nada):

```
LMStore.Api             → Controllers finos, middlewares, DI (composition root), Swagger, JWT bearer
        ↓ depende de
LMStore.Application     → Casos de uso/serviços, DTOs, validadores (FluentValidation), interfaces técnicas
        ↓ depende de                                  (IPaymentGateway, IShippingCalculator, etc.)
LMStore.Domain          ←── implementa ──── LMStore.Infrastructure
   (zero dependências)        interfaces        EF Core, repositórios, JWT/hash, gateways stub
```

- **Domain**: entidades ricas (com comportamento, não anêmicas), Value Objects, enums, interfaces de repositório, exceções de domínio, especificações de filtro. Não referencia nenhuma outra camada.
- **Application**: orquestra casos de uso chamando o Domain e interfaces técnicas; DTOs e validação de forma de entrada (a regra de negócio em si vive no Domain).
- **Infrastructure**: implementa as interfaces definidas em Domain/Application — EF Core, hashing de senha, geração de JWT, gateways de pagamento/frete (stubs, para não depender de serviços externos reais).
- **Api**: só traduz HTTP ⇄ Application. Sem regra de negócio em controller.

O domínio é modelado com nomes em **português** (`Pedido`, `Cancelar`, `StatusPedido`...) porque é a linguagem que o próprio negócio usa — termos puramente técnicos de infraestrutura ficam em inglês, como é convenção no mercado.

Decisões de POO/design notáveis (explicadas em profundidade nos comentários do próprio código, junto ao trecho relevante):

| Decisão | Resumo do porquê |
|---|---|
| `Produto` abstrata + `Roupa`/`Tenis` (herança, TPH) | Uso polimórfico real — o catálogo é sempre navegado como `IEnumerable<Produto>`. |
| `Usuario` independente de `Cliente`/`Administrador` (composição, não herança) | Autenticação é transversal; não existe "listar todos os Usuarios" na UI. |
| `Estoque` como aggregate root próprio | Isola a concorrência de escrita (compras) da edição de catálogo; permite reserva atômica via `ExecuteUpdateAsync`. |
| Repositórios só por aggregate root, sem `IRepository<T>` genérico | `DbContext` já é um Unit of Work; repositório genérico seria só cerimônia. |
| Auth manual (JWT + BCrypt), não ASP.NET Core Identity | `IdentityUser` é anêmico e imporia seu próprio schema — contra o objetivo didático de POO real. |
| Specification pattern no catálogo | Filtros combináveis (categoria+marca+preço+gênero+tamanho) sem explosão de métodos no repositório. |

## Stack técnica

- **.NET 10** / ASP.NET Core Web API
- **EF Core 10** + SQL Server (LocalDB em dev, container em Docker)
- **JWT Bearer** (customizado) + refresh token rotativo via cookie `HttpOnly`/`Secure`/`SameSite=Strict`
- **BCrypt** para hash de senha
- **FluentValidation** para validação de entrada
- **Swashbuckle** (Swagger/OpenAPI)
- **xUnit** para testes (unitários de domínio + integração contra banco real)

## Regras de negócio centrais

- Uma variação sem estoque disponível não pode ser vendida; a reserva nunca excede o disponível (reserva via `UPDATE` condicional atômico, sem depender de lock otimista no caminho crítico de compra).
- Pedido segue máquina de estados estrita — transições inválidas lançam exceção de domínio (400).
- Pedido entregue ou enviado nunca pode ser cancelado por este fluxo (cancelamento pós-envio seria um fluxo de devolução, fora do escopo desta v1).
- Pagamento recusado não aprova nem cancela o pedido automaticamente — ele permanece aguardando novo pagamento (`POST /api/pedidos/{id}/pagamentos`).
- Cupom expirado, ainda não iniciado, inativo, abaixo do valor mínimo ou com limite de uso esgotado não pode ser aplicado.
- CPF e e-mail são únicos — checado na camada de Application **e** garantido por índice único no banco (dupla proteção contra condição de corrida).
- Preço/nome/SKU de um item de pedido são "congelados" no momento da compra (snapshot) — editar o produto depois não altera pedidos já feitos.
- A reserva de estoque acontece na **criação do pedido**, não ao adicionar ao carrinho — evita que um carrinho abandonado prenda a última unidade disponível indefinidamente.

## Como rodar localmente (LocalDB)

Pré-requisitos: .NET SDK 10, SQL Server LocalDB (já vem com o Visual Studio; senão, instale o [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)).

```bash
# 1. Configure a chave de assinatura do JWT (nunca vai para appsettings.json/git)
dotnet user-secrets set "Jwt:SigningKey" "uma-string-aleatoria-de-32-ou-mais-caracteres" --project src/LMStore.Api

# 2. Rode a API
dotnet run --project src/LMStore.Api
```

Em ambiente `Development` (padrão do `dotnet run`), o `Program.cs` aplica as migrations pendentes e semeia automaticamente:
- Marca **L&M** + categoria **Fitness** + dois produtos iniciais (Camiseta e Short dry-fit), cada um com variações e estoque;
- Um administrador padrão: `admin@lmstore.com` / `AdminLM@2026` — **troque essa senha antes de qualquer uso além de estudo/dev.**

A API sobe em `https://localhost:7106` (e `http://localhost:5209`), conforme `src/LMStore.Api/Properties/launchSettings.json`.

## Como rodar com Docker

Alternativa que não depende de LocalDB nem de instalar nada além do Docker — sobe a API e um container SQL Server juntos.

```bash
# 1. Copie o template de variáveis de ambiente e preencha com valores reais
cp .env.example .env
# edite .env: defina MSSQL_SA_PASSWORD e JWT_SIGNING_KEY

# 2. Suba tudo
docker compose up --build
```

A API fica disponível em `http://localhost:8080` (Swagger em `http://localhost:8080/swagger`). O container sobe com `ASPNETCORE_ENVIRONMENT=Development`, então migrations e seed rodam automaticamente, igual ao fluxo local — é pensado para demonstração/estudo, não para um deploy de produção real (que exigiria gestão de segredos via Docker secrets/Key Vault, HTTPS terminado por um proxy reverso, pipeline de migration separado, etc. — fora do escopo desta v1).

> Setup de Docker escrito e revisado, mas não pôde ser testado de ponta a ponta nesta máquina (Docker não está instalado aqui). Se algo não subir de primeira, o suspeito nº 1 é o `.env` não preenchido.

## Usando o Swagger

1. Abra `/swagger` na URL da API (local ou Docker). Em `https://localhost:7106`, o certificado de desenvolvimento não é confiável no navegador — aceite o aviso avançado na primeira vez.
2. Endpoints são agrupados por controller (Auth, Produtos, Carrinho, Pedidos, Admin/...).
3. Endpoints públicos (ex.: `GET /api/produtos`): "Try it out" → "Execute", sem autenticação.
4. Endpoints autenticados:
   - `POST /api/auth/login` com e-mail/senha (use o admin semeado para testar `/api/admin/*`, ou registre um cliente novo via `POST /api/auth/registrar`).
   - Copie o `accessToken` da resposta.
   - Clique em **"Authorize"** (cadeado no topo) e cole só o token (sem o prefixo `Bearer `).
   - Todo "Try it out" seguinte manda o header `Authorization` automaticamente.
   - O access token expira em 15 minutos — depois disso, é só logar de novo.

## Explorando o banco de dados

O banco (LocalDB local ou o container Docker) é SQL Server — qualquer cliente SQL Server padrão funciona:

**Extensão do VS Code — "SQL Server (mssql)" (mais simples):**
- Local: Server `(localdb)\mssqllocaldb`, Database `LMStoreDb`, Windows Authentication (sem usuário/senha).
- Docker: Server `localhost,1433`, Database `LMStoreDb`, usuário `sa`, senha = o valor de `MSSQL_SA_PASSWORD` no seu `.env`.

**Azure Data Studio / SSMS**: mesmos parâmetros de conexão acima.

Tabelas principais: `Usuarios`, `Clientes`, `Enderecos`, `Produtos` (TPH — Roupas e Tênis na mesma tabela, diferenciados pela coluna discriminadora), `VariacaoProdutos`, `Estoques`, `MovimentoEstoques`, `Carrinhos`, `Cupons`, `Pedidos`.

## Testes

```bash
# Suíte rápida — testes de domínio, em memória, sem dependência de banco
dotnet test --filter "Category!=Integration"

# Suíte de integração — precisa de LocalDB rodando com a connection string padrão
dotnet test --filter "Category=Integration"
```

Regras de domínio (estoque insuficiente, cupom expirado, transição de status inválida, e-mail/CPF duplicado, cálculo de total/desconto) são cobertas por casos de teste unitário explícitos. Os testes de integração validam comportamento real do EF Core contra SQL Server — inclusive dois bugs reais de materialização/mapeamento que só apareceram testando contra o banco de verdade (documentados nos comentários dos próprios testes).

## Estrutura de pastas

```
src/
  LMStore.Domain/          Entidades, Value Objects, enums, interfaces de repositório, exceções, specifications
  LMStore.Application/     Serviços de caso de uso, DTOs, validadores, mappings, interfaces técnicas
  LMStore.Infrastructure/  EF Core (DbContext, configurations, repositórios, migrations), segurança, gateways stub
  LMStore.Api/             Controllers, middlewares, composition root (Program.cs), Swagger
tests/
  LMStore.Tests/           Domain/ (unitários) e Integration/ (contra SQL Server real)
Dockerfile, docker-compose.yml, .env.example
```

## Segurança

Itens da checklist de hardening pedida, e o estado real de cada um:

**Implementados:**
- Chave de assinatura JWT fora do código-fonte (User Secrets em dev; variável de ambiente em Docker/produção) — nunca em `appsettings.json`.
- Senhas com hash BCrypt, nunca texto puro.
- Autenticação e autorização (`[Authorize(Roles=...)]`) sempre validadas no servidor — nada de checagem só no cliente.
- Registro público restrito: só cria `Cliente`; nenhum endpoint permite auto-promoção a `Administrador`.
- Proteção contra mass assignment: toda entrada é um DTO explícito mapeado manualmente para o domínio, nunca bind direto em entidade.
- Refresh token: cookie `HttpOnly` + `Secure` + `SameSite=Strict`, valor persistido como hash (nunca o token bruto), com rotação a cada uso.
- Rate limiting: 100 req/min por IP globalmente, 10 req/min por IP nos endpoints de `/api/auth` (alvo típico de força bruta).
- Validação de entrada via FluentValidation em todos os DTOs de escrita.
- Security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, etc.) via middleware dedicado.
- HTTPS forçado (`UseHttpsRedirection`) + HSTS fora de ambiente de desenvolvimento.
- Erros padronizados sem vazar stack trace ao cliente — mensagem genérica em 500, detalhe real só no log server-side.
- Scan de dependências (`dotnet list package --vulnerable`) rodado — nenhuma vulnerabilidade conhecida nas dependências atuais.
- `.gitignore` cobre `.env`, `appsettings.*.Development.json` com segredo e pastas de build — nenhum segredo real chegou a ser commitado neste repositório.

**Deliberadamente adiados** (e por quê):
- **Row-Level Security (RLS)**: é um recurso do próprio SQL Server, mas essa aplicação já isola dados por cliente inteiramente na camada de Application (toda query de pedido/endereço/carrinho filtra por `ClienteId` do usuário autenticado — nunca um Id "cru" vindo do cliente). RLS no banco seria uma segunda camada de defesa redundante; vale reavaliar se o banco algum dia for acessado por múltiplas aplicações que não compartilham essa mesma lógica de Application.
- **Criptografia de CPF em repouso**: hoje o CPF fica em texto puro no banco (validado e normalizado, mas não cifrado). Criptografia em coluna exigiria decidir gestão de chave (Azure Key Vault, DPAPI, etc.) — proposital adiar para quando houver um ambiente de produção real definido, para não inventar uma estratégia de chave "de brinquedo" que passe falsa sensação de segurança.
- **Bot protection (CAPTCHA)**: não implementado — exigiria integração com um serviço de terceiros (Cloudflare Turnstile, hCaptcha...). Rate limiting em `/api/auth` cobre parcialmente o mesmo risco (força bruta), mas não bots de registro em massa.
- **Restrição de upload de arquivos**: não se aplica ainda — a v1 não tem nenhum endpoint de upload (fotos de produto, por exemplo, seriam adicionadas por URL/seed, não por upload). Revisitar se um endpoint de upload for adicionado.
- **Docker/imagem em produção real**: o `docker-compose.yml` deste repositório é para demonstração local (roda em modo `Development`, HTTP puro). Um deploy real precisaria de TLS terminado por um proxy reverso, segredos via um vault de verdade, e um pipeline de migration que não rode automaticamente a cada boot do container.
