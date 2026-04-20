-- Remover a tabela antiga se existir
IF EXISTS (SELECT * FROM sysobjects WHERE name='Pecas' AND xtype='U')
BEGIN
    DROP TABLE [dbo].[Pecas];
END

-- Criar tabela Pecas com Guid
CREATE TABLE [dbo].[Pecas] (
    [Id] uniqueidentifier NOT NULL,
    [Nome] nvarchar(100) NOT NULL,
    [Codigo] nvarchar(50) NOT NULL,
    [PrecoUnitario] decimal(18,2) NOT NULL,
    [Estoque] int NOT NULL,
    [Descricao] nvarchar(500) NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Pecas] PRIMARY KEY ([Id])
);

-- Criar índice único para o código
CREATE UNIQUE INDEX [IX_Pecas_Codigo] ON [dbo].[Pecas] ([Codigo]);

PRINT 'Tabela Pecas recriada com sucesso usando Guid!';
