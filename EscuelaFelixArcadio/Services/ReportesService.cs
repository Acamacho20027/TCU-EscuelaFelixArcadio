using EscuelaFelixArcadio.Models;
using EscuelaFelixArcadio.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Services
{
    public class ReportesService
    {
        private readonly ApplicationDbContext _context;

        public ReportesService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Datos de Inventario

        public List<ReporteInventarioViewModel> ObtenerDatosInventario(DateTime? fechaInicio, DateTime? fechaFin, List<int> categorias = null, List<int> estados = null)
        {
            var query = _context.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .AsNoTracking();

            if (fechaInicio.HasValue)
                query = query.Where(i => i.FechaActualizacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(i => i.FechaActualizacion <= fechaFin.Value);

            if (categorias != null && categorias.Any())
                query = query.Where(i => i.Producto.IdCategoria.HasValue && categorias.Contains(i.Producto.IdCategoria.Value));

            if (estados != null && estados.Any())
                query = query.Where(i => estados.Contains(i.IdEstado));

            return query.Select(i => new ReporteInventarioViewModel
            {
                IdInventario = i.IdInventario,
                Producto = i.Producto.Nombre,
                Codigo = i.Producto.Codigo,
                Categoria = i.Producto.Categoria != null ? i.Producto.Categoria.Nombre : "Sin categoría",
                Estado = i.Estado.Descripcion,
                Cantidad = i.Cantidad,
                Minimo = i.Minimo,
                Maximo = i.Maximo,
                Variante = i.Variante != null ? i.Variante.NombreVariante : "N/A",
                FechaActualizacion = i.FechaActualizacion,
                AlertaStockBajo = i.Cantidad <= i.Minimo
            }).ToList();
        }

        #endregion

        #region Datos de Préstamos

        public List<ReportePrestamosViewModel> ObtenerDatosPrestamos(DateTime? fechaInicio, DateTime? fechaFin, List<string> usuarios = null, List<int> estados = null, bool? devueltos = null)
        {
            var query = _context.Prestamo
                .Include(p => p.ApplicationUser)
                .Include(p => p.Estado)
                .AsNoTracking();

            if (fechaInicio.HasValue)
                query = query.Where(p => p.FechadeCreacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(p => p.FechadeCreacion <= fechaFin.Value);

            if (usuarios != null && usuarios.Any())
                query = query.Where(p => usuarios.Contains(p.Id));

            if (estados != null && estados.Any())
                query = query.Where(p => estados.Contains(p.IdEstado));

            if (devueltos.HasValue)
                query = query.Where(p => p.Devolucion == devueltos.Value);

            // Primero materializamos los datos
            var prestamos = query.Select(p => new
            {
                p.IdPrestamo,
                p.NumeroPrestamo,
                Usuario = p.ApplicationUser.UserName,
                NombreCompleto = p.ApplicationUser.UserName,
                Estado = p.Estado.Descripcion,
                p.FechadeCreacion,
                p.FechaDevolucion,
                p.FechaVencimiento,
                p.Devolucion,
                p.MotivoPrestamo,
                p.EsUrgente
            }).ToList();

            // Luego calculamos DiasRetraso en memoria
            return prestamos.Select(p => new ReportePrestamosViewModel
            {
                IdPrestamo = p.IdPrestamo,
                NumeroPrestamo = p.NumeroPrestamo,
                Usuario = p.Usuario,
                NombreCompleto = p.NombreCompleto,
                Estado = p.Estado,
                FechadeCreacion = p.FechadeCreacion,
                FechaDevolucion = p.FechaDevolucion,
                FechaVencimiento = p.FechaVencimiento,
                Devolucion = p.Devolucion,
                MotivoPrestamo = p.MotivoPrestamo,
                EsUrgente = p.EsUrgente,
                DiasRetraso = p.FechaVencimiento.HasValue && !p.Devolucion && p.FechaVencimiento.Value < DateTime.Now
                    ? (long)(DateTime.Now - p.FechaVencimiento.Value).TotalDays
                    : 0L
            }).ToList();
        }

        #endregion

        #region Datos de Sanciones

        public List<ReporteSancionesViewModel> ObtenerDatosSanciones(DateTime? fechaInicio, DateTime? fechaFin, List<string> usuarios = null, List<int> estados = null, List<string> tipos = null)
        {
            var query = _context.Sancion
                .Include(s => s.ApplicationUser)
                .Include(s => s.Estado)
                .Include(s => s.Prestamo)
                .AsNoTracking();

            if (fechaInicio.HasValue)
                query = query.Where(s => s.FechaInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(s => s.FechaInicio <= fechaFin.Value);

            if (usuarios != null && usuarios.Any())
                query = query.Where(s => usuarios.Contains(s.Id));

            if (estados != null && estados.Any())
                query = query.Where(s => estados.Contains(s.IdEstado));

            if (tipos != null && tipos.Any())
                query = query.Where(s => tipos.Contains(s.Tipo));

            return query.Select(s => new ReporteSancionesViewModel
            {
                IdSancion = s.IdSancion,
                Usuario = s.ApplicationUser.UserName,
                NombreCompleto = s.ApplicationUser.UserName,
                Estado = s.Estado.Descripcion,
                Tipo = s.Tipo,
                Motivo = s.Motivo,
                Monto = s.Monto,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                PrestamoRelacionado = s.Prestamo != null ? s.Prestamo.NumeroPrestamo : "N/A",
                SancionActiva = !s.FechaFin.HasValue || s.FechaFin.Value > DateTime.Now
            }).ToList();
        }

        #endregion

        #region Comparación de Períodos

        public object CompararPeriodos(string tipoReporte, DateTime periodo1Inicio, DateTime periodo1Fin, DateTime periodo2Inicio, DateTime periodo2Fin)
        {
            switch (tipoReporte.ToLower())
            {
                case "prestamos":
                    return CompararPrestamosPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);
                case "inventario":
                    return CompararInventarioPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);
                case "sanciones":
                    return CompararSancionesPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);
                default:
                    return null;
            }
        }

        private object CompararPrestamosPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var periodo1 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p1Inicio && p.FechadeCreacion <= p1Fin)
                .AsNoTracking();

            var periodo2 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p2Inicio && p.FechadeCreacion <= p2Fin)
                .AsNoTracking();

            var totalP1 = periodo1.Count();
            var totalP2 = periodo2.Count();
            var devueltosP1 = periodo1.Count(p => p.Devolucion);
            var devueltosP2 = periodo2.Count(p => p.Devolucion);

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio,
                    Fin = p1Fin,
                    TotalPrestamos = totalP1,
                    Devueltos = devueltosP1,
                    Pendientes = totalP1 - devueltosP1,
                    TasaDevolucion = totalP1 > 0 ? (devueltosP1 * 100.0 / totalP1) : 0
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio,
                    Fin = p2Fin,
                    TotalPrestamos = totalP2,
                    Devueltos = devueltosP2,
                    Pendientes = totalP2 - devueltosP2,
                    TasaDevolucion = totalP2 > 0 ? (devueltosP2 * 100.0 / totalP2) : 0
                },
                Diferencias = new
                {
                    TotalPrestamos = totalP2 - totalP1,
                    TotalPrestamosPorc = totalP1 > 0 ? ((totalP2 - totalP1) * 100.0 / totalP1) : 0,
                    Devueltos = devueltosP2 - devueltosP1,
                    DevueltosPorc = devueltosP1 > 0 ? ((devueltosP2 - devueltosP1) * 100.0 / devueltosP1) : 0
                }
            };
        }

        private object CompararInventarioPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var periodo1 = _context.Inventario
                .Where(i => i.FechaActualizacion >= p1Inicio && i.FechaActualizacion <= p1Fin)
                .AsNoTracking();

            var periodo2 = _context.Inventario
                .Where(i => i.FechaActualizacion >= p2Inicio && i.FechaActualizacion <= p2Fin)
                .AsNoTracking();

            var totalItemsP1 = periodo1.Sum(i => i.Cantidad);
            var totalItemsP2 = periodo2.Sum(i => i.Cantidad);
            var alertasP1 = periodo1.Count(i => i.Cantidad <= i.Minimo);
            var alertasP2 = periodo2.Count(i => i.Cantidad <= i.Minimo);

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio,
                    Fin = p1Fin,
                    TotalItems = totalItemsP1,
                    AlertasStockBajo = alertasP1
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio,
                    Fin = p2Fin,
                    TotalItems = totalItemsP2,
                    AlertasStockBajo = alertasP2
                },
                Diferencias = new
                {
                    TotalItems = totalItemsP2 - totalItemsP1,
                    TotalItemsPorc = totalItemsP1 > 0 ? ((totalItemsP2 - totalItemsP1) * 100.0 / totalItemsP1) : 0,
                    Alertas = alertasP2 - alertasP1
                }
            };
        }

        private object CompararSancionesPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var periodo1 = _context.Sancion
                .Where(s => s.FechaInicio >= p1Inicio && s.FechaInicio <= p1Fin)
                .AsNoTracking();

            var periodo2 = _context.Sancion
                .Where(s => s.FechaInicio >= p2Inicio && s.FechaInicio <= p2Fin)
                .AsNoTracking();

            var totalP1 = periodo1.Count();
            var totalP2 = periodo2.Count();
            var montoP1 = periodo1.Sum(s => s.Monto);
            var montoP2 = periodo2.Sum(s => s.Monto);

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio,
                    Fin = p1Fin,
                    TotalSanciones = totalP1,
                    MontoTotal = montoP1
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio,
                    Fin = p2Fin,
                    TotalSanciones = totalP2,
                    MontoTotal = montoP2
                },
                Diferencias = new
                {
                    TotalSanciones = totalP2 - totalP1,
                    TotalSancionesPorc = totalP1 > 0 ? ((totalP2 - totalP1) * 100.0 / totalP1) : 0,
                    MontoTotal = montoP2 - montoP1,
                    MontoTotalPorc = montoP1 > 0 ? ((montoP2 - montoP1) * 100.0m / montoP1) : 0
                }
            };
        }

        #endregion

        #region Alertas

        public void GenerarAlertasAutomaticas()
        {
            // Alerta de stock bajo
            var productosStockBajo = _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Cantidad <= i.Minimo)
                .ToList();

            foreach (var item in productosStockBajo)
            {
                var alertaExistente = _context.AlertaReporte
                    .FirstOrDefault(a => a.TipoAlerta == "StockBajo" &&
                                        a.IdRegistroRelacionado == item.IdInventario &&
                                        !a.Leida);

                if (alertaExistente == null)
                {
                    _context.AlertaReporte.Add(new AlertaReporte
                    {
                        TipoAlerta = "StockBajo",
                        Descripcion = $"El producto '{item.Producto.Nombre}' tiene stock bajo. Cantidad actual: {item.Cantidad}, Mínimo: {item.Minimo}",
                        Severidad = item.Cantidad == 0 ? "Crítica" : "Alta",
                        IdRegistroRelacionado = item.IdInventario,
                        TipoRegistroRelacionado = "Inventario",
                        RequiereAccion = true
                    });
                }
            }

            // Alerta de préstamos vencidos
            var prestamosVencidos = _context.Prestamo
                .Where(p => !p.Devolucion &&
                           p.FechaVencimiento.HasValue &&
                           p.FechaVencimiento.Value < DateTime.Now)
                .ToList();

            foreach (var prestamo in prestamosVencidos)
            {
                var diasRetraso = (DateTime.Now - prestamo.FechaVencimiento.Value).Days;
                var alertaExistente = _context.AlertaReporte
                    .FirstOrDefault(a => a.TipoAlerta == "PrestamoVencido" &&
                                        a.IdRegistroRelacionado == prestamo.IdPrestamo &&
                                        !a.Leida);

                if (alertaExistente == null)
                {
                    _context.AlertaReporte.Add(new AlertaReporte
                    {
                        TipoAlerta = "PrestamoVencido",
                        Descripcion = $"Préstamo #{prestamo.NumeroPrestamo} vencido hace {diasRetraso} días",
                        Severidad = diasRetraso > 7 ? "Crítica" : diasRetraso > 3 ? "Alta" : "Media",
                        IdRegistroRelacionado = prestamo.IdPrestamo,
                        TipoRegistroRelacionado = "Prestamo",
                        RequiereAccion = true
                    });
                }
            }

            _context.SaveChanges();
        }

        public List<AlertaReporte> ObtenerAlertasActivas()
        {
            return _context.AlertaReporte
                .Include(a => a.ApplicationUser)
                .Where(a => !a.Leida)
                .OrderByDescending(a => a.FechaGeneracion)
                .ToList();
        }

        #endregion

        #region Historial de Aprobaciones

        public object ObtenerHistorialAprobaciones(DateTime? fechaInicio, DateTime? fechaFin, string accion = null)
        {
            var query = _context.HistorialAprobacionPrestamo
                .Include(h => h.Prestamo)
                .Include(h => h.UsuarioSolicitante)
                .Include(h => h.UsuarioRevisor)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaRevision >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(h => h.FechaRevision <= fechaFin.Value);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(h => h.Accion == accion);

            return query.Select(h => new
            {
                h.IdHistorial,
                NumeroPrestamo = h.Prestamo.NumeroPrestamo,
                Solicitante = h.UsuarioSolicitante.UserName,
                Revisor = h.UsuarioRevisor.UserName,
                h.EstadoPrevio,
                h.EstadoNuevo,
                h.Accion,
                h.MotivoRechazo,
                h.ComentariosRevisor,
                h.FechaRevision,
                h.DuracionRevision,
                h.Prioridad
            }).ToList();
        }

        public EstadisticasAprobacionesViewModel ObtenerEstadisticasAprobaciones(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.HistorialAprobacionPrestamo.AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaRevision >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(h => h.FechaRevision <= fechaFin.Value);

            var total = query.Count();
            var aprobados = query.Count(h => h.Accion == "Aprobado");
            var rechazados = query.Count(h => h.Accion == "Rechazado");
            var pendientes = query.Count(h => h.Accion == "Pendiente");
            var tiempoPromedioRevision = query.Where(h => h.DuracionRevision.HasValue)
                                              .Average(h => h.DuracionRevision);

            return new EstadisticasAprobacionesViewModel
            {
                Total = total,
                Aprobados = aprobados,
                Rechazados = rechazados,
                Pendientes = pendientes,
                TasaAprobacion = total > 0 ? (aprobados * 100.0 / total) : 0,
                TasaRechazo = total > 0 ? (rechazados * 100.0 / total) : 0,
                TiempoPromedioRevision = tiempoPromedioRevision.HasValue ? (int?)tiempoPromedioRevision.Value : null
            };
        }

        #endregion

        #region Gráficos Multidimensionales

        public object ObtenerDatosGraficoActividadUsuarios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var usuarios = _context.Users.AsNoTracking().ToList();
            var datos = new List<object>();

            foreach (var usuario in usuarios)
            {
                var prestamoQuery = _context.Prestamo.Where(p => p.Id == usuario.Id);
                var sancionQuery = _context.Sancion.Where(s => s.Id == usuario.Id);

                if (fechaInicio.HasValue)
                {
                    prestamoQuery = prestamoQuery.Where(p => p.FechadeCreacion >= fechaInicio.Value);
                    sancionQuery = sancionQuery.Where(s => s.FechaInicio >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    prestamoQuery = prestamoQuery.Where(p => p.FechadeCreacion <= fechaFin.Value);
                    sancionQuery = sancionQuery.Where(s => s.FechaInicio <= fechaFin.Value);
                }

                var totalPrestamos = prestamoQuery.Count();
                var totalSanciones = sancionQuery.Count();
                var documentosSubidos = _context.Documento.Count(d => d.Id == usuario.Id);

                datos.Add(new
                {
                    Usuario = usuario.UserName,
                    NombreCompleto = usuario.UserName,
                    TotalPrestamos = totalPrestamos,
                    TotalSanciones = totalSanciones,
                    DocumentosSubidos = documentosSubidos,
                    Score = totalPrestamos * 1 + documentosSubidos * 2 - totalSanciones * 5
                });
            }

            return datos;
        }

        public object ObtenerDatosHeatmapActividad(DateTime fechaInicio, DateTime fechaFin)
        {
            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicio && p.FechadeCreacion <= fechaFin)
                .AsNoTracking()
                .ToList();

            var heatmapData = prestamos
                .GroupBy(p => new { Fecha = p.FechadeCreacion.Date, Hora = p.FechadeCreacion.Hour })
                .Select(g => new
                {
                    Fecha = g.Key.Fecha.ToString("yyyy-MM-dd"),
                    Hora = g.Key.Hora,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.Hora)
                .ToList();

            return heatmapData;
        }

        #endregion
    }
}

