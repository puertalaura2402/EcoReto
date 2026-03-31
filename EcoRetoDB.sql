CREATE DATABASE EcoRetoDB;
GO
USE EcoRetoDB;
GO

CREATE TABLE Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    Usuario NVARCHAR(50) NOT NULL,
    Contraseña NVARCHAR(50) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NULL
);
GO


CREATE TABLE Misiones (
    IdMision INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(300) NOT NULL,
    Puntos INT NOT NULL
);

CREATE TABLE UsuarioMisiones (
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(IdUsuario),
    IdMision INT FOREIGN KEY REFERENCES Misiones(IdMision),
    PRIMARY KEY (IdUsuario, IdMision)
);


ALTER TABLE Misiones
ADD IdCategoria INT;


CREATE TABLE Categorias (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    NombreCategoria NVARCHAR(50) NOT NULL
);

INSERT INTO Categorias (NombreCategoria)
VALUES 
('Reciclaje'),
('Ahorro de energía'),
('Ahorro de agua'),
('Transporte sostenible'),
('Estilo de vida verde');

-- Verificar
SELECT * FROM Categorias;

ALTER TABLE Misiones
ADD CONSTRAINT FK_Misiones_Categorias
FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria);


SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Categorias';

SELECT DB_NAME() AS BaseDeDatosActual;
SELECT * FROM sys.tables WHERE name = 'Categorias';




--PROCEDIMIENTOS

--INSERTAR USUARIO
CREATE PROCEDURE sp_InsertarUsuario
    @Usuario NVARCHAR(50),
    @Contraseña NVARCHAR(50),
    @Rol NVARCHAR(50),
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Usuarios (Usuario, Contraseña, Rol, Email)
    VALUES (@Usuario, @Contraseña, @Rol, @Email);
END;
GO

--LISTAR USUARIOS
CREATE PROCEDURE sp_ListarUsuarios
AS
BEGIN
    SELECT * FROM Usuarios;
END;
GO


--VALIDAR USUARIO
CREATE PROCEDURE sp_ValidarUsuario
    @Usuario NVARCHAR(50),
    @Contraseña NVARCHAR(50)
AS
BEGIN
    SELECT * FROM Usuarios
    WHERE Usuario = @Usuario AND Contraseña = @Contraseña;
END;
GO


--ACTUALIZAR USUARIO
CREATE PROCEDURE sp_ActualizarUsuario
    @IdUsuario INT,
    @Usuario NVARCHAR(50),
    @Contraseña NVARCHAR(50),
    @Rol NVARCHAR(50),
    @Email NVARCHAR(100)
AS
BEGIN
    UPDATE Usuarios
    SET Usuario = @Usuario,
        Contraseña = @Contraseña,
        Rol = @Rol,
        Email = @Email
    WHERE IdUsuario = @IdUsuario;
END;
GO



--ELIMINAR USUARIO
CREATE PROCEDURE sp_EliminarUsuario
    @IdUsuario INT
AS
BEGIN
    DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario;
END;
GO


--VERIFICAR USUARIO EXIXTENTE
CREATE PROCEDURE sp_VerificarUsuarioExistente
    @Usuario NVARCHAR(50)
AS
BEGIN
    SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario;
END;
GO


---PROCEDIMIENTOS MISIONES

--INSERTAR MISIÓN
CREATE PROCEDURE sp_InsertarMision
    @Titulo NVARCHAR(100),
    @Descripcion NVARCHAR(300),
    @Puntos INT
AS
BEGIN
    INSERT INTO Misiones (Titulo, Descripcion, Puntos)
    VALUES (@Titulo, @Descripcion, @Puntos);
END;
GO

---LISTAR MISIÓN
CREATE PROCEDURE sp_ListarMisiones
AS
BEGIN
    SELECT * FROM Misiones ORDER BY IdMision DESC;
END;
GO


--ACTUALIZAR MISIÓN
CREATE PROCEDURE sp_ActualizarMision
    @IdMision INT,
    @Titulo NVARCHAR(100),
    @Descripcion NVARCHAR(300),
    @Puntos INT
AS
BEGIN
    UPDATE Misiones
    SET Titulo = @Titulo,
        Descripcion = @Descripcion,
        Puntos = @Puntos
    WHERE IdMision = @IdMision;
