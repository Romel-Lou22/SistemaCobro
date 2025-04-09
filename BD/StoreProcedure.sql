--validacion
CREATE FUNCTION ValidarCedula
(
    @ncedula AS VARCHAR(50)
)
RETURNS INT
AS
BEGIN
      SET @ncedula = RTRIM(LTRIM(@ncedula))

	  DECLARE @bandera AS INT,
	          @total   AS INT,
			  @digito  AS INT,
			  @num     AS INT,
			  @mult  AS INT,
			  @i  AS INT,
			  @decena AS DECIMAL(10,2),
			  @final AS INT,
			  @resto AS INT
     
	  IF ISNUMERIC(@ncedula) <> 1 OR LEN(@ncedula) !=10
	  BEGIN
	       SET @bandera = 0
	  END
	  ELSE
	  BEGIN
	       SET @bandera =0
		   SET @num = LEN(@ncedula)
		   SET @total = 0
		   SET @digito = CAST(SUBSTRING(@ncedula,@i,1) AS INT) --extrae ultio numero de cedula
		   SET @i =1
	  END

	  WHILE @i < @num
	  BEGIN
	      SET @mult = 0

		  IF @i % 2 <> 1
		  BEGIN 
		        SET @total = @total + CAST(SUBSTRING(@ncedula,@i,1) AS INT)
		  END
	  END


END


CREATE PROCEDURE ValidarCedula
(
    -- Par�metro de entrada: C�dula que se va a validar
    @pers_ci AS VARCHAR(50),
    
    -- Par�metro de salida: Indicador si la c�dula es v�lida o no
    @resultado AS INT OUTPUT
)
AS
BEGIN
    -- Eliminar espacios en blanco antes y despu�s de la c�dula
    SET @pers_ci = RTRIM(LTRIM(@pers_ci))

    -- Variables para el proceso de validaci�n
    DECLARE @total AS INT,              -- Acumulador para sumar los valores calculados
            @digito AS INT,             -- �ltimo d�gito de la c�dula
            @num AS INT,                -- Longitud de la c�dula
            @mult AS INT,               -- Valor temporal para almacenar los resultados de multiplicaciones
            @i AS INT,                  -- Contador para iterar los d�gitos de la c�dula
            @decena AS DECIMAL(10,2),   -- Variable que almacena la decena superior del total
            @final AS INT               -- Resultado de la validaci�n del d�gito verificador

    -- Validaci�n inicial: la c�dula debe ser num�rica y tener exactamente 10 d�gitos
    IF ISNUMERIC(@pers_ci) <> 1 OR LEN(@pers_ci) <> 10   
    BEGIN
        -- Si la c�dula no es num�rica o no tiene 10 d�gitos, se marca como no v�lida
        SET @resultado = 0
    END
    ELSE
    BEGIN
        -- Inicializaci�n de variables
        SET @num = LEN(@pers_ci)           -- Obtiene la longitud de la c�dula
        SET @total = 0                     -- Reinicia el total de la suma
        SET @digito = CAST(SUBSTRING(@pers_ci, 10, 1) AS INT)  -- Extrae el �ltimo d�gito (verificador)
        SET @i = 1                         -- Comienza la iteraci�n desde el primer d�gito

        -- Bucle para recorrer cada d�gito de la c�dula excepto el �ltimo
        WHILE @i < @num
        BEGIN
            SET @mult = 0  -- Resetea el valor de multiplicaci�n temporal

            -- Si la posici�n es par
            IF @i % 2 <> 1
            BEGIN
                -- Suma el valor del d�gito en posici�n par directamente
                SET @total = @total + CAST(SUBSTRING(@pers_ci, @i, 1) AS INT)
            END
            ELSE
            BEGIN
                -- Multiplica el d�gito en posici�n impar por 2
                SET @mult = CAST(SUBSTRING(@pers_ci, @i, 1) AS INT) * 2

                -- Si el resultado de la multiplicaci�n es mayor que 9, resta 9 (regla de validaci�n)
                IF @mult > 9 
                    SET @total = @total + (@mult - 9)
                ELSE
                    SET @total = @total + @mult
            END

            -- Avanza al siguiente d�gito
            SET @i = @i + 1
        END

        -- C�lculo del valor esperado para el d�gito verificador
        SET @decena = @total / 10
        SET @decena = FLOOR(@decena)         -- Redondea hacia abajo
        SET @decena = (@decena + 1) * 10     -- Obtiene la siguiente decena m�s cercana
        SET @final = (@decena - @total)      -- Calcula la diferencia entre la decena y el total acumulado

        -- Verificaci�n final: compara el d�gito calculado con el �ltimo d�gito de la c�dula
        IF (@final = 10 AND @digito = 0) OR (@final = @digito)
        BEGIN
            SET @resultado = 1  -- La c�dula es v�lida
        END
        ELSE
        BEGIN   
            SET @resultado = 0  -- La c�dula no es v�lida
        END
    END
