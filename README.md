# MPFinance 🚀

MPFinance é um sistema de controle financeiro completo, com separação bem definida de responsabilidades através dos princípios de arquitetura limpa (Clean Architecture). 

## 🏗 Estrutura do Projeto (Clean Architecture)
A arquitetura do backend em .NET está segregada nas seguintes camadas:
- **Domain (`MPFinance.Domain`)**: Coração da aplicação. Contém as entidades, interfaces (`IBaseRepository`, `IGoalRepository`, etc.) e lógicas puras do negócio (ex: o método `Deposit` dentro da entidade `Goal`). 
- **Application (`MPFinance.Application`)**: Regras e casos de uso da aplicação. É a camada onde residem as DTOs e Facades (`FinancialFacade`).
- **Infrastructure (`MPFinance.Infrastructure`)**: Detalhes de implementação técnica e acesso ao banco de dados utilizando Entity Framework Core (`MPFinanceDbContext`) com repositórios concretos.
- **API (`MPFinance.API`)**: A camada de apresentação (Controllers) e ponto de entrada da aplicação, onde todas as injeções de dependência acontecem.

## 🎨 Design Patterns Utilizados
As seguintes abordagens de Padrões de Projeto (Design Patterns) são aplicadas no backend:

### 1. Repository Pattern
Utilizado para isolar a camada de acesso a dados. Existe uma interface base `IBaseRepository<T>` que fornece o CRUD padrão e evita código duplicado, além de repositórios especializados como `TransactionRepository`, `GoalRepository`, `CategoryRepository` e `UserRepository` para consultas específicas.

### 2. Facade Pattern (Estrutural)
A classe `FinancialFacade` centraliza fluxos e interações complexas entre múltiplos repositórios. Como por exemplo: criar uma transação financeira (`Transaction`) e automaticamente atualizar o progresso financeiro de uma meta associada (`Goal`), escondendo esta grande orquestração dos Controllers da API.

### 3. Factory Pattern (Criacional)
A estrutura `TransactionFactory` remove das outras camadas o peso de entender e processar as regras de como montar corretamente uma transação complexa no sistema, emitindo objetos padronizados sempre consistentes e validados.

### 4. Observer / Mediator Pattern (Comportamental)
Fazemos o uso do framework **MediatR** para publicar e processar assincronamente os eventos de domínio da aplicação. Por exemplo, registrar que um usuário foi criado (`UserRegisteredEvent`) dispara sem acoplar a classe de cadastro original.

## 🚀 Como rodar o sistema localmente
Contamos com o arquivo `docker-compose.yml` na raiz que provisiona recursos como o banco MySQL.
Para rodar em sua máquina, utilize o Bash/PowerShell:
```bash
docker-compose up -d
```
O banco será migrado e construído automaticamente pelas rotinas do `.NET` (`context.Database.Migrate();`).

## 🔐 Padronização de Commits
Utilizamos o **Husky** juntamente com o **Commitlint**. O nosso padrão adotado é a especificação do `Conventional Commits`.
Sempre escreva o log associando com a especificação certa. Exemplos: 
- `feat: adicionado módulo de usuários`
- `fix: corrigido cálculo da taxa interna`
- `docs: atualizado README`