END;
GO


--ELIMINAR MISIÓN
CREATE PROCEDURE sp_EliminarMision
    @IdMision INT
AS
BEGIN
    DELETE FROM Misiones WHERE IdMision = @IdMision;
END;
GO

--Procedimientos UsuarioMisiones

-- Registrar misión completada
CREATE PROCEDURE sp_CompletarMision
    @IdUsuario INT,
    @IdMision INT
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM UsuarioMisiones WHERE IdUsuario = @IdUsuario AND IdMision = @IdMision)
    BEGIN
        INSERT INTO UsuarioMisiones (IdUsuario, IdMision)
        VALUES (@IdUsuario, @IdMision);
    END
END;
GO

-- Listar misiones completadas por usuario
CREATE PROCEDURE sp_ListarMisionesCompletadas
    @IdUsuario INT
AS
BEGIN
    SELECT M.IdMision, M.Titulo, M.Descripcion, M.Puntos, C.NombreCategoria AS CategoriaNombre
    FROM Misiones M
    INNER JOIN UsuarioMisiones UM ON M.IdMision = UM.IdMision
    INNER JOIN Categorias C ON M.IdCategoria = C.IdCategoria
    WHERE UM.IdUsuario = @IdUsuario;
END;
GO

-- Calcular puntaje total
CREATE PROCEDURE sp_CalcularPuntajeUsuario
    @IdUsuario INT
AS
BEGIN
    SELECT ISNULL(SUM(M.Puntos), 0) AS PuntajeTotal
    FROM Misiones M
    INNER JOIN UsuarioMisiones UM ON M.IdMision = UM.IdMision
    WHERE UM.IdUsuario = @IdUsuario;
END;
GO

USE EcoRetoDB;
GO

CREATE PROCEDURE sp_ListarMisionesDisponibles
AS
BEGIN
    SELECT 
        M.IdMision,
        M.Titulo,
        M.Descripcion,
        M.Puntos,
        C.NombreCategoria AS CategoriaNombre
    FROM Misiones M
    INNER JOIN Categorias C ON M.IdCategoria = C.IdCategoria;
END;
GO


INSERT INTO Usuarios (Usuario, Contraseña, Rol, Email)
VALUES ('admin', 'admin123', 'Administrador', 'admin@ecoreto.com');
GO
SELECT * FROM Usuarios;
GO


-- TABLA DE INSIGNIAS (solo si no existe)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Insignias')
BEGIN
    CREATE TABLE Insignias (
        IdInsignia INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(300) NOT NULL,
        ImagenPath NVARCHAR(200) NOT NULL,
        IdCategoria INT NULL, -- NULL para insignias generales
        TipoRequisito NVARCHAR(50) NOT NULL, -- 'MisionesPorCategoria', 'MisionesTotales'
        CantidadRequisito INT NOT NULL, -- Cantidad requerida (3, 5, 10, 30, 100 misiones)
        FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
    );
    PRINT 'Tabla Insignias creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Insignias ya existe.';
END
GO

-- ============================================
-- TABLA DE USUARIO-INSIGNIAS (solo si no existe)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsuarioInsignias')
BEGIN
    CREATE TABLE UsuarioInsignias (
        IdUsuario INT,
        IdInsignia INT,
        PRIMARY KEY (IdUsuario, IdInsignia),
        FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario),
        FOREIGN KEY (IdInsignia) REFERENCES Insignias(IdInsignia)
    );
    PRINT 'Tabla UsuarioInsignias creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla UsuarioInsignias ya existe.';
END
GO

-- ============================================
-- INSERTAR INSIGNIAS (solo si no existen)
-- ============================================

-- Insignias Generales
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Héroe del Planeta')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Héroe del Planeta', 'Por completar 30 misiones totales.', '/Content/Images/Generales/HeroedelPlaneta-Photoroom.png', NULL, 'MisionesTotales', 30);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Leyenda Verde')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Leyenda Verde', 'Insignia por alcanzar 100 misiones.', '/Content/Images/Generales/LeyendaVerde-Photoroom.png', NULL, 'MisionesTotales', 100);
END

