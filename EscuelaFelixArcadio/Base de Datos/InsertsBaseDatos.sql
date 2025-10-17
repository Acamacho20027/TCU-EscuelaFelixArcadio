-- =====================================================
-- SCRIPT DE INSERTS PARA BASE DE DATOS
-- ESCUELA FELIX ARCADIO MONTERO MONGE
-- =====================================================

-- 1. ESTADO (Activo/Inactivo)
-- =====================================================
INSERT INTO Estado (Descripcion) VALUES 
('Activo'),
('Inactivo');

-- 2. CATEGORIA (Categorías de productos deportivos)
-- =====================================================
INSERT INTO Categoria (Nombre, Descripcion, IdEstado, FechaCreacion) VALUES 
('Futbol', 'Equipos y materiales para futbol', 1, GETDATE()),
('Basquetbol', 'Equipos y materiales para basquetbol', 1, GETDATE()),
('Volleyball', 'Equipos y materiales para volleyball', 1, GETDATE()),
('Atletismo', 'Equipos y materiales para atletismo', 1, GETDATE()),
('Badminton', 'Equipos y materiales para badminton', 1, GETDATE()),
('Tenis', 'Equipos y materiales para tenis', 1, GETDATE()),
('Gimnasia', 'Equipos y materiales para gimnasia', 1, GETDATE());

-- 3. PRODUCTO (Productos deportivos)
-- =====================================================
INSERT INTO Producto (Codigo, Nombre, Descripcion, Marca, EsServicio, Eliminado, IdEstado, IdCategoria, FechaCreacion) VALUES 
-- Futbol
('FB001', 'Balon de Futbol', 'Balon oficial de futbol talla 5', 'Nike', 0, 0, 1, 1, GETDATE()),
('FB002', 'Conos de Entrenamiento', 'Set de 10 conos para entrenamiento', 'Generic', 0, 0, 1, 1, GETDATE()),
('FB003', 'Chalecos de Entrenamiento', 'Set de 20 chalecos de colores', 'Adidas', 0, 0, 1, 1, GETDATE()),

-- Basquetbol
('BB001', 'Balon de Basquetbol', 'Balon oficial de basquetbol talla 7', 'Spalding', 0, 0, 1, 2, GETDATE()),
('BB002', 'Aros de Basquetbol', 'Aros reglamentarios con redes', 'Wilson', 0, 0, 1, 2, GETDATE()),

-- Volleyball
('VB001', 'Balon de Volleyball', 'Balon oficial de volleyball', 'Mikasa', 0, 0, 1, 3, GETDATE()),
('VB002', 'Red de Volleyball', 'Red oficial de volleyball', 'Generic', 0, 0, 1, 3, GETDATE()),

-- Atletismo
('AT001', 'Cronometro Digital', 'Cronometro digital para competencias', 'Casio', 0, 0, 1, 4, GETDATE()),
('AT002', 'Barras de Salto', 'Barras para salto de altura', 'Generic', 0, 0, 1, 4, GETDATE()),

-- Badminton
('BD001', 'Raquetas de Badminton', 'Raquetas profesionales de badminton', 'Yonex', 0, 0, 1, 5, GETDATE()),
('BD002', 'Volantes de Badminton', 'Volantes oficiales de badminton', 'Yonex', 0, 0, 1, 5, GETDATE()),
('BD003', 'Red de Badminton', 'Red oficial de badminton', 'Generic', 0, 0, 1, 5, GETDATE()),

-- Tenis
('TN001', 'Raquetas de Tenis', 'Raquetas profesionales', 'Wilson', 0, 0, 1, 6, GETDATE()),
('TN002', 'Balones de Tenis', 'Balones oficiales de tenis', 'Wilson', 0, 0, 1, 6, GETDATE()),

-- Gimnasia
('GM001', 'Colchonetas', 'Colchonetas para gimnasia', 'Generic', 0, 0, 1, 7, GETDATE()),
('GM002', 'Barras Paralelas', 'Barras paralelas de gimnasia', 'Generic', 0, 0, 1, 7, GETDATE());

-- 4. VARIANTEPRODUCTO (Variantes de productos)
-- =====================================================
INSERT INTO VarianteProducto (IdProducto, CodigoVariante, NombreVariante, CostoAdicional, FechaCreacion) VALUES 
-- Variantes de balones de futbol
(1, 'FB001-S', 'Balon de Futbol - Talla S', 0, GETDATE()),
(1, 'FB001-M', 'Balon de Futbol - Talla M', 0, GETDATE()),
(1, 'FB001-L', 'Balon de Futbol - Talla L', 0, GETDATE()),

