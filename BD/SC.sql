--SISTEMA COBROS AGUA
create database SistemaCobro
USE  SistemaCobro
CREATE TABLE Usuarios (
    CodigoUsuario BIGINT PRIMARY KEY,
    UsuarioSistema NVARCHAR(100),
    Cedula VARCHAR(15),
    LugarSistema NVARCHAR(50)
);

--IMPORTAR DATOS CSV

BULK INSERT Usuarios
FROM 'D:\usuarios.csv'
WITH (
    FIELDTERMINATOR = ';',  -- Especifica el punto y coma como separador de campos
    ROWTERMINATOR = '\n',   -- Especifica el salto de línea como delimitador de filas
    FIRSTROW = 2,           -- Omitir la primera fila (encabezado)
    CODEPAGE = 'ACP'        -- Especifica la codificación de caracteres
);
---CONSULTAS
select *from Usuarios 
SELECT * FROM Usuarios WHERE CodigoUsuario = 576
SELECT * FROM Usuarios
ORDER BY UsuarioSistema;

--CREACIÓN TABLA REGISTRAR PAGOS REALIZADOS
CREATE TABLE Pagos (
    IdPago BIGINT PRIMARY KEY IDENTITY(1,1),
    CodigoUsuario BIGINT,
    Cedula VARCHAR(15),
    UsuarioSistema NVARCHAR(100),
    FechaPago DATE,
    MontoPago DECIMAL(10, 2),  -- Almacena el monto del pago
    TipoPago NVARCHAR(10),  -- 'Mensual' o 'Anual'
    CONSTRAINT FK_Usuario FOREIGN KEY (CodigoUsuario) REFERENCES Usuarios(CodigoUsuario)
);

select *from Pagos where CodigoUsuario = 12 and FechaPago between '1-10-2024' and '31-12-2024'

SELECT *
FROM Pagos
WHERE CodigoUsuario = 12
  AND FechaPago BETWEEN '2024-10-01' AND '2024-11-01';

select

CREATE TABLE InicioSesion
(
id_usuario INT PRIMARY KEY IDENTITY(1,1),
usuario varchar(50),
contrasena varchar(50)

);

select *from InicioSesion

INSERT INTO InicioSesion VALUES('ROMEL','1234');




select *from Pagos where CodigoUsuario = 24 and MONTH(FechaPago)= 10 and year(FechaPago)= 2024

select *from Donadores

ALTER TABLE Donadores ALTER COLUMN fecha DATETIME2;

DELETE FROM Donadores