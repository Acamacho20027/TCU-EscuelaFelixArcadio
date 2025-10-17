-- Actualizar registros existentes para que tengan IdVariante específicos
-- Cronómetros (IdProducto = 8) - Asignar a variante 1 (si existe) o crear variante
UPDATE NumeroSerieProducto 
SET IdVariante = 1 
WHERE IdProducto = 8 AND NumeroSerie IN ('CRONO001', 'CRONO002', 'CRONO003', 'CRONO004', 'CRONO005');

-- Raquetas de Tenis (IdProducto = 13) - Asignar a variante 1 (si existe) o crear variante
UPDATE NumeroSerieProducto 
SET IdVariante = 1 
WHERE IdProducto = 13 AND NumeroSerie IN ('RAQ001', 'RAQ002', 'RAQ003', 'RAQ004', 'RAQ005');

-- Si no existen las variantes, crearlas primero:
-- INSERT INTO VarianteProducto (IdProducto, CodigoVariante, NombreVariante, CostoAdicional, FechaCreacion) VALUES 
-- (8, 'CRONO-STD', 'Cronometro Estandar', 0, GETDATE()),
-- (13, 'RAQ-STD', 'Raqueta Estandar', 0, GETDATE());

-- Agregar nuevos números de serie con variantes específicas
INSERT INTO NumeroSerieProducto (IdVariante, IdProducto, NumeroSerie, IdEstado, Ubicacion, FechaCreacion) VALUES 
-- Cronómetros con variante específica
(1, 8, 'CRONO006', 1, 'Oficina de Educacion Fisica', GETDATE()),
(1, 8, 'CRONO007', 1, 'Oficina de Educacion Fisica', GETDATE()),
(1, 8, 'CRONO008', 2, 'En Reparacion', GETDATE()),

-- Raquetas de Tenis con variante específica
(1, 13, 'RAQ006', 1, 'Almacen Principal', GETDATE()),
(1, 13, 'RAQ007', 1, 'Almacen Principal', GETDATE()),
(1, 13, 'RAQ008', 2, 'En Mantenimiento', GETDATE()),

-- Balones de Futbol (IdProducto = 1) con variantes específicas
(1, 1, 'FB001-SERIE001', 1, 'Almacen Principal', GETDATE()),
(1, 1, 'FB001-SERIE002', 1, 'Almacen Principal', GETDATE()),
(2, 1, 'FB002-SERIE001', 1, 'Almacen Principal', GETDATE()),
(2, 1, 'FB002-SERIE002', 1, 'Almacen Principal', GETDATE()),
(3, 1, 'FB003-SERIE001', 1, 'Almacen Principal', GETDATE()),

-- Chalecos de Entrenamiento (IdProducto = 3) con variantes específicas
(4, 3, 'CHALECO-AZUL-001', 1, 'Almacen Principal', GETDATE()),
(4, 3, 'CHALECO-AZUL-002', 1, 'Almacen Principal', GETDATE()),
(5, 3, 'CHALECO-ROJO-001', 1, 'Almacen Principal', GETDATE()),
(5, 3, 'CHALECO-ROJO-002', 1, 'Almacen Principal', GETDATE()),
(6, 3, 'CHALECO-VERDE-001', 1, 'Almacen Principal', GETDATE()),
(6, 3, 'CHALECO-VERDE-002', 1, 'Almacen Principal', GETDATE()),

-- Balones de Basquetbol (IdProducto = 4) con variantes específicas
(7, 4, 'BB-JUVENIL-001', 1, 'Almacen Principal', GETDATE()),
(7, 4, 'BB-JUVENIL-002', 1, 'Almacen Principal', GETDATE()),
(8, 4, 'BB-ADULTO-001', 1, 'Almacen Principal', GETDATE()),
(8, 4, 'BB-ADULTO-002', 1, 'Almacen Principal', GETDATE()),

-- Balones de Volleyball (IdProducto = 6)
(1, 6, 'VB001-SERIE001', 1, 'Almacen Principal', GETDATE()),
(1, 6, 'VB001-SERIE002', 1, 'Almacen Principal', GETDATE()),
(1, 6, 'VB001-SERIE003', 2, 'En Reparacion', GETDATE()),

-- Red de Volleyball (IdProducto = 7)
(1, 7, 'RED-VB-001', 1, 'Almacen Principal', GETDATE()),
(1, 7, 'RED-VB-002', 1, 'Almacen Principal', GETDATE()),

-- Barras de Salto (IdProducto = 9)
(1, 9, 'BARRA-SALTO-001', 1, 'Almacen Principal', GETDATE()),
(1, 9, 'BARRA-SALTO-002', 1, 'Almacen Principal', GETDATE()),

-- Volantes de Badminton (IdProducto = 11)
(1, 11, 'VOLANTE-BD-001', 1, 'Almacen Principal', GETDATE()),
(1, 11, 'VOLANTE-BD-002', 1, 'Almacen Principal', GETDATE()),
(1, 11, 'VOLANTE-BD-003', 1, 'Almacen Principal', GETDATE()),

-- Red de Badminton (IdProducto = 12)
(1, 12, 'RED-BD-001', 1, 'Almacen Principal', GETDATE()),
(1, 12, 'RED-BD-002', 1, 'Almacen Principal', GETDATE()),

-- Balones de Tenis (IdProducto = 14)
(1, 14, 'TENIS-BALL-001', 1, 'Almacen Principal', GETDATE()),
(1, 14, 'TENIS-BALL-002', 1, 'Almacen Principal', GETDATE()),
(1, 14, 'TENIS-BALL-003', 1, 'Almacen Principal', GETDATE()),

-- Colchonetas (IdProducto = 15)
(1, 15, 'COLCHONETA-001', 1, 'Almacen Principal', GETDATE()),
(1, 15, 'COLCHONETA-002', 1, 'Almacen Principal', GETDATE()),
(1, 15, 'COLCHONETA-003', 1, 'Almacen Principal', GETDATE()),

-- Barras Paralelas (IdProducto = 16)
(1, 16, 'BARRAS-PARALELAS-001', 1, 'Almacen Principal', GETDATE()),
(1, 16, 'BARRAS-PARALELAS-002', 1, 'Almacen Principal', GETDATE());