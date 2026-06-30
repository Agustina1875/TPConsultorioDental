
--CONSULTORIO DENTAL(sino funciona el .bak creamos la tabla de 0)
USE master;
GO

-- Crea la base de datos (si no existe)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ConsultorioDental')
BEGIN
    CREATE DATABASE ConsultorioDental;
    PRINT 'Base de datos ConsultorioDental creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos ConsultorioDental ya existe. Continuando...';
END
GO

USE ConsultorioDental;
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id]               NVARCHAR(450)  NOT NULL,
        [Name]             NVARCHAR(256)  NULL,
        [NormalizedName]   NVARCHAR(256)  NULL,
        [ConcurrencyStamp] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla AspNetRoles creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id]                   NVARCHAR(450)    NOT NULL,
        [NombreCompleto]       NVARCHAR(MAX)    NULL,
        [UserName]             NVARCHAR(256)    NULL,
        [NormalizedUserName]   NVARCHAR(256)    NULL,
        [Email]                NVARCHAR(256)    NULL,
        [NormalizedEmail]      NVARCHAR(256)    NULL,
        [EmailConfirmed]       BIT              NOT NULL DEFAULT 0,
        [PasswordHash]         NVARCHAR(MAX)    NULL,
        [SecurityStamp]        NVARCHAR(MAX)    NULL,
        [ConcurrencyStamp]     NVARCHAR(MAX)    NULL,
        [PhoneNumber]          NVARCHAR(MAX)    NULL,
        [PhoneNumberConfirmed] BIT              NOT NULL DEFAULT 0,
        [TwoFactorEnabled]     BIT              NOT NULL DEFAULT 0,
        [LockoutEnd]           DATETIMEOFFSET   NULL,
        [LockoutEnabled]       BIT              NOT NULL DEFAULT 1,
        [AccessFailedCount]    INT              NOT NULL DEFAULT 0,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla AspNetUsers creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserRoles creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id]         INT            NOT NULL IDENTITY(1,1),
        [UserId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserClaims creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(128)  NOT NULL,
        [ProviderKey]         NVARCHAR(128)  NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX)  NULL,
        [UserId]              NVARCHAR(450)  NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserLogins creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId]        NVARCHAR(450)  NOT NULL,
        [LoginProvider] NVARCHAR(128)  NOT NULL,
        [Name]          NVARCHAR(128)  NOT NULL,
        [Value]         NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserTokens creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id]         INT            NOT NULL IDENTITY(1,1),
        [RoleId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetRoleClaims creada.';
END
GO

-- Indices de Identity
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID('AspNetRoles'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers]([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE INDEX [EmailIndex] ON [AspNetUsers]([NormalizedEmail]);
GO


--TABLAS DEL SISTEMA 


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacientes')
BEGIN
    CREATE TABLE [Pacientes] (
        [Id]              INT             NOT NULL IDENTITY(1,1),
        [Nombre]          NVARCHAR(50)    NOT NULL,
        [Apellido]        NVARCHAR(50)    NOT NULL,
        [DNI]             NVARCHAR(MAX)   NOT NULL,
        [Telefono]        NVARCHAR(MAX)   NOT NULL,
        [Email]           NVARCHAR(MAX)   NOT NULL,
        [FechaNacimiento] DATETIME2       NOT NULL,
        CONSTRAINT [PK_Pacientes] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Pacientes creada.';
END
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Odontologos')
BEGIN
    CREATE TABLE [Odontologos] (
        [Id]          INT           NOT NULL IDENTITY(1,1),
        [Nombre]      NVARCHAR(50)  NOT NULL,
        [Apellido]    NVARCHAR(50)  NOT NULL,
        [Especialidad] NVARCHAR(80) NOT NULL,
        [Telefono]    NVARCHAR(MAX) NOT NULL,
        [Email]       NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_Odontologos] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Odontologos creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Turnos')
BEGIN
    CREATE TABLE [Turnos] (
        [Id]          INT             NOT NULL IDENTITY(1,1),
        [FechaHora]   DATETIME2       NOT NULL,
        [PacienteId]  INT             NOT NULL,
        [OdontologoId] INT            NOT NULL,
        [Motivo]      NVARCHAR(200)   NULL,
        [Estado]      NVARCHAR(30)    NOT NULL DEFAULT 'Pendiente',
        [Asistio]     BIT             NULL,
        CONSTRAINT [PK_Turnos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Turnos_Pacientes]   FOREIGN KEY ([PacienteId])   REFERENCES [Pacientes]([Id]),
        CONSTRAINT [FK_Turnos_Odontologos] FOREIGN KEY ([OdontologoId]) REFERENCES [Odontologos]([Id])
    );
    PRINT 'Tabla Turnos creada.';
END
GO


IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE [Name] = 'Administrador')
    INSERT INTO AspNetRoles ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', NEWID());

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE [Name] = 'Odontologo')
    INSERT INTO AspNetRoles ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(), 'Odontologo', 'ODONTOLOGO', NEWID());

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE [Name] = 'Paciente')
    INSERT INTO AspNetRoles ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(), 'Paciente', 'PACIENTE', NEWID());

PRINT 'Roles insertados correctamente.';
GO

