-- Script para resetear los intentos de bloqueo de todos los usuarios
-- Útil para probar el sistema de bloqueo desde cero
-- Ejecutar este script en la base de datos EscuelaFelixArcadio

USE EscuelaFelixArcadio
GO

-- Resetear contador de intentos fallidos
UPDATE AspNetUsers
SET AccessFailedCount = 0;

-- Desbloquear todas las cuentas
UPDATE AspNetUsers
SET LockoutEndDateUtc = NULL;

-- Asegurar que todos los usuarios tienen habilitado el bloqueo
UPDATE AspNetUsers
SET LockoutEnabled = 1
WHERE LockoutEnabled = 0 OR LockoutEnabled IS NULL;

-- Verificar los cambios
SELECT 
    Id,
    UserName,
    Email,
    LockoutEnabled,
    AccessFailedCount,
    LockoutEndDateUtc
FROM AspNetUsers;

PRINT 'Todos los intentos de bloqueo han sido reseteados.'
PRINT 'LockoutEnabled ha sido activado para todos los usuarios.'

GO