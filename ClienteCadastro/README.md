# Sistema de Cadastro de Clientes

## Visão Geral

Este projeto implementa um sistema CRUD para cadastro de clientes, seguindo os princípios de Domain-Driven Design (DDD), Command Query Responsibility Segregation (CQRS) e Event Sourcing. O sistema foi desenvolvido utilizando .NET 8 no backend e Angular no frontend, com SQL Server como banco de dados.

## Estrutura do Projeto

A solução foi estruturada seguindo os princípios de DDD, com as seguintes camadas:

### 1. ClienteCadastro.API

**Propósito**: Camada de apresentação responsável por expor os endpoints da API REST.

**Justificativa**: Separar a interface de usuário (API) das regras de negócio permite maior flexibilidade e facilita a manutenção do código. Esta camada traduz as requisições HTTP em comandos e consultas para a camada de aplicação.

### 2. ClienteCadastro.Domain

**Propósito**: Contém as entidades de domínio, regras de negócio, interfaces de repositórios e eventos de domínio.

**Justificativa**: O domínio é o coração da aplicação e contém as regras de negócio essenciais. Mantê-lo isolado garante que as regras de negócio não sejam contaminadas por preocupações de infraestrutura ou apresentação.

**Principais entidades**:
- Cliente (com discriminação entre Pessoa Física e Jurídica)
- Endereço
- Eventos de domínio

### 3. ClienteCadastro.Application

**Propósito**: Implementa os casos de uso da aplicação, utilizando o padrão CQRS para separar operações de leitura e escrita.

**Justificativa**: O CQRS permite otimizar separadamente as operações de leitura e escrita, além de facilitar a implementação de Event Sourcing. A camada de aplicação orquestra as interações entre a API e o domínio.

**Componentes principais**:
- Commands (CreateClienteCommand, UpdateClienteCommand, etc.)
- Queries (GetClienteQuery, ListClientesQuery, etc.)
- Command Handlers e Query Handlers
- Validadores (FluentValidation)

### 4. ClienteCadastro.Infrastructure

**Propósito**: Fornece implementações concretas para persistência de dados, serviços externos e Event Sourcing.

**Justificativa**: Isolar a infraestrutura permite trocar implementações sem afetar o domínio ou a aplicação. Por exemplo, podemos mudar o banco de dados sem alterar a lógica de negócio.

**Subcomponentes**:
- **Data**: Implementações de repositórios e contexto do Entity Framework
- **EventSourcing**: Implementação do Event Store para armazenar eventos de domínio
- **Services**: Serviços de infraestrutura como envio de e-mails, integração com APIs externas, etc.

### 5. ClienteCadastro.Tests

**Propósito**: Contém testes unitários para validar o comportamento do sistema.

**Justificativa**: Testes automatizados garantem que as regras de negócio sejam respeitadas e facilitam a refatoração do código com confiança.

## Regras de Negócio

O sistema implementa as seguintes regras de negócio:

1. **Unicidade de documentos e e-mail**:
   - Apenas um cadastro por CPF/CNPJ
   - Apenas um cadastro por e-mail

2. **Validações específicas por tipo de pessoa**:
   - **Pessoa Física**: 
     - Idade mínima de 18 anos
     - Validação de CPF

   - **Pessoa Jurídica**:
     - Obrigatoriedade de Inscrição Estadual (IE) ou declaração de isenção
     - Validação de CNPJ

3. **Validações de endereço**:
   - CEP válido
   - Campos obrigatórios preenchidos (logradouro, número, bairro, cidade, estado)

## Padrões Arquiteturais

### Domain-Driven Design (DDD)

**Por que usar DDD?**
- Foco no domínio e nas regras de negócio
- Linguagem ubíqua compartilhada entre desenvolvedores e especialistas do domínio
- Separação clara de responsabilidades em camadas
- Facilita a manutenção e evolução do sistema

### Command Query Responsibility Segregation (CQRS)

**Por que usar CQRS?**
- Separação de modelos de leitura e escrita
- Otimização independente para operações de leitura e escrita
- Facilita a escalabilidade
- Complementa bem o Event Sourcing

