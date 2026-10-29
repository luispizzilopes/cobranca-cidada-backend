# Cobrança Cidada — Backend

## Visão Geral

API REST em .NET/C# para acompanhar compromissos de campanha de governantes: catálogo de compromissos, situação de execução editorial (com fonte e histórico), curadoria e, nas próximas fases, votos de cidadãos ("Cobrar" e "Boa/Má proposta").

Segue o padrão de desenvolvimento do Kerdos (`../../kerdos/kerdos-backend`): mesma arquitetura, estrutura de pastas, convenções e pipeline.

## Stack

- **Runtime:** .NET 8, ASP.NET Core Web API
- **ORM:** Entity Framework Core (PostgreSQL / Npgsql)
- **CQRS/Mediator:** MediatR · **Validação:** FluentValidation · **Erros:** ErrorOr
- **Cache:** Redis (`RedisCacheService`; `NoOpCacheService` quando não configurado)
- **Email:** Brevo · **Logs:** Serilog · **Testes:** xUnit + Testcontainers (PostgreSQL real)

## Comandos Essenciais

```bash
docker compose up -d postgres redis          # dependências locais (Postgres na 5433, Redis na 6380)
dotnet build cobranca-cidada.sln
dotnet test Cobranca.Cidada.Tests            # integração exige Docker
ASPNETCORE_ENVIRONMENT=Localhost dotnet run --project Cobranca.Cidada.Presentation -- migrate
ASPNETCORE_ENVIRONMENT=Localhost dotnet run --project Cobranca.Cidada.Presentation -- seed --file <caminho>/promessas-lula-eleicoes-2026.json
ASPNETCORE_ENVIRONMENT=Localhost dotnet run --project Cobranca.Cidada.Presentation   # API em http://localhost:5399/swagger

dotnet ef migrations add <Nome> --project Cobranca.Cidada.Infrastructure --startup-project Cobranca.Cidada.Presentation --output-dir Migrations
```

## Arquitetura

Clean Architecture + DDD. Dependências apontam para dentro: Presentation → Application → Domain. Infrastructure implementa as interfaces do Domain.

```
Cobranca.Cidada.Domain          ← núcleo (Modules/<Módulo>/{Aggregates,Entities,ValueObjects,Enums,Rules,Filters,Constants})
Cobranca.Cidada.Application     ← Modules/<Módulo>/{Commands,Queries,UseCases,Requests,Responses,Validators,Services}, Behaviors, Common
Cobranca.Cidada.Infrastructure  ← Context, Configuration (EF), Repositories, UnitOfWork, Services, Interceptor, Migrations
Cobranca.Cidada.Presentation    ← Controllers/V1, Extensions, Middlewares, Swagger, Cli (migrate/seed), Program.cs
Cobranca.Cidada.BuildingBlocks  ← Entity, AggregateRoot, ValueObject, eventos de domínio, paginação
Cobranca.Cidada.CrossCutting    ← IoC (registro de DI)
Cobranca.Cidada.BackgroundJobs  ← jobs agendados (agregados de votos, fase 1+)
Cobranca.Cidada.Tests           ← Domain/, Application/, Integration/ (Postgres real em contêiner)
```

## Módulos

| Módulo | Responsabilidade |
|--------|-----------------|
| `Commitment` | Compromisso (agregado), fontes, histórico imutável de situação (`StatusEntry`), revisões, consulta pública, seed |
| `Catalog` | Áreas e origens (tabelas de apoio editáveis) |
| `Mandate` | Agente político (`Official`) e mandato; todo compromisso pertence a um mandato |
| `Audit` | `AuditLog` de ações de curadoria |
| `Auth` | Cadastro com código por e-mail, login (senha ou Google), refresh rotativo, logout, perfil, exclusão de conta, recuperação de senha e troca de e-mail (Identity EF Core + JWT) |
| `Seo` | `GET /sitemap.xml` e os trechos de HTML (`/api/v1/seo/head` e `/body`) que o nginx do site injeta no `index.html` de cada compromisso: título, descrição, canonical, Open Graph, JSON-LD e um resumo do conteúdo |
| `Admin` (transversal) | Painel de administração: `/admin/compromissos` (listar, criar, editar conteúdo/prazo/fontes com revisão, registrar situação, publicar/despublicar) e `/admin/catalogo` (renomear área/origem). Política `Admin` |
| `Comment` | Comentários públicos sobre compromissos (só com e-mail verificado): listar (público, paginado), destaques da tela inicial (`GET /comments/recent`: o último comentário raiz de cada compromisso publicado, sem respostas), comentar, apagar o próprio e moderação por admin (`DELETE /admin/comments/{id}`, com `AuditLog`) |
| `Moderation` | Tabela `ProfanityWords` (palavras/expressões a censurar nos comentários, pt e en, com variações). `Word` já vem normalizada (minúscula, sem acento, hífen/espaços viram um espaço; use `ProfanityWord.Normalize` ao comparar), única, e `IsActive` desliga sem apagar. Carga inicial na migração `AddProfanityWords`. `AddCommentUseCase` censura (`ProfanityFilter`, lista em cache 10 min) e **grava o texto já censurado** (`merda` → `m****`); as leituras não censuram de novo. Comentários anteriores à censura ficam como estavam |
| `Voting` | Voto só com e-mail verificado (token de votante), votar/desfazer, meus votos, exclusão dos próprios votos, detecção de rajadas (`VoteBurstGuard`) |

