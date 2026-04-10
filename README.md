# MPFinance

MPFinance é um sistema web de controle financeiro pessoal com autenticação segura, verificação de e-mail, registro de transações, categorização, metas financeiras e transações fixas recorrentes.

---

## Sumário

- [Arquitetura do Sistema](#arquitetura-do-sistema)
- [Design Patterns Utilizados](#design-patterns-utilizados)
- [Modelagem do Banco de Dados](#modelagem-do-banco-de-dados)
- [Fluxo de Autenticação](#fluxo-de-autenticação)
- [Como Rodar Localmente](#como-rodar-localmente)
- [Deploy com Docker](#deploy-com-docker)
- [Padronização de Commits](#padronização-de-commits)

---

## Arquitetura do Sistema

### Análise das Necessidades

O MPFinance foi projetado com base em quatro dimensões críticas:

**Escalabilidade:** O sistema atende usuários individuais com sessões independentes. O volume de dados por usuário é previsível, portanto uma única instância bem dimensionada suporta milhares de usuários simultâneos.

**Manutenção:** Por tratar de dados financeiros, a manutenção é prioritária. A arquitetura permite corrigir e evoluir partes isoladas sem afetar o conjunto.

**Complexidade:** A lógica envolve regras financeiras (precisão decimal, metas com depósito progressivo), segurança (JWT, BCrypt, verificação por código OTP) e integrações externas (SMTP). Complexidade moderada — suficiente para justificar separação de responsabilidades, mas não para exigir múltiplos serviços independentes.

**Flexibilidade:** O sistema absorve evolução (relatórios, Open Finance, notificações push) sem reescrever o núcleo.

### Escolha: Monólito com Clean Architecture + Elementos Orientados a Eventos

Com base na análise e em referências reais (YNAB iniciou monolítico; Nubank adotou microsserviços apenas por ter múltiplos times e domínios regulatoriamente isolados), o MPFinance adota **arquitetura monolítica estruturada internamente em Clean Architecture**, complementada por **comunicação orientada a eventos** via MediatR.

#### Por que não microsserviços?

| Critério | Monólito | Microsserviços |
|---|---|---|
| Time de desenvolvimento | 1 desenvolvedor | Requer times distribuídos |
| Overhead operacional | Baixo (1 processo, 1 banco) | Alto (service mesh, mensageria, múltiplos bancos) |
| Latência entre domínios | Zero (in-process) | Latência de rede |
| Consistência de dados | ACID nativo | Eventual consistency complexa |
| Debugabilidade | Stack trace linear | Rastreamento distribuído |

#### Estrutura em Camadas (Clean Architecture)

```
┌─────────────────────────────────────────────┐
│              MPFinance.API                  │  ← Apresentação
│         Controllers, Program.cs             │
├─────────────────────────────────────────────┤
│           MPFinance.Application             │  ← Casos de Uso
│      Facades, Handlers (MediatR), DTOs      │
├─────────────────────────────────────────────┤
│             MPFinance.Domain                │  ← Núcleo de Negócio
│   Entities, Interfaces, Events, Factories   │
├─────────────────────────────────────────────┤
│          MPFinance.Infrastructure           │  ← Detalhes Técnicos
│   EF Core, Repositories, SMTP, BCrypt       │
└─────────────────────────────────────────────┘
```

A **regra de dependência** garante que camadas internas não conhecem camadas externas: o Domain não referencia o Infrastructure, o Application não conhece o EF Core. Toda dependência aponta para dentro.

#### Diagrama de Componentes

```
┌──────────────┐     HTTP/REST      ┌─────────────────────┐
│   Angular 21  │ ◄──────────────── │   ASP.NET Core 10   │
│  (Frontend)   │ ────────────────► │   (AuthController)  │
└──────────────┘        JWT         └──────────┬──────────┘
                                               │ DI
                              ┌────────────────▼──────────────────┐
                              │         Application Layer          │
                              │  FinancialFacade | WelcomeHandler  │
                              │         MediatR (Events)           │
                              └────────────────┬──────────────────┘
                                               │ Interfaces
                              ┌────────────────▼──────────────────┐
                              │           Domain Layer             │
                              │  User, Goal, Transaction, Category │
                              │  IUserRepo, IEmailService, etc.    │
                              └────────────────┬──────────────────┘
                                               │ Implementações
                              ┌────────────────▼──────────────────┐
                              │        Infrastructure Layer        │
                              │  EF Core + MySQL  │  MailKit/SMTP  │
                              │  BCrypt Hasher    │  Repositories  │
                              └───────────────────────────────────┘

                              ┌───────────────────────────────────┐
                              │          Docker Compose           │
                              │  mpfinance_api + mpfinance_db     │
                              └───────────────────────────────────┘
```

### Stack Tecnológica

| Camada | Tecnologia | Justificativa |
|---|---|---|
| Frontend | Angular 21 + Signals | Componentes standalone com estado reativo, sem gerenciador externo |
| Backend | ASP.NET Core 10 (.NET 10) | Alta performance, DI nativo, Clean Architecture out-of-the-box |
| ORM | Entity Framework Core 9 | Migrations automáticas, abstração do banco preservando o domínio |
| Banco de dados | MySQL 8.0 | Relacional (ACID) adequado para dados financeiros |
| Mensageria interna | MediatR | Padrão Observer in-process sem overhead de mensageria externa |
| E-mail | MailKit (SMTP/Gmail) | Referência .NET para SMTP com suporte a TLS |
| Contêineres | Docker Compose | Isola dependências e padroniza ambientes (dev e produção) |
| Autenticação | JWT (HS256) | Stateless, compatível com SPA, expiração configurável |
| Segurança | BCrypt | Hash adaptativo com salt automático |

---

## Design Patterns Utilizados

### 1. Repository Pattern
Isola a camada de acesso a dados. `IBaseRepository<T>` fornece CRUD padrão evitando duplicação, com repositórios especializados (`UserRepository`, `TransactionRepository`, etc.) para consultas específicas de domínio.

### 2. Facade Pattern (Estrutural)
`FinancialFacade` centraliza fluxos complexos entre múltiplos repositórios. Exemplo: criar uma transação e atualizar automaticamente o progresso de uma meta associada — escondendo essa orquestração dos Controllers.

### 3. Factory Pattern (Criacional)
`TransactionFactory` remove das demais camadas o peso de entender as regras de construção de uma transação, sempre emitindo objetos padronizados e validados.

### 4. Observer / Mediator Pattern (Comportamental)
MediatR publica e processa eventos de domínio assincronamente. Registrar um usuário (`UserRegisteredEvent`) dispara o `WelcomeEmailHandler` sem acoplar o Controller ao serviço de e-mail.

### 5. Strategy Pattern
`IPasswordHasher` abstrai o algoritmo de hash. A implementação (`BCryptHasher`) pode ser substituída sem alterar nenhuma regra de negócio.

---

## Modelagem do Banco de Dados

```
users ─────────────────────────────────────────────────────┐
│ id (GUID) · name · email · password_hash                  │
│ is_verified · created_at                                  │
│                                                           │
├──────────── transactions                                  │
│             id · user_id · category_id                    │ 1:N para todas
│             amount · description · date                   │ as tabelas abaixo
│             is_fixed · created_at                         │
│                                                           │
├──────────── fixed_transactions                            │
│             id · user_id · category_id                    │
│             amount · description · day_of_month           │
│                                                           │
├──────────── goals                                         │
│             id · user_id · title                          │
│             target_amount · current_amount                │
│             deadline · created_at                         │
│                                                           │
├──────────── password_resets                               │
│             id · user_id · token · expires_at             │
│                                                           │
└──────────── email_verification_codes                      │
              id · user_id · code · expires_at             ─┘

categories
  id · name · type ENUM('income', 'expense')
  └── referenciada por transactions e fixed_transactions (Restrict delete)
```

---

## Fluxo de Autenticação

```
1. Register   POST /api/auth/register
              → hash BCrypt · salva usuário (IsVerified=false)
              → publica UserRegisteredEvent
              → WelcomeEmailHandler: gera código 6 dígitos (15min)
              → salva em email_verification_codes
              → envia e-mail HTML via SMTP

2. Verify     POST /api/auth/verify-email
              → valida código (userId + code + ExpiresAt > now)
              → IsVerified = true · apaga códigos do usuário

3. Login      POST /api/auth/login
              → verifica credenciais com BCrypt
              → bloqueia (403) se IsVerified = false
              → emite JWT HS256 com expiração de 2h

4. Guards     authGuard  → protege /home (exige token)
              guestGuard → protege /login, /register, /verify-email
                           (redireciona para /home se já logado)
```

---

## Como Rodar Localmente

### Pré-requisitos
- .NET 10 SDK
- Node.js 22+ e Yarn
- Docker Desktop

### 1. Banco de dados

```bash
docker-compose up -d db
```

### 2. Backend

```bash
cd backend/MPFinance.API
dotnet run
```

As migrations são aplicadas automaticamente na inicialização. API disponível em `http://localhost:5000`.  
Swagger em `http://localhost:5000/swagger`.

### 3. Frontend

```bash
cd frontend
yarn dev
```

Frontend disponível em `http://localhost:4200`.

### Variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
# Banco de dados
MYSQL_ROOT_PASSWORD=sua_senha_root
MYSQL_DATABASE=mpfinance_db
MYSQL_USER=admin_mpfinance
MYSQL_PASSWORD=sua_senha

# Backend
DB_CONNECTION_STRING="Server=localhost;Database=mpfinance_db;Uid=admin_mpfinance;Pwd=sua_senha;"

# JWT
JWT_SECRET=seu_secret_aqui_minimo_32_caracteres
JWT_ISSUER=mpfinance_api
JWT_AUDIENCE=mpfinance_angular

# E-mail (Gmail com App Password)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=seu@gmail.com
SMTP_APP_PASSWORD=xxxx xxxx xxxx xxxx
```

---

## Deploy com Docker

Para subir **banco de dados + API** juntos em um servidor (VPS, etc.):

```bash
docker-compose up -d
```

O Docker Compose garante que:
1. O MySQL sobe primeiro e passa pelo health check
2. A API só inicia quando o banco está pronto para aceitar conexões
3. A connection string é automaticamente ajustada para o nome do serviço interno (`db`)

| Serviço | Porta exposta | Descrição |
|---|---|---|
| `mpfinance_db` | 3306 | MySQL 8.0 |
| `mpfinance_api` | 5000 | ASP.NET Core API |

Para rebuild após alterações no código:

```bash
docker-compose up -d --build api
```

Para ver os logs da API em tempo real:

```bash
docker-compose logs -f api
```

---

## Padronização de Commits

Utilizamos **Husky** + **Commitlint** com a especificação `Conventional Commits`:

```bash
feat: adicionado módulo de metas financeiras
fix: corrigido cálculo de saldo mensal
docs: atualizado README com diagrama de arquitetura
refactor: extraído serviço de categorias do facade
```