END





--=====================Validacion cedula========================
CREATE PROCEDURE ValidarCedula
(
    -- Par�metro de entrada: C�dula que se va a validar
    @pers_ci AS VARCHAR(50)
)
RETURNS INT
AS
BEGIN
    -- Eliminar espacios en blanco antes y despu�s de la c�dula
    SET @pers_ci = RTRIM(LTRIM(@pers_ci))

    -- Variables para el proceso de validaci�n
    DECLARE @bandera AS INT,            -- Indicador de si la c�dula es v�lida (1) o no (0)
            @total AS INT,              -- Acumulador para sumar los valores calculados
            @digito AS INT,             -- �ltimo d�gito de la c�dula
            @num AS INT,                -- Longitud de la c�dula
            @mult AS INT,               -- Valor temporal para almacenar los resultados de multiplicaciones
            @i AS INT,                  -- Contador para iterar los d�gitos de la c�dula
            @decena AS DECIMAL(10,2),   -- Variable que almacena la decena superior del total
            @final AS INT,              -- Resultado de la validaci�n del d�gito verificador
            @resto AS INT               -- No se utiliza en el c�digo actual

    -- Validaci�n inicial: la c�dula debe ser num�rica y tener exactamente 10 d�gitos
    IF ISNUMERIC(@pers_ci) <> 1 OR LEN(@pers_ci) <> 10   
    BEGIN
        -- Si la c�dula no es num�rica o no tiene 10 d�gitos, se marca como no v�lida
        SET @bandera = 0
    END
    ELSE
    BEGIN
        -- Inicializaci�n de variables
        SET @bandera = 0                  -- Asume inicialmente que la c�dula es inv�lida
        SET @num = LEN(@pers_ci)           -- Obtiene la longitud de la c�dula
        SET @total = 0                     -- Reinicia el total de la suma
        SET @digito = CAST(SUBSTRING(@pers_ci, 10, 1) AS INT)  -- Extrae el �ltimo d�gito (verificador)
        SET @i = 1                         -- Comienza la iteraci�n desde el primer d�gito

        -- Bucle para recorrer cada d�gito de la c�dula excepto el �ltimo
        WHILE @i < @num
        BEGIN
            SET @mult = 0  -- Resetea el valor de multiplicaci�n temporal

            -- Si la posici�n es par
            IF @i % 2 <> 1
            BEGIN
                -- Suma el valor del d�gito en posici�n par directamente
                SET @total = @total + CAST(SUBSTRING(@pers_ci, @i, 1) AS INT)
            END
            ELSE
            BEGIN
                -- Multiplica el d�gito en posici�n impar por 2
                SET @mult = CAST(SUBSTRING(@pers_ci, @i, 1) AS INT) * 2

                -- Si el resultado de la multiplicaci�n es mayor que 9, resta 9 (regla de validaci�n)
                IF @mult > 9 
                    SET @total = @total + (@mult - 9)
                ELSE
                    SET @total = @total + @mult
            END

            -- Avanza al siguiente d�gito
            SET @i = @i + 1
        END

        -- C�lculo del valor esperado para el d�gito verificador
        SET @decena = @total / 10
        SET @decena = FLOOR(@decena)         -- Redondea hacia abajo
        SET @decena = (@decena + 1) * 10     -- Obtiene la siguiente decena m�s cercana
        SET @final = (@decena - @total)      -- Calcula la diferencia entre la decena y el total acumulado

        -- Verificaci�n final: compara el d�gito calculado con el �ltimo d�gito de la c�dula
        IF (@final = 10 AND @digito = 0) OR (@final = @digito)
        BEGIN
            SET @bandera = 1  -- La c�dula es v�lida
        END
        ELSE
        BEGIN   
            SET @bandera = 0  -- La c�dula no es v�lida
        END
    END

    -- Retorna el resultado de la validaci�n (1 si es v�lida, 0 si no)
    RETURN @bandera
END


ValidarCedula'1754607255'

DECLARE @validacion INT

EXEC ValidarCedula @pers_ci = '1600909335', @resultado = @validacion OUTPUT

SELECT @validacion  -- Mostrar� 1 si la c�dula es v�lida, 0 si no
