-- =============================================
-- Script: Verificar Roles de Usuarios
-- Descripción: Consulta para verificar qué roles tienen los usuarios
-- Fecha: 2024
-- =============================================

-- Ver todos los usuarios con sus roles
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RoleName,
    u.Id AS UserId,
    r.Id AS RoleId
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email;

-- Ver solo usuarios sin rol
SELECT 
    u.Email,
    u.UserName,
    u.Id
FROM AspNetUsers u
WHERE u.Id NOT IN (SELECT UserId FROM AspNetUserRoles)
ORDER BY u.Email;

-- Ver usuarios con rol Estudiante
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Estudiante'
ORDER BY u.Email;

-- Ver todos los roles disponibles
SELECT 
    Id,
    Name
FROM AspNetRoles
ORDER BY Name;

