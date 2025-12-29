-- Crear script SQL para init del contenedor (solo BD, sin usuarios)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuickMeet')
BEGIN
    CREATE DATABASE [QuickMeet]
END
