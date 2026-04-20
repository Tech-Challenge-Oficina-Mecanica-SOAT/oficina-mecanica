-- Criar tabela Pecas se não existir
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pecas' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Pecas] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Nome] nvarchar(100) NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [PrecoUnitario] decimal(18,2) NOT NULL,
        [Estoque] int NOT NULL,
        [Descricao] nvarchar(500) NULL,
        [CriadoEm] datetime2 NOT NULL,
        [AtualizadoEm] datetime2 NULL,
        CONSTRAINT [PK_Pecas] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Pecas_Codigo] ON [dbo].[Pecas] ([Codigo]);
    
    PRINT 'Tabela Pecas criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Pecas já existe. Nenhuma ação necessária.';
END
