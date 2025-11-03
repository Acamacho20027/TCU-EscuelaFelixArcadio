-- =============================================
-- Script: Asignar Rol de Estudiante a Usuario
-- Descripción: Asigna el rol de Estudiante a un usuario específico
-- Fecha: 2024
-- =============================================

-- IMPORTANTE: Reemplaza 'usuario@ejemplo.com' con el email del usuario que quieres convertir en estudiante

-- Primero, verificar si el usuario existe y ver su rol actual
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RolActual
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'andrewcr72.o@gmail.com';

-- Eliminar cualquier rol existente del usuario
DELETE FROM AspNetUserRoles 
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'andrewcr72.o@gmail.com');

-- Asignar el rol de Estudiante
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT 
    (SELECT Id FROM AspNetUsers WHERE Email = 'andrewcr72.o@gmail.com'),
    (SELECT Id FROM AspNetRoles WHERE Name = 'Estudiante');

-- Verificar el cambio
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RolNuevo
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'andrewcr72.o@gmail.com';

-- Resultado esperado: El usuario debe tener el rol "Estudiante"

