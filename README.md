# Cobrança Cidada — Backend

API do sistema Cobrança Cidadã. Veja `CLAUDE.md` para arquitetura e convenções.

## Subir localmente

```bash
docker compose up -d postgres redis
export ASPNETCORE_ENVIRONMENT=Localhost
dotnet run --project Cobranca.Cidada.Presentation -- migrate
dotnet run --project Cobranca.Cidada.Presentation -- seed --file ../docs/dados/promessas-lula-eleicoes-2026.json
dotnet run --project Cobranca.Cidada.Presentation        # http://localhost:5399/swagger
```

Tudo em contêiner (API + Postgres + Redis): `docker compose --profile full up --build`, depois rode `migrate` e `seed` contra o banco.

## Seed

`seed --file <json> [--overwrite] [--mandate-start yyyy-MM-dd] [--mandate-end yyyy-MM-dd]`

Idempotente. Ignora IDs que já existem; `--overwrite` os atualiza (gerando revisão). Imprime relatório: criados, atualizados, ignorados e rejeitados com o motivo. Sai com código 1 se algum item foi rejeitado.

## Endpoints (v1, públicos)

- `GET /api/v1/compromissos` — `busca`, `area`, `origem`, `situacao`, `comPrazo`, `ordem` (`area`|`code`|`deadline`), `pageNumber`, `pageSize`
- `GET /api/v1/compromissos/{id}` — ex.: `ECON-006`
- `GET /api/v1/catalogo` — áreas, origens e situações

## Autenticação e voto

- `POST /api/v1/votante/codigo` e `POST /api/v1/votante/verificar` — votar só com e-mail verificado (token de votante).
- `POST /api/v1/authentication/request-registration-code`, `register`, `login`, `refresh`, `logout`; `GET`/`DELETE /api/v1/authentication/me`.
- `PUT`/`DELETE /api/v1/compromissos/{id}/voto` — body `{ "rating": "boa" | "ma" | null, "charge": true | false }`.
- `GET`/`DELETE /api/v1/votos/meus` — listar/exportar e apagar os próprios votos.
- `GET /api/v1/compromissos?minhasCobrancas=true&ordem=mais-cobradas|melhor-avaliadas|pior-avaliadas` (as duas primeiras opções exigem token só em `minhasCobrancas`).

- `POST /api/v1/authentication/request-password-reset` e `confirm-password-reset` — recuperação de senha por código.
- `POST /api/v1/authentication/request-email-change` e `confirm-email-change` (token de conta) — troca de e-mail; os votos acompanham o novo e-mail e é preciso entrar de novo.
- Votos em rajada (mesma pessoa, mesmo IP, muitas pessoas num IP) recebem `429` com `Retry-After`. Limiares em `Voting:Burst`; atrás de proxy, defina `ForwardedHeaders__Enabled=true` e os proxies confiáveis (`ForwardedHeaders__KnownProxies__0=<ip>` e/ou `ForwardedHeaders__KnownNetworks__0=<cidr>`); sem a lista a API recusa subir.

- `POST /api/v1/authentication/google` — entra com o ID token do "Entrar com o Google" (`{ "credential": "...", "acceptedPrivacyPolicy": true|false }`). Vincula à conta existente com o mesmo e-mail ou cria uma conta sem senha (exige o aceite da política). `503` se `Authentication:Google:ClientId` não estiver configurado.
- `DELETE /authentication/me` e `POST /authentication/request-email-change` aceitam `password` ou, para conta sem senha, `googleCredential` (ID token recente do Google).
- `GET /sitemap.xml` e `GET /api/v1/seo/head|body?path=/compromissos/ECON-006` — usados pelo nginx do site (SEO). Veja `docs/decisoes.md` (D8).

Configuração: `Authentication:Google:ClientId` (opcional; sem ele não há login com Google) e `Seo:SiteUrl` (endereço público do site; obrigatório em Produção).

Segredos obrigatórios em Produção: `Jwt:key`, `Voting:EmailHmacKey` (32+ caracteres) e `BrevoSettings:ApiKey`. Em desenvolvimento sem Brevo, o código chega no log da API.

## Testes

```bash
dotnet test Cobranca.Cidada.Tests   # a parte de integração sobe PostgreSQL em contêiner (requer Docker)
```