## Regras que não podem quebrar

- **Os compromissos vivem só no banco.** Nenhum endpoint, job ou tela lê o JSON. O JSON entra apenas pelo comando `seed`, que é idempotente (chave natural `ExternalId`), ignora IDs existentes e só atualiza com `--overwrite`.
- **Regras de situação em um lugar só:** `Domain/Modules/Commitment/Rules/StatusRules.cs`, usado pelo agregado, pelo `CommitmentInputValidator` (seed e painel de curadoria). Nunca "Descumprido" antes do prazo; sem prazo estruturado, só com ato formal e fonte.
- **Voto não altera situação.** Contagens de voto nunca entram em `CurrentStatus`.
- **Sem dados pessoais em logs.** Nenhum e-mail ou identidade de votante em mensagens de log.
- **O voto nunca guarda e-mail nem usuário:** só `VoterKey` (HMAC do e-mail canônico, `IVoterKeyService`). Contas e votantes sem conta com o mesmo e-mail compartilham a chave.
- **Dois tipos de token:** conta (`typ=user`, com `sid` e papéis) e votante (`typ=voter`). Políticas: `Account` (só conta) e `Voter` (qualquer um com `vk`). Endpoints de votação usam `Voter`; perfil e exclusão de conta usam `Account`.
- **Trocar o e-mail muda a chave do votante:** a troca migra os votos para a chave nova na mesma transação (`ExecuteInTransactionAsync`) e encerra as sessões. Qualquer novo fluxo que altere o e-mail precisa fazer o mesmo.
- **Rajadas:** todo voto passa pelo `VoteBurstGuard` (limiares em `Voting:Burst`). Contadores e logs só com valores pseudonimizados (`IVoterKeyService.Pseudonymize`), nunca IP ou e-mail em claro. Atrás de proxy, ligue `ForwardedHeaders:Enabled` e informe `KnownProxies`/`KnownNetworks` (obrigatórios quando ligado). Códigos de e-mail: tentativas contadas de forma atômica por e-mail+finalidade (não zeram ao reenviar). Login: bloqueio por par IP+conta (mais um limite alto por conta), resposta de falha idêntica e hash fictício para conta inexistente.
- **Login com Google** (`POST /authentication/google`, `GoogleLoginUseCase`): o ID token é validado por `IGoogleIdentityVerifier` (`Google.Apis.Auth`: assinatura, emissor, validade e `aud` = `Authentication:Google:ClientId`; sem o ClientId a rota responde 503). O vínculo fica em `AspNetUserLogins` (provedor `Google` + `sub`), então senha e Google levam ao **mesmo usuário**. Ordem: (1) `sub` já vinculado entra; (2) senão, conta com o mesmo e-mail (ignora maiúsculas) é vinculada e entra, **só se o Google marcou `email_verified`** (sem isso qualquer um assumiria a conta de outra pessoa); uma conta que já tem outro `sub` Google não aceita um segundo; (3) senão cria conta **sem senha** e exige `acceptedPrivacyPolicy` (`Auth.ConsentRequired`). Alias de Gmail (pontos, `+tag`) não é casado com a conta: entra como conta nova, mas os votos se unem pela `VoterKey`.
- **Conta sem senha:** `ProfileResponse.HasPassword=false`. Login por senha falha igual a qualquer credencial errada; trocar senha responde `Auth.NoPassword`; a senha se cria pelo "Esqueci minha senha". Ações que pedem a senha (excluir conta, trocar e-mail) passam por `AccountReauthenticationService`: com senha, confere a senha (o ID token do Google **não** a substitui); sem senha, exige um ID token do Google emitido nos últimos 10 min e do `sub` vinculado.
- **SEO:** `GetSeoFragmentUseCase` só devolve conteúdo para `/compromissos/{slug}` (publicado = etiquetas completas; inexistente ou despublicado = `noindex`; qualquer outra rota = vazio, e vale o padrão do `index.html`). Nada de voto vai para o HTML. Todo texto vindo do banco passa por `SeoPageRenderer.E`; o JSON-LD pelo serializador JSON (que escapa `<`). Cache de 60 s por página (sitemap, 10 min) via `ICacheService`. `Seo:SiteUrl` é obrigatório em Produção e não pode ser `localhost`. A política `seo` tem limite alto porque quem chama é o nginx (um só IP).
- **Administrador = flag `IsAdmin` em `AspNetUsers`.** Nenhum endpoint concede; só banco ou `dotnet run --project Cobranca.Cidada.Presentation -- set-admin --email <e-mail> [--revoke]`. O login emite a claim `admin=true` e o papel `Admin` (papel `Admin` avulso do Identity não vale). A política `Admin` exige token de conta + claim, e `DropStaleAdminClaimsAsync` reconfere a flag no banco a cada requisição: revogar vale na hora. Quem foi promovido recebe a claim no próximo login/refresh.
- Criação pelo painel (`POST /admin/compromissos`): nasce despublicado (a menos que `publish`), situação "Não avaliado", mandato mais recente, identificador público informado ou o próximo da área (`ECON-050`), sem `ExternalId`; mesma validação do seed.
- Edição pelo painel: `ExpectedVersion` obrigatório (409 se desatualizado), motivo obrigatório, versão anterior vira `CommitmentRevision`, validação pelo mesmo `CommitmentInputValidator` do seed, ação no `AuditLog`. Mudar o prazo não pode invalidar um "Descumprido" já registrado.
- **Comentário é opinião e anônimo para o público:** guarda só `VoterKey` (nunca e-mail, nome ou usuário) e a resposta só diz `isMine`. Texto de 3 a 1000 caracteres, até 5 comentários **raiz** por pessoa por compromisso (respostas, via `ParentId`, não têm esse limite e ficam em um só nível: responder a uma resposta cai na mesma conversa; apagar a raiz apaga as respostas), política de rate limit `comment`. Apagados junto com a conta e migrados na troca de e-mail (como os votos). Apagar "meus votos" não apaga comentários.
- Contagens públicas só com 20+ votos (`VotePolicy`); nunca devolver votos individuais.
- Não invente dados: toda afirmação sobre promessa ou execução precisa de fonte com URL.

