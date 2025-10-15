-- =============================================
-- Script: Cambiar Usuario de Estudiante a Administrador
-- Descripción: Cambia el rol de un usuario específico de "Estudiante" a "Administrador"
-- Fecha: 2024
-- Autor: Sistema Escuela Félix Arcadio Montero Monge
-- =============================================

-- Cambiar de Estudiante a Administrador
DELETE FROM AspNetUserRoles 
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'admin.escuelafelixarcadio@gmail.com')
AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Estudiante')

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT 
    (SELECT Id FROM AspNetUsers WHERE Email = 'admin.escuelafelixarcadio@gmail.com'),
    (SELECT Id FROM AspNetRoles WHERE Name = 'Administrador')

-- Verificar el cambio
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin.escuelafelixarcadio@gmail.com'

