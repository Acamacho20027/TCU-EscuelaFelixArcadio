USE EscuelaFelixArcadio
GO

-- Habilitar el bloqueo de cuenta (LockoutEnabled = 1) para todos los usuarios
UPDATE AspNetUsers
SET LockoutEnabled = 1
WHERE LockoutEnabled = 0 OR LockoutEnabled IS NULL;

-- Reiniciar el contador de intentos fallidos para todos los usuarios
UPDATE AspNetUsers
SET AccessFailedCount = 0;

-- Limpiar fechas de bloqueo antiguas
UPDATE AspNetUsers
SET LockoutEndDateUtc = NULL
WHERE LockoutEndDateUtc IS NOT NULL AND LockoutEndDateUtc < GETUTCDATE();

-- Verificar los cambios
SELECT 
    Id,
    UserName,
    Email,
    LockoutEnabled,
    AccessFailedCount,
    LockoutEndDateUtc
FROM AspNetUsers;

GO