## Padrões (idênticos ao Kerdos)

- Controllers finos: `_mediator.Send(...)` e `result.ToActionResult(this)`.
- Command/Query → Handler fino → UseCase (`IUseCase`, registrado por varredura) → `IUnitOfWork`. Nunca injete repositório direto em UseCase.
- `ErrorOr<T>` para erros de negócio; validação de entrada no `Validator` (mensagens em português).
- `AsNoTracking()` em leituras. Nunca exponha entidade de domínio: use `Responses`.
- IDs são value objects (`CommitmentId` etc.) com conversão no `IEntityTypeConfiguration`.
- Rotas: `api/v{version}/<recurso>`; recursos públicos em português (`/compromissos`, `/catalogo`).
- Nomes: `<Ação><Módulo>Command|Query|UseCase|CommandValidator`, `I<Módulo>Repository`.

## Testes

- `Domain/` — agregados e regras puras (`StatusRulesTests`, `CommitmentTests`).
- `Application/` — `SeoPageRendererTests` (escape de HTML e JSON-LD, tamanho da descrição, links só http(s), sitemap), validators, mapper e **contrato do arquivo de seed** (falha o build com ID duplicado, status inválido, URL malformada).
- `Integration/` — `GoogleLoginIntegrationTests` (verificador falso do Google em `ApiFactory`: conta nova com aceite, vínculo por e-mail, mesmo usuário por senha e Google, e-mail não verificado, conta inativa, conta sem senha, exclusão e troca de e-mail por credencial Google) e `SeoIntegrationTests` (head/body de compromisso publicado, `noindex` para inexistente e despublicado, vazio nas demais rotas, sitemap só com publicados). Postgres real via Testcontainers: seed idempotente, seed não sobrescreve edição, filtros/paginação da API, "não depende do JSON em execução", fluxos de conta (cadastro, login, bloqueio, refresh, logout, exclusão) e de voto (só com e-mail, desfazer, unificação com a conta, contagem mínima, ordenação, "minhas cobranças"). O e-mail é capturado por `CapturingEmailService`. Os contadores de rajada têm teste contra Redis real (Testcontainers).
- Nunca mocke o banco.
