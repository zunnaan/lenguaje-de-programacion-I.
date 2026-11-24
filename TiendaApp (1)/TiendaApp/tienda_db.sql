-- ============================================================
-- Script de base de datos para TiendaApp (administracion + facturacion)
-- Ejecutar en SQL Server Management Studio (SSMS).
-- Es seguro re-ejecutarlo: solo crea o agrega lo que falta
-- (tambien actualiza una base creada con la version anterior).
-- ============================================================

IF DB_ID('tienda_db') IS NULL
BEGIN
    CREATE DATABASE tienda_db;
END
GO

USE tienda_db;
GO

-- ---------- persona (se usa como CLIENTES) ----------
IF OBJECT_ID('dbo.persona', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.persona (
        id       INT IDENTITY(1,1) PRIMARY KEY,
        nombre   VARCHAR(50)  NOT NULL,
        apellido VARCHAR(50)  NOT NULL,
        correo   VARCHAR(100) NOT NULL
    );

    INSERT INTO dbo.persona (nombre, apellido, correo) VALUES
        ('Juan',  'Perez',    'juan.perez@correo.com'),
        ('Maria', 'Gomez',    'maria.gomez@correo.com'),
        ('Pedro', 'Martinez', 'pedro.martinez@correo.com');
END
GO

-- ---------- usuario (administradores y cajeros) ----------
IF OBJECT_ID('dbo.usuario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        id         INT IDENTITY(1,1) PRIMARY KEY,
        usuario    VARCHAR(30)    NOT NULL UNIQUE,
        clave_hash VARBINARY(32)  NOT NULL,          -- SHA2-256 de la contrasena
        nombre     VARCHAR(100)   NOT NULL,
        rol        VARCHAR(15)    NOT NULL CHECK (rol IN ('Administrador', 'Cajero')),
        turno      VARCHAR(10)    NULL,              -- Mañana / Tarde / Noche (obligatorio para cajeros)
        activo     BIT            NOT NULL DEFAULT 1,
        CONSTRAINT CK_usuario_turno CHECK (rol <> 'Cajero' OR turno IN ('Mañana', 'Tarde', 'Noche'))
    );

    INSERT INTO dbo.usuario (usuario, clave_hash, nombre, rol, turno) VALUES
        ('admin',         HASHBYTES('SHA2_256', 'admin123'), 'Administrador',  'Administrador', NULL),
        ('cajero.manana', HASHBYTES('SHA2_256', 'caja123'),  'Cajero Mañana',  'Cajero',        'Mañana'),
        ('cajero.tarde',  HASHBYTES('SHA2_256', 'caja123'),  'Cajero Tarde',   'Cajero',        'Tarde'),
        ('cajero.noche',  HASHBYTES('SHA2_256', 'caja123'),  'Cajero Noche',   'Cajero',        'Noche');
END
GO

-- ---------- cajero (vista): los usuarios con rol Cajero y su turno ----------
IF OBJECT_ID('dbo.cajero', 'V') IS NOT NULL DROP VIEW dbo.cajero;
GO
CREATE VIEW dbo.cajero AS
    SELECT id, usuario, nombre, turno, activo
    FROM dbo.usuario
    WHERE rol = 'Cajero';
GO

-- ---------- producto ----------
IF OBJECT_ID('dbo.producto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.producto (
        id        INT IDENTITY(1,1) PRIMARY KEY,      -- ID / codigo del producto
        nombre    VARCHAR(100)  NOT NULL,
        categoria VARCHAR(50)   NOT NULL DEFAULT 'General',
        precio    DECIMAL(12,2) NOT NULL CHECK (precio >= 0),
        itbis_pct DECIMAL(5,2)  NOT NULL DEFAULT 18,   -- 18 = ITBIS; 0 = exento
        stock     INT           NOT NULL DEFAULT 0 CHECK (stock >= 0),
        activo    BIT           NOT NULL DEFAULT 1
    );
END
GO

-- Bases creadas con la version anterior: agrega la columna categoria
IF COL_LENGTH('dbo.producto', 'categoria') IS NULL
BEGIN
    ALTER TABLE dbo.producto ADD categoria VARCHAR(50) NOT NULL CONSTRAINT DF_producto_categoria DEFAULT 'General';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.producto)
