-- Criar o banco de dados
CREATE DATABASE ClienteDB;
GO

USE ClienteDB;
GO

-- Tabela de Clientes
CREATE TABLE Clientes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TipoPessoa CHAR(1) NOT NULL CHECK (TipoPessoa IN ('F', 'J')),
    Nome NVARCHAR(100) NOT NULL,
    Documento VARCHAR(18) NOT NULL UNIQUE,
    IE VARCHAR(20) NULL,
    IsIsentoIE BIT NULL,
    DataNascimento DATE NOT NULL,
    Telefone VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
    DataAtualizacao DATETIME NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_Cliente_IE CHECK (
        (TipoPessoa = 'F') OR 
        (TipoPessoa = 'J' AND (IE IS NOT NULL OR IsIsentoIE = 1))
    ),
    CONSTRAINT CK_Cliente_DataNascimento CHECK (
        (TipoPessoa = 'J') OR 
        (TipoPessoa = 'F' AND DATEDIFF(YEAR, DataNascimento, GETDATE()) >= 18)
    )
);
GO

-- Tabela de Endereços
CREATE TABLE Enderecos (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClienteId UNIQUEIDENTIFIER NOT NULL,
    CEP VARCHAR(9) NOT NULL,
    Logradouro NVARCHAR(100) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Complemento NVARCHAR(50) NULL,
    Bairro NVARCHAR(50) NOT NULL,
    Cidade NVARCHAR(50) NOT NULL,
    Estado CHAR(2) NOT NULL,
    DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
    DataAtualizacao DATETIME NULL,
    CONSTRAINT FK_Enderecos_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);
GO

-- Tabela para Event Sourcing
CREATE TABLE EventStore (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    AggregateType NVARCHAR(100) NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    Data NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Version INT NOT NULL
);
GO

-- Índices
CREATE INDEX IX_Clientes_Documento ON Clientes(Documento);
CREATE INDEX IX_Clientes_Email ON Clientes(Email);
CREATE INDEX IX_Enderecos_ClienteId ON Enderecos(ClienteId);
CREATE INDEX IX_EventStore_AggregateId ON EventStore(AggregateId);
GO

-- Criar um usuário para a aplicação acessar o banco
CREATE LOGIN ClienteApp WITH PASSWORD = 'Cliente@123456';
GO

CREATE USER ClienteApp FOR LOGIN ClienteApp;
GO

ALTER ROLE db_owner ADD MEMBER ClienteApp;
GO