### Event Sourcing

**Por que usar Event Sourcing?**
- Mantém um histórico completo de todas as mudanças no sistema
- Permite reconstruir o estado do sistema em qualquer ponto no tempo
- Facilita auditoria e rastreabilidade
- Possibilita implementar funcionalidades como "desfazer" e "refazer"

### FluentValidation

**Por que usar FluentValidation?**
- Separação clara entre entidades e suas validações
- Sintaxe expressiva e legível
- Reutilização de validações
- Facilidade para criar mensagens de erro personalizadas

## Estrutura do Banco de Dados

O banco de dados SQL Server foi estruturado com as seguintes tabelas:

### Tabela: Clientes
```
Clientes
├── Id (UNIQUEIDENTIFIER) - Chave primária
├── TipoPessoa (CHAR(1)) - 'F' para física, 'J' para jurídica
├── Nome (NVARCHAR(100)) - Nome ou Razão Social
├── Documento (VARCHAR(18)) - CPF ou CNPJ
├── IE (VARCHAR(20)) - Inscrição Estadual (para PJ)
├── IsIsentoIE (BIT) - Indica se é isento de IE
├── DataNascimento (DATE) - Data de Nascimento ou Fundação
├── Telefone (VARCHAR(20))
├── Email (VARCHAR(100))
├── DataCriacao (DATETIME)
├── DataAtualizacao (DATETIME)
└── Ativo (BIT)
```

### Tabela: Enderecos
```
Enderecos
├── Id (UNIQUEIDENTIFIER) - Chave primária
├── ClienteId (UNIQUEIDENTIFIER) - Chave estrangeira para Clientes
├── CEP (VARCHAR(9))
├── Logradouro (NVARCHAR(100))
├── Numero (VARCHAR(20))
├── Complemento (NVARCHAR(50))
├── Bairro (NVARCHAR(50))
├── Cidade (NVARCHAR(50))
├── Estado (CHAR(2))
├── DataCriacao (DATETIME)
└── DataAtualizacao (DATETIME)
```

### Tabela: EventStore
```
EventStore
├── Id (UNIQUEIDENTIFIER) - Chave primária
├── AggregateId (UNIQUEIDENTIFIER) - ID da entidade relacionada
├── AggregateType (NVARCHAR(100)) - Tipo da entidade
├── EventType (NVARCHAR(100)) - Tipo do evento
├── Data (NVARCHAR(MAX)) - Dados do evento em JSON
├── Timestamp (DATETIME)
└── Version (INT) - Versão do agregado
```

## Configuração do Docker

O projeto utiliza Docker para facilitar a configuração do ambiente de desenvolvimento e garantir consistência entre diferentes ambientes.

### Estrutura do Docker

O Docker está configurado na pasta `Docker` na raiz da solução, contendo:
- `docker-compose.yml`: Configuração do container do SQL Server
- `init-database.sql`: Script de inicialização do banco de dados

### Comandos para Utilização do Banco de Dados

Para iniciar o ambiente de banco de dados:

```bash
# Navegar até a pasta Docker
cd ClienteCadastro/Docker

# Iniciar o container do SQL Server
docker-compose up -d

# Verificar se o container está em execução
docker ps

# Copiar o script de inicialização para o container (se necessário)
docker cp init-database.sql cliente-sqlserver:/var/opt/mssql/

# Executar o script de inicialização
docker exec -i cliente-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Cliente@123456" -C -i /var/opt/mssql/init-database.sql

# Verificar se as tabelas foram criadas
docker exec -i cliente-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Cliente@123456" -C -Q "USE ClienteDB; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
```

### Conexão ao Banco de Dados

- **Servidor**: localhost,1433
- **Usuário**: sa
- **Senha**: Cliente@123456
- **Banco de Dados**: ClienteDB

## Próximos Passos

1. Implementar as entidades de domínio e regras de negócio
2. Configurar o Entity Framework Core para acesso a dados
3. Implementar os comandos e consultas CQRS
4. Desenvolver a API REST
5. Implementar o frontend em Angular
6. Configurar testes unitários
7. Integrar o Docker Compose para a aplicação completa