-- Variantes de chalecos
(3, 'FB003-AZUL', 'Chalecos Azules', 0, GETDATE()),
(3, 'FB003-ROJO', 'Chalecos Rojos', 0, GETDATE()),
(3, 'FB003-VERDE', 'Chalecos Verdes', 0, GETDATE()),

-- Variantes de balones de basquetbol
(4, 'BB001-JUVENIL', 'Balon Basquetbol Juvenil', 0, GETDATE()),
(4, 'BB001-ADULTO', 'Balon Basquetbol Adulto', 0, GETDATE()),

-- Variantes de raquetas de badminton
(10, 'BD001-ADULTO', 'Raqueta Badminton Adulto', 0, GETDATE()),
(10, 'BD001-JUVENIL', 'Raqueta Badminton Juvenil', 0, GETDATE());

-- 5. ESPACIO (Solo el Gimnasio)
-- =====================================================
INSERT INTO Espacio (Codigo, Nombre, Descripcion, Capacidad, Ubicacion, IdEstado, FechaCreacion) VALUES 
('GIM001', 'Gimnasio Principal', 'Gimnasio principal de la escuela con cancha de basquetbol y volleyball', 50, 'Edificio Principal - Planta Baja', 1, GETDATE());

-- 6. TIPOMOVIMIENTOINVENTARIO (Tipos de movimientos)
-- =====================================================
INSERT INTO TipoMovimientoInventario (Descripcion) VALUES 
('Ingreso'),
('Salida'),
('Ajuste'),
('Prestamo'),
('Devolucion');

-- 7. INVENTARIO (Stock inicial de productos)
-- =====================================================
INSERT INTO Inventario (IdEstado, IdProducto, IdVariante, Cantidad, Minimo, Maximo, FechaActualizacion) VALUES 
-- Balones de Futbol
(1, 1, 1, 15, 5, 25, GETDATE()), -- Balon Futbol Talla S
(1, 1, 2, 20, 8, 30, GETDATE()), -- Balon Futbol Talla M
(1, 1, 3, 10, 3, 20, GETDATE()), -- Balon Futbol Talla L

-- Conos de Entrenamiento
(1, 2, NULL, 50, 10, 100, GETDATE()),

-- Chalecos de Entrenamiento
(1, 3, 4, 25, 5, 50, GETDATE()), -- Chalecos Azules
(1, 3, 5, 25, 5, 50, GETDATE()), -- Chalecos Rojos
(1, 3, 6, 25, 5, 50, GETDATE()), -- Chalecos Verdes

-- Balones de Basquetbol
(1, 4, 7, 12, 3, 20, GETDATE()), -- Balon Basquetbol Juvenil
(1, 4, 8, 8, 2, 15, GETDATE()), -- Balon Basquetbol Adulto

-- Aros de Basquetbol
(1, 5, NULL, 4, 1, 8, GETDATE()),

-- Balones de Volleyball
(1, 6, NULL, 15, 5, 25, GETDATE()),

-- Red de Volleyball
(1, 7, NULL, 2, 1, 4, GETDATE()),

-- Cronometro Digital
(1, 8, NULL, 5, 1, 10, GETDATE()),

-- Barras de Salto
(1, 9, NULL, 3, 1, 6, GETDATE()),

-- Raquetas de Badminton
(1, 10, 9, 8, 2, 15, GETDATE()), -- Raqueta Badminton Adulto
(1, 10, 10, 6, 2, 12, GETDATE()), -- Raqueta Badminton Juvenil

-- Volantes de Badminton
(1, 11, NULL, 100, 20, 200, GETDATE()),

-- Red de Badminton
(1, 12, NULL, 2, 1, 4, GETDATE()),

-- Raquetas de Tenis
(1, 13, NULL, 10, 3, 20, GETDATE()),

-- Balones de Tenis
(1, 14, NULL, 50, 15, 100, GETDATE()),

-- Colchonetas
(1, 15, NULL, 20, 5, 40, GETDATE()),

-- Barras Paralelas
(1, 16, NULL, 2, 1, 4, GETDATE());