-- Insignias de Reciclaje (IdCategoria = 1)
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Mano Verde')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Mano Verde', 'Se obtiene al completar las primeras 3 misiones de reciclaje.', '/Content/Images/Reciclaje/ManoVerde-Photoroom.png', 1, 'MisionesPorCategoria', 3);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Maestro del Reúso')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Maestro del Reúso', 'Por completar 5 misiones relacionadas con reusar objetos.', '/Content/Images/Reciclaje/MaestrodelReuso-Photoroom.png', 1, 'MisionesPorCategoria', 5);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Guerrero del Reciclaje')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Guerrero del Reciclaje', 'Por completar 10 misiones de reciclaje.', '/Content/Images/Reciclaje/GerrerodelReciclaje-Photoroom.png', 1, 'MisionesPorCategoria', 10);
END

-- Insignias de Ahorro de Energía (IdCategoria = 2)
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Guardián de la Energía')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Guardián de la Energía', 'Se obtiene al completar 3 misiones.', '/Content/Images/AhorrodeEnergia/GuardianDeLaEnergia-Photoroom.png', 2, 'MisionesPorCategoria', 3);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Ahorrador de Energía')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Ahorrador de Energía', 'Por realizar 5 misiones para reducir el consumo energético.', '/Content/Images/AhorrodeEnergia/AhorrodeEnergia-Photoroom.png', 2, 'MisionesPorCategoria', 5);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Maestro del Consumo')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Maestro del Consumo', 'Otorgada por completar 10 misiones.', '/Content/Images/AhorrodeEnergia/MaestroDelConsumo-Photoroom.png', 2, 'MisionesPorCategoria', 10);
END

-- Insignias de Ahorro de Agua (IdCategoria = 3)
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Ahorrador Inicial')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Ahorrador Inicial', 'Se obtiene al completar las primeras 3 misiones.', '/Content/Images/AhorrodeAgua/AhorradorInicial-Photoroom.png', 3, 'MisionesPorCategoria', 3);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Guardián del Agua')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Guardián del Agua', 'Por completar 5 misiones de ahorro del agua.', '/Content/Images/AhorrodeAgua/GuardiandelAgua-Photoroom.png', 3, 'MisionesPorCategoria', 5);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Héroe del Agua')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Héroe del Agua', 'Por completar 10 misiones.', '/Content/Images/AhorrodeAgua/HeroeDelAgua-Photoroom.png', 3, 'MisionesPorCategoria', 10);
END

-- Insignias de Transporte Sostenible (IdCategoria = 4)
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Explorador Urbano')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Explorador Urbano', 'Por completar 3 misiones.', '/Content/Images/TransporteSostenible/ExploradorUrbano-Photoroom.png', 4, 'MisionesPorCategoria', 3);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Caminante Ecológico')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Caminante Ecológico', 'Por completar 5 misiones.', '/Content/Images/TransporteSostenible/CaminanteEcologico-Photoroom.png', 4, 'MisionesPorCategoria', 5);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Ciudadano Sostenible')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Ciudadano Sostenible', 'Por completar 10 misiones.', '/Content/Images/TransporteSostenible/CiudadanoSostenible-Photoroom.png', 4, 'MisionesPorCategoria', 10);
END

-- Insignias de Estilo de Vida Verde (IdCategoria = 5)
IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Vida Verde')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Vida Verde', 'Por completar 3 misiones.', '/Content/Images/EstilodeVidaVerde/VidaVerde-Photoroom.png', 5, 'MisionesPorCategoria', 3);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'EcoHábito')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('EcoHábito', 'Por completar 5 misiones.', '/Content/Images/EstilodeVidaVerde/EcoHabito-Photoroom.png', 5, 'MisionesPorCategoria', 5);
END

IF NOT EXISTS (SELECT 1 FROM Insignias WHERE Nombre = 'Guardián del Bienestar Verde')
BEGIN
    INSERT INTO Insignias (Nombre, Descripcion, ImagenPath, IdCategoria, TipoRequisito, CantidadRequisito)
    VALUES ('Guardián del Bienestar Verde', 'Se otorga al completar 10 misiones.', '/Content/Images/EstilodeVidaVerde/GuardianDelBienestarVerde-Photoroom.png', 5, 'MisionesPorCategoria', 10);
END

PRINT 'Insignias verificadas/insertadas correctamente.';
GO

-- ============================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================

-- Listar todas las insignias
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_ListarInsignias') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_ListarInsignias;
GO

