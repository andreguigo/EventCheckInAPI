# Sistema de Check-in

Este projeto implementa um sistema de **Check-in**, com o objetivo de registrar e gerenciar a entrada de usuários em eventos, estabelecimentos ou outros locais.

## Construção

### Pré requisitos
* .Net SDK 8
* Dapper

###### Outros pacotes na aplicação
* MySqlConnector
* Swashbuckle.AspNetCore

### Banco de dados

Crie uma base de dados chamada `eventoscheckin`
~~~sql
-- Base de dados
create database eventoscheckin;

-- Tabela de Eventos
CREATE TABLE Eventos (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    Codigo VARCHAR(25) NOT NULL,
    Nome VARCHAR(255) NOT NULL,
    DataHoraInicio DATETIME NOT NULL,
    DataHoraFim DATETIME NOT NULL,
    PeriodoContinuo TEXT,
    Local VARCHAR(255) NOT NULL,
    Pago BOOLEAN NOT NULL,
    LimiteInscricoes INT NOT NULL
);

-- Tabela de Usuarios
CREATE TABLE Usuarios (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    NomeCompleto VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Telefone VARCHAR(20)
);

-- Tabela de Inscricoes
CREATE TABLE Inscricoes (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    QrCode TEXT,
    Pin VARCHAR(5),
    EventoId CHAR(36) NOT NULL,
    UsuarioId CHAR(36) NOT NULL,
    FOREIGN KEY (EventoId) REFERENCES Eventos(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

-- Índices para otimizar consultas
CREATE INDEX IDX_Evento_Codigo ON Eventos (Codigo);
CREATE INDEX IDX_Usuario_Email ON Usuarios (Email);
~~~