-- 8. NUMEROSERIEPRODUCTO (Números de serie para productos específicos)
-- =====================================================
INSERT INTO NumeroSerieProducto (IdVariante, IdProducto, NumeroSerie, IdEstado, Ubicacion, FechaCreacion) VALUES 
-- Cronometros digitales
(NULL, 8, 'CRONO001', 1, 'Oficina de Educacion Fisica', GETDATE()),
(NULL, 8, 'CRONO002', 1, 'Oficina de Educacion Fisica', GETDATE()),
(NULL, 8, 'CRONO003', 1, 'Oficina de Educacion Fisica', GETDATE()),
(NULL, 8, 'CRONO004', 2, 'En Reparacion', GETDATE()),
(NULL, 8, 'CRONO005', 1, 'Oficina de Educacion Fisica', GETDATE()),

-- Raquetas de tenis
(NULL, 13, 'RAQ001', 1, 'Almacen Principal', GETDATE()),
(NULL, 13, 'RAQ002', 1, 'Almacen Principal', GETDATE()),
(NULL, 13, 'RAQ003', 2, 'En Mantenimiento', GETDATE()),
(NULL, 13, 'RAQ004', 1, 'Almacen Principal', GETDATE()),
(NULL, 13, 'RAQ005', 1, 'Almacen Principal', GETDATE()),

-- Raquetas de badminton
(9, 10, 'BADM001', 1, 'Almacen Principal', GETDATE()),
(9, 10, 'BADM002', 1, 'Almacen Principal', GETDATE()),
(10, 10, 'BADM003', 1, 'Almacen Principal', GETDATE()),
(10, 10, 'BADM004', 2, 'En Reparacion', GETDATE());

-- 9. MOVIMIENTOINVENTARIO (Movimientos iniciales de inventario)
-- =====================================================
INSERT INTO MovimientoInventario (IdProducto, IdVariante, IdSerie, IdEstadoInventario, Cantidad, TipoMovimiento, Referencia, Notas, Id, FechaMovimiento) VALUES 
-- Ingresos iniciales
(1, 1, NULL, 1, 15, 'Ingreso', 'COMPRA001', 'Compra inicial de balones de futbol talla S', 'admin', GETDATE()),
(1, 2, NULL, 1, 20, 'Ingreso', 'COMPRA001', 'Compra inicial de balones de futbol talla M', 'admin', GETDATE()),
(1, 3, NULL, 1, 10, 'Ingreso', 'COMPRA001', 'Compra inicial de balones de futbol talla L', 'admin', GETDATE()),
(2, NULL, NULL, 1, 50, 'Ingreso', 'COMPRA002', 'Compra inicial de conos de entrenamiento', 'admin', GETDATE()),
(3, 4, NULL, 1, 25, 'Ingreso', 'COMPRA003', 'Compra inicial de chalecos azules', 'admin', GETDATE()),
(3, 5, NULL, 1, 25, 'Ingreso', 'COMPRA003', 'Compra inicial de chalecos rojos', 'admin', GETDATE()),
(3, 6, NULL, 1, 25, 'Ingreso', 'COMPRA003', 'Compra inicial de chalecos verdes', 'admin', GETDATE()),
(10, 9, NULL, 1, 8, 'Ingreso', 'COMPRA004', 'Compra inicial de raquetas de badminton adulto', 'admin', GETDATE()),
(10, 10, NULL, 1, 6, 'Ingreso', 'COMPRA004', 'Compra inicial de raquetas de badminton juvenil', 'admin', GETDATE()),
(11, NULL, NULL, 1, 100, 'Ingreso', 'COMPRA005', 'Compra inicial de volantes de badminton', 'admin', GETDATE());

-- 10. PRESTAMO (Préstamos de ejemplo)
-- =====================================================
INSERT INTO Prestamo (NumeroPrestamo, Id, IdEstado, FechadeCreacion, FechaDevolucion, Notas, Devolucion) VALUES 
('PREST001', 'admin', 1, GETDATE(), DATEADD(day, 7, GETDATE()), 'Prestamo de balones para entrenamiento', 0),
('PREST002', 'admin', 1, GETDATE(), DATEADD(day, 3, GETDATE()), 'Prestamo de chalecos para partido', 0),
('PREST003', 'admin', 1, GETDATE(), NULL, 'Prestamo de cronometro para competencia', 0);

-- 11. RESERVA (Reservas de ejemplo)
-- =====================================================
INSERT INTO Reserva (NumeroReserva, Id, TipoRecurso, IdRecurso, IdVariante, FechaInicio, FechaFin, IdEstado, FechaCreacion) VALUES 
('RES001', 'admin', 'producto', 1, 1, GETDATE(), DATEADD(hour, 2, GETDATE()), 1, GETDATE()),
('RES002', 'admin', 'espacio', 1, NULL, GETDATE(), DATEADD(hour, 3, GETDATE()), 1, GETDATE()),
('RES003', 'admin', 'producto', 4, 7, DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE())), 1, GETDATE());