CREATE PROCEDURE sp_ListarInsignias
AS
BEGIN
    SELECT 
        I.IdInsignia,
        I.Nombre,
        I.Descripcion,
        I.ImagenPath,
        I.IdCategoria,
        C.NombreCategoria,
        I.TipoRequisito,
        I.CantidadRequisito
    FROM Insignias I
    LEFT JOIN Categorias C ON I.IdCategoria = C.IdCategoria
    ORDER BY I.IdCategoria, I.CantidadRequisito;
END;
GO

-- Obtener insignias desbloqueadas por usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_ObtenerInsigniasDesbloqueadas') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_ObtenerInsigniasDesbloqueadas;
GO

CREATE PROCEDURE sp_ObtenerInsigniasDesbloqueadas
    @IdUsuario INT
AS
BEGIN
    SELECT 
        I.IdInsignia,
        I.Nombre,
        I.Descripcion,
        I.ImagenPath,
        I.IdCategoria,
        C.NombreCategoria,
        I.TipoRequisito,
        I.CantidadRequisito
    FROM UsuarioInsignias UI
    INNER JOIN Insignias I ON UI.IdInsignia = I.IdInsignia
    LEFT JOIN Categorias C ON I.IdCategoria = C.IdCategoria
    WHERE UI.IdUsuario = @IdUsuario
    ORDER BY I.IdCategoria, I.CantidadRequisito;
END;
GO

-- Desbloquear insignia para usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_DesbloquearInsignia') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_DesbloquearInsignia;
GO

CREATE PROCEDURE sp_DesbloquearInsignia
    @IdUsuario INT,
    @IdInsignia INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UsuarioInsignias WHERE IdUsuario = @IdUsuario AND IdInsignia = @IdInsignia)
    BEGIN
        INSERT INTO UsuarioInsignias (IdUsuario, IdInsignia)
        VALUES (@IdUsuario, @IdInsignia);
    END
END;
GO

-- Obtener progreso de misiones por categoría
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_ObtenerProgresoPorCategoria') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_ObtenerProgresoPorCategoria;
GO

CREATE PROCEDURE sp_ObtenerProgresoPorCategoria
    @IdUsuario INT,
    @IdCategoria INT
AS
BEGIN
    SELECT COUNT(*) AS MisionesCompletadas
    FROM UsuarioMisiones UM
    INNER JOIN Misiones M ON UM.IdMision = M.IdMision
    WHERE UM.IdUsuario = @IdUsuario 
    AND M.IdCategoria = @IdCategoria;
END;
GO

-- Obtener total de misiones completadas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_ObtenerTotalMisionesCompletadas') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_ObtenerTotalMisionesCompletadas;
GO

CREATE PROCEDURE sp_ObtenerTotalMisionesCompletadas
    @IdUsuario INT
AS
BEGIN
    SELECT COUNT(*) AS TotalMisiones
    FROM UsuarioMisiones
    WHERE IdUsuario = @IdUsuario;
END;
GO

PRINT '========================================';
PRINT 'Script ejecutado exitosamente!';
PRINT 'El sistema de insignias está listo para usar.';
PRINT '========================================';

-- ============================================
-- TABLA DE NOTIFICACIONES
-- ============================================
use EcoRetoDB;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notificaciones')
BEGIN
    CREATE TABLE Notificaciones (
        IdNotificacion INT IDENTITY(1,1) PRIMARY KEY,
        IdUsuario INT NOT NULL,
        Tipo NVARCHAR(50) NOT NULL, -- 'InsigniaDesbloqueada', 'NuevaMision'
        Titulo NVARCHAR(200) NOT NULL,
        Mensaje NVARCHAR(500) NOT NULL,
        IdReferencia INT NULL, -- Id de la insignia o misión relacionada
        Leida BIT DEFAULT 0,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
    );
    PRINT 'Tabla Notificaciones creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Notificaciones ya existe.';
END
GO

-- Índice para mejorar consultas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notificaciones_Usuario_Leida')
BEGIN
    CREATE INDEX IX_Notificaciones_Usuario_Leida ON Notificaciones(IdUsuario, Leida);
END
GO

PRINT '========================================';
PRINT 'Script de notificaciones ejecutado exitosamente!';
PRINT '========================================';
