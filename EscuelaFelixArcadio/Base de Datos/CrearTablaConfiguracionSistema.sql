-- ========================================
-- Crear tabla ConfiguracionSistema
-- ========================================

USE EscuelaFelixArcadio;
GO

-- Crear tabla si no existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionSistema]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConfiguracionSistema] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Nombre] NVARCHAR(100) NOT NULL,
        [Valor] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(500) NULL,
        [Categoria] NVARCHAR(50) NULL,
        [FechaActualizacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [UsuarioActualizacion] NVARCHAR(450) NULL,
        CONSTRAINT [PK_ConfiguracionSistema] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_ConfiguracionSistema_Nombre] UNIQUE NONCLUSTERED ([Nombre] ASC)
    );
    
    PRINT 'Tabla ConfiguracionSistema creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla ConfiguracionSistema ya existe.';
END
GO

-- Insertar configuraciones por defecto
IF NOT EXISTS (SELECT * FROM ConfiguracionSistema WHERE Nombre = 'NombreInstitucion')
BEGIN
    INSERT INTO [dbo].[ConfiguracionSistema] ([Nombre], [Valor], [Descripcion], [Categoria])
    VALUES 
        ('NombreInstitucion', 'Escuela Félix Arcadio Montero Monge', 'Nombre oficial de la institución', 'General'),
        ('CorreoPrincipal', 'contacto@escuela.edu', 'Correo electrónico principal de contacto', 'General'),
        ('TelefonoContacto', '+506 2222-2222', 'Número telefónico de contacto', 'General'),
        ('IdiomaSistema', 'Español', 'Idioma por defecto del sistema', 'General'),
        ('DuracionSesion', '30', 'Duración de sesión en minutos', 'Seguridad'),
        ('IntentosLogin', '3', 'Número de intentos permitidos de inicio de sesión', 'Seguridad'),
        ('RequerirPasswordSegura', 'true', 'Requerir contraseñas seguras', 'Seguridad'),
        ('BloqueoInactividad', 'true', 'Bloquear cuenta por inactividad', 'Seguridad'),
        ('NotificacionesCorreo', 'true', 'Habilitar notificaciones por correo', 'Notificaciones'),
        ('AlertasPrestamos', 'true', 'Enviar alertas de préstamos pendientes', 'Notificaciones'),
        ('NotificarSanciones', 'false', 'Notificar sobre sanciones', 'Notificaciones'),
        ('CorreoNotificaciones', 'noreply@escuela.edu', 'Correo que envía notificaciones', 'Notificaciones'),
        ('ModoOscuro', 'false', 'Activar modo oscuro en la interfaz', 'Apariencia'),
        ('ZonaHoraria', 'America/Costa_Rica (GMT-6)', 'Zona horaria del sistema', 'Apariencia'),
        ('FormatoFecha', 'DD/MM/YYYY HH:MM', 'Formato de fecha y hora', 'Apariencia'),
        ('MantenerSesion', 'false', 'Mantener sesión activa', 'Apariencia'),
        ('ModoMantenimiento', 'false', 'Activar modo de mantenimiento', 'Sistema'),
        ('FrecuenciaBackup', 'Semanal', 'Frecuencia de backups automáticos', 'Sistema'),
        ('RegistroActividades', 'true', 'Registrar actividades del sistema', 'Sistema'),
        ('LimiteRegistros', '12', 'Límite de registros por página', 'Sistema');
    
    PRINT 'Configuraciones por defecto insertadas.';
END
ELSE
BEGIN
    PRINT 'Las configuraciones por defecto ya existen.';
END
GO

-- Mostrar todas las configuraciones
SELECT * FROM ConfiguracionSistema ORDER BY Categoria, Nombre;
GO