-- 12. RESERVAESPACIO (Reservas del gimnasio)
-- =====================================================
INSERT INTO ReservaEspacio (IdEspacio, Id, FechaInicio, FechaFin, IdEstado, Notas, FechaCreacion) VALUES 
(1, 'admin', GETDATE(), DATEADD(hour, 2, GETDATE()), 1, 'Reserva para clase de educacion fisica', GETDATE()),
(1, 'admin', DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 3, GETDATE())), 1, 'Reserva para entrenamiento de basquetbol', GETDATE()),
(1, 'admin', DATEADD(day, 2, GETDATE()), DATEADD(day, 2, DATEADD(hour, 1, GETDATE())), 1, 'Reserva para clase de volleyball', GETDATE());

-- 13. MANTENIMIENTOESPACIO (Mantenimientos del gimnasio)
-- =====================================================
INSERT INTO MantenimientoEspacio (IdEspacio, Id, Descripcion, FechaInicio, FechaFin, IdEstado, Costo) VALUES 
(1, 'admin', 'Mantenimiento preventivo del piso del gimnasio', GETDATE(), DATEADD(day, 1, GETDATE()), 1, 500.00),
(1, 'admin', 'Reparacion de iluminacion', DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE()), 1, 300.00),
(1, 'admin', 'Limpieza profunda y desinfeccion', DATEADD(day, -10, GETDATE()), DATEADD(day, -9, GETDATE()), 1, 200.00);

-- 14. SANCION (Sanciones de ejemplo)
-- =====================================================
INSERT INTO Sancion (Id, IdPrestamo, IdEstado, Motivo, Tipo, Monto, FechaInicio, FechaFin) VALUES 
('admin', 1, 1, 'Devolucion tardia de material deportivo', 'Retraso', 50.00, GETDATE(), DATEADD(day, 7, GETDATE())),
('admin', 2, 1, 'Mal uso del equipo prestado', 'Mal Uso', 100.00, GETDATE(), DATEADD(day, 15, GETDATE())),
('admin', NULL, 1, 'No devolucion de material', 'Perdida', 200.00, GETDATE(), DATEADD(day, 30, GETDATE()));

-- 15. DOCUMENTO (Documentos de ejemplo)
-- =====================================================
INSERT INTO Documento (Titulo, Descripcion, Id, FechaSubida, Publico) VALUES 
('Manual de Uso de Equipos Deportivos', 'Manual completo para el uso correcto de todos los equipos deportivos', 'admin', GETDATE(), 1),
('Reglamento de Prestamos', 'Reglamento interno para prestamos de material deportivo', 'admin', GETDATE(), 1),
('Procedimientos de Seguridad', 'Procedimientos de seguridad para actividades deportivas', 'admin', GETDATE(), 1),
('Inventario Mensual', 'Reporte mensual de inventario - Enero 2024', 'admin', GETDATE(), 0);

-- =====================================================
-- FIN DEL SCRIPT DE INSERTS
-- =====================================================

-- Verificar que los datos se insertaron correctamente
SELECT 'Estado' as Tabla, COUNT(*) as Registros FROM Estado
UNION ALL
SELECT 'Categoria', COUNT(*) FROM Categoria
UNION ALL
SELECT 'Producto', COUNT(*) FROM Producto
UNION ALL
SELECT 'VarianteProducto', COUNT(*) FROM VarianteProducto
UNION ALL
SELECT 'Espacio', COUNT(*) FROM Espacio
UNION ALL
SELECT 'TipoMovimientoInventario', COUNT(*) FROM TipoMovimientoInventario
UNION ALL
SELECT 'Inventario', COUNT(*) FROM Inventario
UNION ALL
SELECT 'NumeroSerieProducto', COUNT(*) FROM NumeroSerieProducto
UNION ALL
SELECT 'MovimientoInventario', COUNT(*) FROM MovimientoInventario
UNION ALL
SELECT 'Prestamo', COUNT(*) FROM Prestamo
UNION ALL
SELECT 'Reserva', COUNT(*) FROM Reserva
UNION ALL
SELECT 'ReservaEspacio', COUNT(*) FROM ReservaEspacio
UNION ALL
SELECT 'MantenimientoEspacio', COUNT(*) FROM MantenimientoEspacio
UNION ALL
SELECT 'Sancion', COUNT(*) FROM Sancion
UNION ALL
SELECT 'Documento', COUNT(*) FROM Documento;