BEGIN
    INSERT INTO dbo.producto (nombre, categoria, precio, itbis_pct, stock) VALUES
        ('Arroz 5 lb',          'Víveres',   185.00, 0,  100),
        ('Habichuelas rojas',   'Víveres',    65.00, 0,   80),
        ('Aceite vegetal 1 L',  'Víveres',   210.00, 18,  60),
        ('Refresco 2 L',        'Bebidas',    95.00, 18, 120),
        ('Jabon de baño',       'Higiene',    45.00, 18, 150),
        ('Cuaderno 100 hojas',  'Papelería',  75.00, 18,  90);
END
GO

-- ---------- secuencia (numeracion de facturas sin saltos) ----------
IF OBJECT_ID('dbo.secuencia', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.secuencia (
        nombre VARCHAR(30) PRIMARY KEY,
        ultimo INT NOT NULL
    );

    INSERT INTO dbo.secuencia (nombre, ultimo) VALUES ('factura', 0);   -- la primera factura sera 00001
END
GO

-- ---------- factura ----------
IF OBJECT_ID('dbo.factura', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.factura (
        id           INT IDENTITY(1,1) PRIMARY KEY,
        numero       INT           NOT NULL UNIQUE,   -- se muestra con 5 digitos: 00001
        fecha        DATETIME      NOT NULL,          -- fecha y hora
        tipo         VARCHAR(7)    NOT NULL CHECK (tipo IN ('Contado', 'Credito')),
        usuario_id   INT           NOT NULL REFERENCES dbo.usuario(id),   -- cajero responsable
        turno        VARCHAR(10)   NULL,
        persona_id   INT           NULL REFERENCES dbo.persona(id),       -- cliente (obligatorio en credito)
        total_bruto  DECIMAL(12,2) NOT NULL,
        total_itbis  DECIMAL(12,2) NOT NULL,
        total_pagar  DECIMAL(12,2) NOT NULL,
        monto_pagado DECIMAL(12,2) NOT NULL DEFAULT 0,
        devuelta     DECIMAL(12,2) NOT NULL DEFAULT 0
    );
END
GO

-- ---------- factura_detalle ----------
IF OBJECT_ID('dbo.factura_detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.factura_detalle (
        id              INT IDENTITY(1,1) PRIMARY KEY,
        factura_id      INT           NOT NULL REFERENCES dbo.factura(id),
        producto_id     INT           NOT NULL REFERENCES dbo.producto(id),
        nombre_producto VARCHAR(100)  NOT NULL,
        cantidad        INT           NOT NULL CHECK (cantidad > 0),
        valor           DECIMAL(12,2) NOT NULL,   -- precio unitario
        itbis           DECIMAL(12,2) NOT NULL,   -- ITBIS de la linea
        total           DECIMAL(12,2) NOT NULL    -- cantidad * valor + itbis
    );
END
GO

-- ---------- cuenta_por_cobrar ----------
IF OBJECT_ID('dbo.cuenta_por_cobrar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.cuenta_por_cobrar (
        id         INT IDENTITY(1,1) PRIMARY KEY,
        factura_id INT           NOT NULL UNIQUE REFERENCES dbo.factura(id),
        persona_id INT           NOT NULL REFERENCES dbo.persona(id),
        fecha      DATETIME      NOT NULL,
        monto      DECIMAL(12,2) NOT NULL,
        saldo      DECIMAL(12,2) NOT NULL,
        estado     VARCHAR(10)   NOT NULL DEFAULT 'Pendiente'
    );
END
GO
