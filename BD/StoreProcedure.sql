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
    -- Parámetro de entrada: Cédula que se va a validar
    @pers_ci AS VARCHAR(50),
    
    -- Parámetro de salida: Indicador si la cédula es válida o no
    @resultado AS INT OUTPUT
)
AS
BEGIN
    -- Eliminar espacios en blanco antes y después de la cédula
    SET @pers_ci = RTRIM(LTRIM(@pers_ci))

    -- Variables para el proceso de validación
    DECLARE @total AS INT,              -- Acumulador para sumar los valores calculados
            @digito AS INT,             -- Último dígito de la cédula
            @num AS INT,                -- Longitud de la cédula
            @mult AS INT,               -- Valor temporal para almacenar los resultados de multiplicaciones
            @i AS INT,                  -- Contador para iterar los dígitos de la cédula
            @decena AS DECIMAL(10,2),   -- Variable que almacena la decena superior del total
            @final AS INT               -- Resultado de la validación del dígito verificador

    -- Validación inicial: la cédula debe ser numérica y tener exactamente 10 dígitos
    IF ISNUMERIC(@pers_ci) <> 1 OR LEN(@pers_ci) <> 10   
    BEGIN
        -- Si la cédula no es numérica o no tiene 10 dígitos, se marca como no válida
        SET @resultado = 0
    END
    ELSE
    BEGIN
        -- Inicialización de variables
        SET @num = LEN(@pers_ci)           -- Obtiene la longitud de la cédula
        SET @total = 0                     -- Reinicia el total de la suma
        SET @digito = CAST(SUBSTRING(@pers_ci, 10, 1) AS INT)  -- Extrae el último dígito (verificador)
        SET @i = 1                         -- Comienza la iteración desde el primer dígito

        -- Bucle para recorrer cada dígito de la cédula excepto el último
        WHILE @i < @num
        BEGIN
            SET @mult = 0  -- Resetea el valor de multiplicación temporal

            -- Si la posición es par
            IF @i % 2 <> 1
            BEGIN
                -- Suma el valor del dígito en posición par directamente
                SET @total = @total + CAST(SUBSTRING(@pers_ci, @i, 1) AS INT)
            END
            ELSE
            BEGIN
                -- Multiplica el dígito en posición impar por 2
                SET @mult = CAST(SUBSTRING(@pers_ci, @i, 1) AS INT) * 2

                -- Si el resultado de la multiplicación es mayor que 9, resta 9 (regla de validación)
                IF @mult > 9 
                    SET @total = @total + (@mult - 9)
                ELSE
                    SET @total = @total + @mult
            END

            -- Avanza al siguiente dígito
            SET @i = @i + 1
        END

        -- Cálculo del valor esperado para el dígito verificador
        SET @decena = @total / 10
        SET @decena = FLOOR(@decena)         -- Redondea hacia abajo
        SET @decena = (@decena + 1) * 10     -- Obtiene la siguiente decena más cercana
        SET @final = (@decena - @total)      -- Calcula la diferencia entre la decena y el total acumulado

        -- Verificación final: compara el dígito calculado con el último dígito de la cédula
        IF (@final = 10 AND @digito = 0) OR (@final = @digito)
        BEGIN
            SET @resultado = 1  -- La cédula es válida
        END
        ELSE
        BEGIN   
            SET @resultado = 0  -- La cédula no es válida
        END
    END
END





--=====================Validacion cedula========================
CREATE PROCEDURE ValidarCedula
(
    -- Parámetro de entrada: Cédula que se va a validar
    @pers_ci AS VARCHAR(50)
)
RETURNS INT
AS
BEGIN
    -- Eliminar espacios en blanco antes y después de la cédula
    SET @pers_ci = RTRIM(LTRIM(@pers_ci))

    -- Variables para el proceso de validación
    DECLARE @bandera AS INT,            -- Indicador de si la cédula es válida (1) o no (0)
            @total AS INT,              -- Acumulador para sumar los valores calculados
            @digito AS INT,             -- Último dígito de la cédula
            @num AS INT,                -- Longitud de la cédula
            @mult AS INT,               -- Valor temporal para almacenar los resultados de multiplicaciones
            @i AS INT,                  -- Contador para iterar los dígitos de la cédula
            @decena AS DECIMAL(10,2),   -- Variable que almacena la decena superior del total
            @final AS INT,              -- Resultado de la validación del dígito verificador
            @resto AS INT               -- No se utiliza en el código actual

    -- Validación inicial: la cédula debe ser numérica y tener exactamente 10 dígitos
    IF ISNUMERIC(@pers_ci) <> 1 OR LEN(@pers_ci) <> 10   
    BEGIN
        -- Si la cédula no es numérica o no tiene 10 dígitos, se marca como no válida
        SET @bandera = 0
    END
    ELSE
    BEGIN
        -- Inicialización de variables
        SET @bandera = 0                  -- Asume inicialmente que la cédula es inválida
        SET @num = LEN(@pers_ci)           -- Obtiene la longitud de la cédula
        SET @total = 0                     -- Reinicia el total de la suma
        SET @digito = CAST(SUBSTRING(@pers_ci, 10, 1) AS INT)  -- Extrae el último dígito (verificador)
        SET @i = 1                         -- Comienza la iteración desde el primer dígito

        -- Bucle para recorrer cada dígito de la cédula excepto el último
        WHILE @i < @num
        BEGIN
            SET @mult = 0  -- Resetea el valor de multiplicación temporal

            -- Si la posición es par
            IF @i % 2 <> 1
            BEGIN
                -- Suma el valor del dígito en posición par directamente
                SET @total = @total + CAST(SUBSTRING(@pers_ci, @i, 1) AS INT)
            END
            ELSE
            BEGIN
                -- Multiplica el dígito en posición impar por 2
                SET @mult = CAST(SUBSTRING(@pers_ci, @i, 1) AS INT) * 2

                -- Si el resultado de la multiplicación es mayor que 9, resta 9 (regla de validación)
                IF @mult > 9 
                    SET @total = @total + (@mult - 9)
                ELSE
                    SET @total = @total + @mult
            END

            -- Avanza al siguiente dígito
            SET @i = @i + 1
        END

        -- Cálculo del valor esperado para el dígito verificador
        SET @decena = @total / 10
        SET @decena = FLOOR(@decena)         -- Redondea hacia abajo
        SET @decena = (@decena + 1) * 10     -- Obtiene la siguiente decena más cercana
        SET @final = (@decena - @total)      -- Calcula la diferencia entre la decena y el total acumulado

        -- Verificación final: compara el dígito calculado con el último dígito de la cédula
        IF (@final = 10 AND @digito = 0) OR (@final = @digito)
        BEGIN
            SET @bandera = 1  -- La cédula es válida
        END
        ELSE
        BEGIN   
            SET @bandera = 0  -- La cédula no es válida
        END
    END

    -- Retorna el resultado de la validación (1 si es válida, 0 si no)
    RETURN @bandera
END


ValidarCedula'1754607255'

DECLARE @validacion INT

EXEC ValidarCedula @pers_ci = '1600909335', @resultado = @validacion OUTPUT

SELECT @validacion  -- Mostrará 1 si la cédula es válida, 0 si no
