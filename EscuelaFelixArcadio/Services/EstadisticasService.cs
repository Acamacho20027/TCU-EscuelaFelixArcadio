using EscuelaFelixArcadio.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Services
{
    public class EstadisticasService
    {
        private readonly ApplicationDbContext _context;

        public EstadisticasService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Dashboard Principal - Indicadores Clave

        public object ObtenerDatosDashboard()
        {
            var fechaActual = DateTime.Now;
            var fechaInicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            var fechaInicioAno = new DateTime(fechaActual.Year, 1, 1);

            // Préstamos activos
            var prestamosActivos = _context.Prestamo
                .Where(p => !p.Devolucion && p.Estado.Descripcion == "Aprobado")
                .Count();

            // Espacios ocupados actualmente
            var espaciosOcupados = _context.ReservaEspacio
                .Where(r => r.FechaInicio <= fechaActual && r.FechaFin >= fechaActual && r.Estado.Descripcion == "Aprobado")
                .Count();

            // Tasa de devolución del mes
            var prestamosMes = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicioMes)
                .ToList();

            var tasaDevolucion = prestamosMes.Count > 0 
                ? (prestamosMes.Count(p => p.Devolucion) * 100.0 / prestamosMes.Count) 
                : 0;

            // Usuario más activo del mes
            var usuarioMasActivo = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicioMes)
                .GroupBy(p => p.Id)
                .Select(g => new { Usuario = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .FirstOrDefault();

            // Productos más solicitados
            var productosMasSolicitados = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicioMes)
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new { Motivo = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            // Espacios más utilizados
            var espaciosMasUtilizados = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= fechaInicioMes)
                .Include(r => r.Espacio)
                .GroupBy(r => r.IdEspacio)
                .Select(g => new { 
                    IdEspacio = g.Key, 
                    NombreEspacio = g.First().Espacio.Nombre,
                    Cantidad = g.Count() 
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            return new
            {
                PrestamosActivos = prestamosActivos,
                EspaciosOcupados = espaciosOcupados,
                TasaDevolucion = Math.Round(tasaDevolucion, 2),
                UsuarioMasActivo = usuarioMasActivo?.Usuario ?? "N/A",
                CantidadUsuarioMasActivo = usuarioMasActivo?.Cantidad ?? 0,
                ProductosMasSolicitados = productosMasSolicitados,
                EspaciosMasUtilizados = espaciosMasUtilizados,
                FechaActualizacion = fechaActual
            };
        }

        public object ObtenerKPIs()
        {
            var fechaActual = DateTime.Now;
            var fechaInicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            var fechaFinMes = fechaInicioMes.AddMonths(1).AddDays(-1);
            var fechaInicioMesAnterior = fechaInicioMes.AddMonths(-1);
            var fechaFinMesAnterior = fechaInicioMes.AddDays(-1);

            // KPI 1: Total de préstamos del mes
            var prestamosMesActual = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicioMes && p.FechadeCreacion <= fechaFinMes)
                .Count();

            var prestamosMesAnterior = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicioMesAnterior && p.FechadeCreacion <= fechaFinMesAnterior)
                .Count();

            var variacionPrestamos = prestamosMesAnterior > 0 
                ? ((prestamosMesActual - prestamosMesAnterior) * 100.0 / prestamosMesAnterior)
                : 0;

            // KPI 2: Tiempo promedio de préstamo
            var prestamosCompletados = _context.Prestamo
                .Where(p => p.Devolucion && p.FechaDevolucion.HasValue)
                .ToList();

            var tiempoPromedioPrestamo = prestamosCompletados.Count > 0
                ? prestamosCompletados.Average(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays)
                : 0;

            // KPI 3: Tasa de ocupación de espacios
            var totalEspacios = _context.Espacio.Count();
            var reservasMesActual = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= fechaInicioMes && r.FechaCreacion <= fechaFinMes)
                .Count();

            var tasaOcupacionEspacios = totalEspacios > 0 
                ? (reservasMesActual * 100.0 / totalEspacios)
                : 0;

            // KPI 4: Eficiencia de devolución
            var prestamosVencidos = _context.Prestamo
                .Where(p => !p.Devolucion && p.FechaVencimiento.HasValue && p.FechaVencimiento.Value < fechaActual)
                .Count();

            var eficienciaDevolucion = prestamosMesActual > 0 
                ? ((prestamosMesActual - prestamosVencidos) * 100.0 / prestamosMesActual)
                : 0;

            return new
            {
                PrestamosMesActual = prestamosMesActual,
                VariacionPrestamos = Math.Round(variacionPrestamos, 2),
                TiempoPromedioPrestamo = Math.Round(tiempoPromedioPrestamo, 1),
                TasaOcupacionEspacios = Math.Round(tasaOcupacionEspacios, 2),
                EficienciaDevolucion = Math.Round(eficienciaDevolucion, 2),
                PrestamosVencidos = prestamosVencidos
            };
        }

        #endregion

        #region Análisis de Tendencias de Préstamos

        public object ObtenerTendenciasPrestamos(DateTime? fechaInicio, DateTime? fechaFin, string tipoAnalisis)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                .AsNoTracking()
                .ToList();

            object tendencias;

            switch (tipoAnalisis.ToLower())
            {
                case "diario":
                    tendencias = prestamos
                        .GroupBy(p => p.FechadeCreacion.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("yyyy-MM-dd"),
                            Cantidad = g.Count(),
                            Devueltos = g.Count(p => p.Devolucion),
                            Pendientes = g.Count(p => !p.Devolucion)
                        })
                        .OrderBy(x => x.Fecha)
                        .ToList();
                    break;

                case "semanal":
                    tendencias = prestamos
                        .GroupBy(p => new { 
                            Año = p.FechadeCreacion.Year, 
                            Semana = GetWeekOfYear(p.FechadeCreacion) 
                        })
                        .Select(g => new
                        {
                            Periodo = $"Semana {g.Key.Semana} - {g.Key.Año}",
                            Cantidad = g.Count(),
                            Devueltos = g.Count(p => p.Devolucion),
                            Pendientes = g.Count(p => !p.Devolucion)
                        })
                        .OrderBy(x => x.Periodo)
                        .ToList();
                    break;

                case "mensual":
                    tendencias = prestamos
                        .GroupBy(p => new { p.FechadeCreacion.Year, p.FechadeCreacion.Month })
                        .Select(g => new
                        {
                            Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Cantidad = g.Count(),
                            Devueltos = g.Count(p => p.Devolucion),
                            Pendientes = g.Count(p => !p.Devolucion)
                        })
                        .OrderBy(x => x.Periodo)
                        .ToList();
                    break;

                default:
                    tendencias = prestamos
                        .GroupBy(p => p.FechadeCreacion.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("yyyy-MM-dd"),
                            Cantidad = g.Count(),
                            Devueltos = g.Count(p => p.Devolucion),
                            Pendientes = g.Count(p => !p.Devolucion)
                        })
                        .OrderBy(x => x.Fecha)
                        .ToList();
                    break;
            }

            return tendencias;
        }

        public object ObtenerPatronesPrestamos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-90);
            var fin = fechaFin ?? DateTime.Now;

            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                .AsNoTracking()
                .ToList();

            // Patrón por día de la semana
            var patronDiaSemana = prestamos
                .GroupBy(p => p.FechadeCreacion.DayOfWeek)
                .Select(g => new
                {
                    DiaSemana = g.Key.ToString(),
                    Cantidad = g.Count(),
                    Porcentaje = Math.Round(g.Count() * 100.0 / prestamos.Count, 2)
                })
                .OrderBy(x => x.DiaSemana)
                .ToList();

            // Patrón por hora del día
            var patronHora = prestamos
                .GroupBy(p => p.FechadeCreacion.Hour)
                .Select(g => new
                {
                    Hora = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = Math.Round(g.Count() * 100.0 / prestamos.Count, 2)
                })
                .OrderBy(x => x.Hora)
                .ToList();

            // Patrón por motivo de préstamo
            var patronMotivo = prestamos
                .Where(p => !string.IsNullOrEmpty(p.MotivoPrestamo))
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = Math.Round(g.Count() * 100.0 / prestamos.Count, 2)
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Duración promedio por motivo
            var duracionPorMotivo = prestamos
                .Where(p => p.Devolucion && p.FechaDevolucion.HasValue && !string.IsNullOrEmpty(p.MotivoPrestamo))
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new
                {
                    Motivo = g.Key,
                    DuracionPromedio = Math.Round(g.Average(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays), 1)
                })
                .OrderByDescending(x => x.DuracionPromedio)
                .ToList();

            return new
            {
                PatronDiaSemana = patronDiaSemana,
                PatronHora = patronHora,
                PatronMotivo = patronMotivo,
                DuracionPorMotivo = duracionPorMotivo,
                TotalPrestamos = prestamos.Count
            };
        }

        public object GenerarPrediccionesPrestamos(int diasPrediccion)
        {
            var fechaActual = DateTime.Now;
            var fechaInicio = fechaActual.AddDays(-90); // Usar últimos 90 días para predicción

            var prestamosHistoricos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= fechaInicio && p.FechadeCreacion <= fechaActual)
                .AsNoTracking()
                .ToList();

            if (prestamosHistoricos.Count < 10)
            {
                return new { Error = "Datos insuficientes para generar predicciones" };
            }

            // Calcular promedio diario
            var diasHistoricos = (fechaActual - fechaInicio).Days;
            var promedioDiario = prestamosHistoricos.Count / (double)diasHistoricos;

            // Calcular tendencia (regresión lineal simple)
            var tendencia = CalcularTendenciaLineal(prestamosHistoricos);

            // Generar predicciones
            var predicciones = new List<object>();
            for (int i = 1; i <= diasPrediccion; i++)
            {
                var fechaPrediccion = fechaActual.AddDays(i);
                var prediccion = promedioDiario + (tendencia * i);
                
                predicciones.Add(new
                {
                    Fecha = fechaPrediccion.ToString("yyyy-MM-dd"),
                    Prediccion = Math.Max(0, Math.Round(prediccion, 0)),
                    DiaSemana = fechaPrediccion.DayOfWeek.ToString()
                });
            }

            return new
            {
                Predicciones = predicciones,
                PromedioDiario = Math.Round(promedioDiario, 2),
                Tendencia = Math.Round(tendencia, 4),
                Confianza = CalcularNivelConfianza(prestamosHistoricos)
            };
        }

        #endregion

        #region Análisis de Reservaciones de Espacios

        public object ObtenerTendenciasEspacios(DateTime? fechaInicio, DateTime? fechaFin, int? idEspacio)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var query = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                .Include(r => r.Espacio)
                .AsNoTracking();

            if (idEspacio.HasValue)
            {
                query = query.Where(r => r.IdEspacio == idEspacio.Value);
            }

            var reservas = query.ToList();

            var tendencias = reservas
                .GroupBy(r => r.FechaCreacion.Date)
                .Select(g => new
                {
                    Fecha = g.Key.ToString("yyyy-MM-dd"),
                    CantidadReservas = g.Count(),
                    DuracionPromedio = Math.Round(g.Average(r => (r.FechaFin - r.FechaInicio).TotalHours), 1),
                    EspaciosUtilizados = g.Select(r => r.IdEspacio).Distinct().Count()
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            return tendencias;
        }

        public object ObtenerOcupacionEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var espacios = _context.Espacio
                .Include(e => e.Estado)
                .AsNoTracking()
                .ToList();

            var ocupacion = espacios.Select(espacio =>
            {
                var reservasEspacio = _context.ReservaEspacio
                    .Where(r => r.IdEspacio == espacio.IdEspacio && 
                               r.FechaCreacion >= inicio && 
                               r.FechaCreacion <= fin)
                    .ToList();

                var totalHorasReservadas = reservasEspacio.Sum(r => (r.FechaFin - r.FechaInicio).TotalHours);
                var diasPeriodo = (fin - inicio).TotalDays;
                var horasDisponibles = diasPeriodo * 24; // Asumiendo 24 horas disponibles por día

                return new
                {
                    IdEspacio = espacio.IdEspacio,
                    NombreEspacio = espacio.Nombre,
                    Capacidad = espacio.Capacidad ?? 0,
                    TotalReservas = reservasEspacio.Count,
                    HorasReservadas = Math.Round(totalHorasReservadas, 1),
                    PorcentajeOcupacion = horasDisponibles > 0 
                        ? Math.Round(totalHorasReservadas * 100.0 / horasDisponibles, 2) 
                        : 0,
                    Estado = espacio.Estado.Descripcion
                };
            }).OrderByDescending(x => x.PorcentajeOcupacion).ToList();

            return ocupacion;
        }

        public object ObtenerHeatmapEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var reservas = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                .Include(r => r.Espacio)
                .AsNoTracking()
                .ToList();

            var heatmapData = reservas
                .GroupBy(r => new { 
                    Espacio = r.Espacio.Nombre, 
                    DiaSemana = r.FechaInicio.DayOfWeek, 
                    Hora = r.FechaInicio.Hour 
                })
                .Select(g => new
                {
                    Espacio = g.Key.Espacio,
                    DiaSemana = g.Key.DiaSemana.ToString(),
                    Hora = g.Key.Hora,
                    Cantidad = g.Count(),
                    Intensidad = CalcularIntensidad(g.Count(), reservas.Count)
                })
                .OrderBy(x => x.Espacio)
                .ThenBy(x => x.DiaSemana)
                .ThenBy(x => x.Hora)
                .ToList();

            return heatmapData;
        }

        public object ObtenerEficienciaEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var espacios = _context.Espacio.AsNoTracking().ToList();

            var eficiencia = espacios.Select(espacio =>
            {
                var reservasEspacio = _context.ReservaEspacio
                    .Where(r => r.IdEspacio == espacio.IdEspacio && 
                               r.FechaCreacion >= inicio && 
                               r.FechaCreacion <= fin)
                    .ToList();

                var totalUsuarios = reservasEspacio.Select(r => r.Id).Distinct().Count();
                var horasTotales = reservasEspacio.Sum(r => (r.FechaFin - r.FechaInicio).TotalHours);
                var reservasExitosas = reservasEspacio.Count(r => r.Estado.Descripcion == "Aprobado");
                var reservasCanceladas = reservasEspacio.Count(r => r.Estado.Descripcion == "Cancelado");

                return new
                {
                    IdEspacio = espacio.IdEspacio,
                    NombreEspacio = espacio.Nombre,
                    Capacidad = espacio.Capacidad ?? 0,
                    TotalReservas = reservasEspacio.Count,
                    ReservasExitosas = reservasExitosas,
                    ReservasCanceladas = reservasCanceladas,
                    TasaExito = reservasEspacio.Count > 0 
                        ? Math.Round(reservasExitosas * 100.0 / reservasEspacio.Count, 2) 
                        : 0,
                    HorasTotales = Math.Round(horasTotales, 1),
                    UsuariosUnicos = totalUsuarios,
                    Eficiencia = CalcularEficienciaEspacio(reservasEspacio.Count, horasTotales, espacio.Capacidad ?? 1)
                };
            }).OrderByDescending(x => x.Eficiencia).ToList();

            return eficiencia;
        }

        #endregion

        #region Sistema de Recomendaciones

        public object GenerarRecomendacionesHorarios(int? idEspacio)
        {
            var fechaInicio = DateTime.Now.AddDays(-60);
            var reservas = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= fechaInicio)
                .Include(r => r.Espacio)
                .AsNoTracking();

            if (idEspacio.HasValue)
            {
                reservas = reservas.Where(r => r.IdEspacio == idEspacio.Value);
            }

            var reservasList = reservas.ToList();

            // Análisis de horarios más demandados
            var horariosDemandados = reservasList
                .GroupBy(r => new { r.FechaInicio.DayOfWeek, HoraInicio = r.FechaInicio.Hour })
                .Select(g => new
                {
                    DiaSemana = g.Key.DayOfWeek.ToString(),
                    HoraInicio = g.Key.HoraInicio,
                    Cantidad = g.Count(),
                    TasaExito = g.Count(r => r.Estado.Descripcion == "Aprobado") * 100.0 / g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToList();

            // Horarios con menor demanda
            var horariosMenosDemandados = reservasList
                .GroupBy(r => new { r.FechaInicio.DayOfWeek, HoraInicio = r.FechaInicio.Hour })
                .Select(g => new
                {
                    DiaSemana = g.Key.DayOfWeek.ToString(),
                    HoraInicio = g.Key.HoraInicio,
                    Cantidad = g.Count(),
                    TasaExito = g.Count(r => r.Estado.Descripcion == "Aprobado") * 100.0 / g.Count()
                })
                .OrderBy(x => x.Cantidad)
                .Take(10)
                .ToList();

            // Recomendaciones basadas en patrones
            var recomendaciones = new List<object>();

            foreach (var horario in horariosMenosDemandados)
            {
                if (horario.TasaExito > 80) // Alta tasa de éxito pero baja demanda
                {
                    recomendaciones.Add(new
                    {
                        Tipo = "Horario Recomendado",
                        Descripcion = $"El {horario.DiaSemana} a las {horario.HoraInicio}:00 tiene alta disponibilidad y éxito",
                        Prioridad = "Alta",
                        Beneficio = "Mayor probabilidad de aprobación"
                    });
                }
            }

            return new
            {
                HorariosDemandados = horariosDemandados,
                HorariosMenosDemandados = horariosMenosDemandados,
                Recomendaciones = recomendaciones
            };
        }

        public object GenerarRecomendacionesEquipamiento()
        {
            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= DateTime.Now.AddDays(-90))
                .AsNoTracking()
                .ToList();

            // Análisis de motivos más frecuentes
            var motivosFrecuentes = prestamos
                .Where(p => !string.IsNullOrEmpty(p.MotivoPrestamo))
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    TasaDevolucion = g.Count(p => p.Devolucion) * 100.0 / g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Análisis de tiempos de préstamo por motivo
            var tiemposPorMotivo = prestamos
                .Where(p => p.Devolucion && p.FechaDevolucion.HasValue && !string.IsNullOrEmpty(p.MotivoPrestamo))
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new
                {
                    Motivo = g.Key,
                    TiempoPromedio = g.Average(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.TiempoPromedio)
                .ToList();

            var recomendaciones = new List<object>();

            // Recomendación de equipamiento adicional
            var motivoMasDemandado = motivosFrecuentes.FirstOrDefault();
            if (motivoMasDemandado != null)
            {
                recomendaciones.Add(new
                {
                    Tipo = "Equipamiento Adicional",
                    Descripcion = $"Considerar adquirir más equipamiento para '{motivoMasDemandado.Motivo}' - {motivoMasDemandado.Cantidad} solicitudes",
                    Prioridad = "Alta",
                    Beneficio = "Reducir tiempos de espera"
                });
            }

            // Recomendación de mantenimiento preventivo
            var motivoTiempoLargo = tiemposPorMotivo.FirstOrDefault();
            if (motivoTiempoLargo != null && motivoTiempoLargo.TiempoPromedio > 7)
            {
                recomendaciones.Add(new
                {
                    Tipo = "Mantenimiento Preventivo",
                    Descripcion = $"Equipamiento para '{motivoTiempoLargo.Motivo}' requiere mantenimiento frecuente",
                    Prioridad = "Media",
                    Beneficio = "Prolongar vida útil del equipamiento"
                });
            }

            return new
            {
                MotivosFrecuentes = motivosFrecuentes,
                TiemposPorMotivo = tiemposPorMotivo,
                Recomendaciones = recomendaciones
            };
        }

        public object GenerarRecomendacionesMantenimiento()
        {
            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= DateTime.Now.AddDays(-180))
                .AsNoTracking()
                .ToList();

            // Análisis de equipamiento más utilizado
            var equipamientoUtilizado = prestamos
                .Where(p => !string.IsNullOrEmpty(p.MotivoPrestamo))
                .GroupBy(p => p.MotivoPrestamo)
                .Select(g => new
                {
                    Equipamiento = g.Key,
                    CantidadUsos = g.Count(),
                    TiempoTotalUso = g.Where(p => p.Devolucion && p.FechaDevolucion.HasValue)
                                     .Sum(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays),
                    TasaDesgaste = CalcularTasaDesgaste(g.Count(), g.Average(p => p.Devolucion ? (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays : 0))
                })
                .OrderByDescending(x => x.TasaDesgaste)
                .ToList();

            var recomendaciones = new List<object>();

            foreach (var item in equipamientoUtilizado.Take(5))
            {
                if (item.TasaDesgaste > 0.7) // Alta tasa de desgaste
                {
                    recomendaciones.Add(new
                    {
                        Tipo = "Mantenimiento Urgente",
                        Equipamiento = item.Equipamiento,
                        Descripcion = $"Requiere mantenimiento inmediato - Tasa de desgaste: {Math.Round(item.TasaDesgaste * 100, 1)}%",
                        Prioridad = "Crítica",
                        AccionRecomendada = "Inspección técnica y reparación"
                    });
                }
                else if (item.TasaDesgaste > 0.4) // Media tasa de desgaste
                {
                    recomendaciones.Add(new
                    {
                        Tipo = "Mantenimiento Programado",
                        Equipamiento = item.Equipamiento,
                        Descripcion = $"Programar mantenimiento preventivo - Tasa de desgaste: {Math.Round(item.TasaDesgaste * 100, 1)}%",
                        Prioridad = "Media",
                        AccionRecomendada = "Mantenimiento preventivo en 30 días"
                    });
                }
            }

            return new
            {
                EquipamientoUtilizado = equipamientoUtilizado,
                Recomendaciones = recomendaciones
            };
        }

        #endregion

        #region Visualizaciones Interactivas

        public object ObtenerDatosGraficoPrestamos(DateTime? fechaInicio, DateTime? fechaFin, string tipoGrafico)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var prestamos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                .AsNoTracking()
                .ToList();

            switch (tipoGrafico.ToLower())
            {
                case "linea":
                    return prestamos
                        .GroupBy(p => p.FechadeCreacion.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("MM/dd"),
                            Cantidad = g.Count()
                        })
                        .OrderBy(x => x.Fecha)
                        .ToList();

                case "barras":
                    return prestamos
                        .GroupBy(p => p.Estado.Descripcion)
                        .Select(g => new
                        {
                            Estado = g.Key,
                            Cantidad = g.Count()
                        })
                        .ToList();

                case "circular":
                    return prestamos
                        .Where(p => !string.IsNullOrEmpty(p.MotivoPrestamo))
                        .GroupBy(p => p.MotivoPrestamo)
                        .Select(g => new
                        {
                            Motivo = g.Key,
                            Cantidad = g.Count()
                        })
                        .OrderByDescending(x => x.Cantidad)
                        .Take(5)
                        .ToList();

                default:
                    return prestamos
                        .GroupBy(p => p.FechadeCreacion.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("MM/dd"),
                            Cantidad = g.Count()
                        })
                        .OrderBy(x => x.Fecha)
                        .ToList();
            }
        }

        public object ObtenerDatosGraficoEspacios(DateTime? fechaInicio, DateTime? fechaFin, string tipoGrafico)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var reservas = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                .Include(r => r.Espacio)
                .AsNoTracking()
                .ToList();

            switch (tipoGrafico.ToLower())
            {
                case "barras":
                    return reservas
                        .GroupBy(r => r.Espacio.Nombre)
                        .Select(g => new
                        {
                            Espacio = g.Key,
                            Cantidad = g.Count()
                        })
                        .OrderByDescending(x => x.Cantidad)
                        .ToList();

                case "linea":
                    return reservas
                        .GroupBy(r => r.FechaCreacion.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("MM/dd"),
                            Cantidad = g.Count()
                        })
                        .OrderBy(x => x.Fecha)
                        .ToList();

                case "heatmap":
                    return reservas
                        .GroupBy(r => new { r.FechaInicio.DayOfWeek, Hora = r.FechaInicio.Hour })
                        .Select(g => new
                        {
                            DiaSemana = g.Key.DayOfWeek.ToString(),
                            Hora = g.Key.Hora,
                            Cantidad = g.Count()
                        })
                        .ToList();

                default:
                    return reservas
                        .GroupBy(r => r.Espacio.Nombre)
                        .Select(g => new
                        {
                            Espacio = g.Key,
                            Cantidad = g.Count()
                        })
                        .OrderByDescending(x => x.Cantidad)
                        .ToList();
            }
        }

        public object ObtenerDatosGraficoUsuarios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var usuarios = _context.Users.AsNoTracking().ToList();

            var datosUsuarios = usuarios.Select(usuario =>
            {
                var prestamos = _context.Prestamo
                    .Where(p => p.Id == usuario.Id && p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                    .Count();

                var reservas = _context.ReservaEspacio
                    .Where(r => r.Id == usuario.Id && r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                    .Count();

                var sanciones = _context.Sancion
                    .Where(s => s.Id == usuario.Id && s.FechaInicio >= inicio && s.FechaInicio <= fin)
                    .Count();

                return new
                {
                    Usuario = usuario.UserName,
                    Prestamos = prestamos,
                    Reservas = reservas,
                    Sanciones = sanciones,
                    ScoreActividad = prestamos + reservas - (sanciones * 2)
                };
            }).OrderByDescending(x => x.ScoreActividad).Take(10).ToList();

            return datosUsuarios;
        }

        public object ObtenerDatosGraficoInventario()
        {
            var inventario = _context.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .AsNoTracking()
                .ToList();

            var datosInventario = new
            {
                PorCategoria = inventario
                    .GroupBy(i => i.Producto.Categoria != null ? i.Producto.Categoria.Nombre : "Sin categoría")
                    .Select(g => new
                    {
                        Categoria = g.Key,
                        Cantidad = g.Sum(x => x.Cantidad),
                        Productos = g.Count()
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList(),

                PorEstado = inventario
                    .GroupBy(i => i.Estado.Descripcion)
                    .Select(g => new
                    {
                        Estado = g.Key,
                        Cantidad = g.Sum(x => x.Cantidad)
                    })
                    .ToList(),

                StockBajo = inventario
                    .Where(i => i.Cantidad <= i.Minimo)
                    .Select(i => new
                    {
                        Producto = i.Producto.Nombre,
                        Cantidad = i.Cantidad,
                        Minimo = i.Minimo,
                        Diferencia = i.Cantidad - i.Minimo
                    })
                    .OrderBy(x => x.Diferencia)
                    .ToList()
            };

            return datosInventario;
        }

        #endregion

        #region Análisis Comparativo

        public object CompararPeriodos(string tipoAnalisis, DateTime periodo1Inicio, DateTime periodo1Fin, DateTime periodo2Inicio, DateTime periodo2Fin)
        {
            switch (tipoAnalisis.ToLower())
            {
                case "prestamos":
                    return CompararPrestamosPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);

                case "espacios":
                    return CompararEspaciosPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);

                case "usuarios":
                    return CompararUsuariosPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);

                case "inventario":
                    return CompararInventarioPeriodos(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);

                default:
                    return new { Error = "Tipo de análisis no válido" };
            }
        }

        private object CompararPrestamosPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var periodo1 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p1Inicio && p.FechadeCreacion <= p1Fin)
                .AsNoTracking()
                .ToList();

            var periodo2 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p2Inicio && p.FechadeCreacion <= p2Fin)
                .AsNoTracking()
                .ToList();

            var totalP1 = periodo1.Count;
            var totalP2 = periodo2.Count;
            var devueltosP1 = periodo1.Count(p => p.Devolucion);
            var devueltosP2 = periodo2.Count(p => p.Devolucion);
            var prestamosDevueltosP1 = periodo1.Where(p => p.Devolucion && p.FechaDevolucion.HasValue).ToList();
            var prestamosDevueltosP2 = periodo2.Where(p => p.Devolucion && p.FechaDevolucion.HasValue).ToList();
            
            var tiempoPromedioP1 = prestamosDevueltosP1.Count > 0 
                ? prestamosDevueltosP1.Average(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays)
                : 0.0;
            var tiempoPromedioP2 = prestamosDevueltosP2.Count > 0
                ? prestamosDevueltosP2.Average(p => (p.FechaDevolucion.Value - p.FechadeCreacion).TotalDays)
                : 0.0;

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio.ToString("yyyy-MM-dd"),
                    Fin = p1Fin.ToString("yyyy-MM-dd"),
                    TotalPrestamos = totalP1,
                    Devueltos = devueltosP1,
                    Pendientes = totalP1 - devueltosP1,
                    TasaDevolucion = totalP1 > 0 ? (double)Math.Round(devueltosP1 * 100.0 / totalP1, 2) : 0.0,
                    TiempoPromedio = (double)Math.Round(tiempoPromedioP1, 1)
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio.ToString("yyyy-MM-dd"),
                    Fin = p2Fin.ToString("yyyy-MM-dd"),
                    TotalPrestamos = totalP2,
                    Devueltos = devueltosP2,
                    Pendientes = totalP2 - devueltosP2,
                    TasaDevolucion = totalP2 > 0 ? (double)Math.Round(devueltosP2 * 100.0 / totalP2, 2) : 0.0,
                    TiempoPromedio = (double)Math.Round(tiempoPromedioP2, 1)
                },
                Diferencias = new
                {
                    TotalPrestamos = totalP2 - totalP1,
                    TotalPrestamosPorc = totalP1 > 0 ? (double)Math.Round((totalP2 - totalP1) * 100.0 / totalP1, 2) : 0.0,
                    Devueltos = devueltosP2 - devueltosP1,
                    DevueltosPorc = devueltosP1 > 0 ? (double)Math.Round((devueltosP2 - devueltosP1) * 100.0 / devueltosP1, 2) : 0.0,
                    TiempoPromedio = (double)Math.Round(tiempoPromedioP2 - tiempoPromedioP1, 1)
                }
            };
        }

        private object CompararEspaciosPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var periodo1 = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= p1Inicio && r.FechaCreacion <= p1Fin)
                .AsNoTracking()
                .ToList();

            var periodo2 = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= p2Inicio && r.FechaCreacion <= p2Fin)
                .AsNoTracking()
                .ToList();

            var totalR1 = periodo1.Count;
            var totalR2 = periodo2.Count;
            var exitosasR1 = periodo1.Count(r => r.Estado.Descripcion == "Aprobado");
            var exitosasR2 = periodo2.Count(r => r.Estado.Descripcion == "Aprobado");

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio.ToString("yyyy-MM-dd"),
                    Fin = p1Fin.ToString("yyyy-MM-dd"),
                    TotalReservas = totalR1,
                    Exitosas = exitosasR1,
                    TasaExito = totalR1 > 0 ? (double)Math.Round(exitosasR1 * 100.0 / totalR1, 2) : 0.0
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio.ToString("yyyy-MM-dd"),
                    Fin = p2Fin.ToString("yyyy-MM-dd"),
                    TotalReservas = totalR2,
                    Exitosas = exitosasR2,
                    TasaExito = totalR2 > 0 ? (double)Math.Round(exitosasR2 * 100.0 / totalR2, 2) : 0.0
                },
                Diferencias = new
                {
                    TotalReservas = totalR2 - totalR1,
                    TotalReservasPorc = totalR1 > 0 ? (double)Math.Round((totalR2 - totalR1) * 100.0 / totalR1, 2) : 0.0,
                    Exitosas = exitosasR2 - exitosasR1,
                    TasaExito = (double)Math.Round((totalR2 > 0 ? exitosasR2 * 100.0 / totalR2 : 0) - (totalR1 > 0 ? exitosasR1 * 100.0 / totalR1 : 0), 2)
                }
            };
        }

        private object CompararUsuariosPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var usuariosP1 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p1Inicio && p.FechadeCreacion <= p1Fin)
                .Select(p => p.Id)
                .Distinct()
                .Count();

            var usuariosP2 = _context.Prestamo
                .Where(p => p.FechadeCreacion >= p2Inicio && p.FechadeCreacion <= p2Fin)
                .Select(p => p.Id)
                .Distinct()
                .Count();

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio.ToString("yyyy-MM-dd"),
                    Fin = p1Fin.ToString("yyyy-MM-dd"),
                    UsuariosUnicos = usuariosP1
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio.ToString("yyyy-MM-dd"),
                    Fin = p2Fin.ToString("yyyy-MM-dd"),
                    UsuariosUnicos = usuariosP2
                },
                Diferencias = new
                {
                    UsuariosUnicos = usuariosP2 - usuariosP1,
                    UsuariosUnicosPorc = usuariosP1 > 0 ? (double)Math.Round((usuariosP2 - usuariosP1) * 100.0 / usuariosP1, 2) : 0.0
                }
            };
        }

        private object CompararInventarioPeriodos(DateTime p1Inicio, DateTime p1Fin, DateTime p2Inicio, DateTime p2Fin)
        {
            var inventarioP1 = _context.Inventario
                .Where(i => i.FechaActualizacion >= p1Inicio && i.FechaActualizacion <= p1Fin)
                .AsNoTracking()
                .ToList();

            var inventarioP2 = _context.Inventario
                .Where(i => i.FechaActualizacion >= p2Inicio && i.FechaActualizacion <= p2Fin)
                .AsNoTracking()
                .ToList();

            var totalItemsP1 = inventarioP1.Sum(i => i.Cantidad);
            var totalItemsP2 = inventarioP2.Sum(i => i.Cantidad);
            var alertasP1 = inventarioP1.Count(i => i.Cantidad <= i.Minimo);
            var alertasP2 = inventarioP2.Count(i => i.Cantidad <= i.Minimo);

            return new
            {
                Periodo1 = new
                {
                    Inicio = p1Inicio.ToString("yyyy-MM-dd"),
                    Fin = p1Fin.ToString("yyyy-MM-dd"),
                    TotalItems = totalItemsP1,
                    AlertasStockBajo = alertasP1
                },
                Periodo2 = new
                {
                    Inicio = p2Inicio.ToString("yyyy-MM-dd"),
                    Fin = p2Fin.ToString("yyyy-MM-dd"),
                    TotalItems = totalItemsP2,
                    AlertasStockBajo = alertasP2
                },
                Diferencias = new
                {
                    TotalItems = totalItemsP2 - totalItemsP1,
                    TotalItemsPorc = totalItemsP1 > 0 ? (double)Math.Round((totalItemsP2 - totalItemsP1) * 100.0 / totalItemsP1, 2) : 0.0,
                    Alertas = alertasP2 - alertasP1
                }
            };
        }

        public object ObtenerVariacionesTemporales(string tipoAnalisis, string periodo)
        {
            var fechaActual = DateTime.Now;
            var variaciones = new List<object>();
            int valorAnterior = 0;

            switch (periodo.ToLower())
            {
                case "mensual":
                    for (int i = 11; i >= 0; i--)
                    {
                        var fechaInicio = new DateTime(fechaActual.Year, fechaActual.Month, 1).AddMonths(-i);
                        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
                        
                        var valorActual = CalcularVariacionPeriodo(tipoAnalisis, fechaInicio, fechaFin);
                        
                        // Calcular variación porcentual respecto al período anterior
                        double porcentajeVariacion = 0;
                        string tendencia = "Estable";
                        
                        if (valorAnterior > 0 && i < 11)
                        {
                            porcentajeVariacion = (valorActual.Valor - valorAnterior) * 100.0 / valorAnterior;
                            tendencia = porcentajeVariacion > 5 ? "Creciente" : porcentajeVariacion < -5 ? "Decreciente" : "Estable";
                        }
                        
                        variaciones.Add(new
                        {
                            Periodo = fechaInicio.ToString("MM/yyyy"),
                            Valor = valorActual.Valor,
                            Variacion = Math.Round(porcentajeVariacion, 1),
                            Tendencia = tendencia
                        });
                        
                        valorAnterior = valorActual.Valor;
                    }
                    break;

                case "semanal":
                    for (int i = 11; i >= 0; i--)
                    {
                        var fechaInicio = fechaActual.AddDays(-7 * (i + 1));
                        var fechaFin = fechaActual.AddDays(-7 * i);
                        
                        var valorActual = CalcularVariacionPeriodo(tipoAnalisis, fechaInicio, fechaFin);
                        
                        // Calcular variación porcentual respecto al período anterior
                        double porcentajeVariacion = 0;
                        string tendencia = "Estable";
                        
                        if (valorAnterior > 0 && i < 11)
                        {
                            porcentajeVariacion = (valorActual.Valor - valorAnterior) * 100.0 / valorAnterior;
                            tendencia = porcentajeVariacion > 5 ? "Creciente" : porcentajeVariacion < -5 ? "Decreciente" : "Estable";
                        }
                        
                        variaciones.Add(new
                        {
                            Periodo = $"Sem {GetWeekOfYear(fechaInicio)}",
                            Valor = valorActual.Valor,
                            Variacion = Math.Round(porcentajeVariacion, 1),
                            Tendencia = tendencia
                        });
                        
                        valorAnterior = valorActual.Valor;
                    }
                    break;
            }

            return variaciones;
        }

        #endregion

        #region Sistema de Alertas Estadísticas

        public object ObtenerAlertasEstadisticas()
        {
            var alertas = new List<object>();
            var fechaActual = DateTime.Now;

            // Alerta de préstamos vencidos
            var prestamosVencidos = _context.Prestamo
                .Where(p => !p.Devolucion && p.FechaVencimiento.HasValue && p.FechaVencimiento.Value < fechaActual)
                .Count();

            if (prestamosVencidos > 0)
            {
                alertas.Add(new
                {
                    Tipo = "PrestamosVencidos",
                    Severidad = prestamosVencidos > 10 ? "Crítica" : "Alta",
                    Descripcion = $"{prestamosVencidos} préstamos vencidos",
                    AccionRequerida = "Contactar usuarios para devolución",
                    FechaGeneracion = fechaActual.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            // Alerta de stock bajo
            var stockBajo = _context.Inventario
                .Where(i => i.Cantidad <= i.Minimo)
                .Count();

            if (stockBajo > 0)
            {
                alertas.Add(new
                {
                    Tipo = "StockBajo",
                    Severidad = stockBajo > 5 ? "Alta" : "Media",
                    Descripcion = $"{stockBajo} productos con stock bajo",
                    AccionRequerida = "Reabastecer inventario",
                    FechaGeneracion = fechaActual.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            // Alerta de espacios sobreutilizados
            var espaciosSobreutilizados = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= fechaActual.AddDays(-7))
                .GroupBy(r => r.IdEspacio)
                .Where(g => g.Count() > 20) // Más de 20 reservas en la semana
                .Count();

            if (espaciosSobreutilizados > 0)
            {
                alertas.Add(new
                {
                    Tipo = "EspaciosSobreutilizados",
                    Severidad = "Media",
                    Descripcion = $"{espaciosSobreutilizados} espacios con alta demanda",
                    AccionRequerida = "Evaluar necesidad de espacios adicionales",
                    FechaGeneracion = fechaActual.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            return alertas.Count > 0 ? alertas : new List<object>();
        }

        public object DetectarAnomalias(string tipoAnalisis)
        {
            var anomalias = new List<object>();
            var fechaActual = DateTime.Now;
            var fechaInicio = fechaActual.AddDays(-30);

            switch (tipoAnalisis.ToLower())
            {
                case "prestamos":
                    var prestamos = _context.Prestamo
                        .Where(p => p.FechadeCreacion >= fechaInicio)
                        .AsNoTracking()
                        .ToList();

                    var promedioDiario = prestamos.Count / 30.0;
                    var desviacionEstandar = CalcularDesviacionEstandar(prestamos.GroupBy(p => p.FechadeCreacion.Date).Select(g => g.Count()));

                    var diasAnomalos = prestamos
                        .GroupBy(p => p.FechadeCreacion.Date)
                        .Where(g => Math.Abs(g.Count() - promedioDiario) > 2 * desviacionEstandar)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("yyyy-MM-dd"),
                            Cantidad = g.Count(),
                            Desviacion = Math.Round(Math.Abs(g.Count() - promedioDiario), 2),
                            Tipo = g.Count() > promedioDiario ? "Pico" : "Valle"
                        })
                        .ToList();

                    anomalias.AddRange(diasAnomalos);
                    break;

                case "espacios":
                    var reservas = _context.ReservaEspacio
                        .Where(r => r.FechaCreacion >= fechaInicio)
                        .AsNoTracking()
                        .ToList();

                    var promedioDiarioEspacios = reservas.Count / 30.0;
                    var desviacionEstandarEspacios = CalcularDesviacionEstandar(reservas.GroupBy(r => r.FechaCreacion.Date).Select(g => g.Count()));

                    var diasAnomalosEspacios = reservas
                        .GroupBy(r => r.FechaCreacion.Date)
                        .Where(g => Math.Abs(g.Count() - promedioDiarioEspacios) > 2 * desviacionEstandarEspacios)
                        .Select(g => new
                        {
                            Fecha = g.Key.ToString("yyyy-MM-dd"),
                            Cantidad = g.Count(),
                            Desviacion = Math.Round(Math.Abs(g.Count() - promedioDiarioEspacios), 2),
                            Tipo = g.Count() > promedioDiarioEspacios ? "Pico" : "Valle"
                        })
                        .ToList();

                    anomalias.AddRange(diasAnomalosEspacios);
                    break;
            }

            return anomalias;
        }

        public object DetectarPatronesInusuales(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            var patrones = new List<object>();

            // Patrón inusual: usuario con muchos préstamos en poco tiempo
            var usuariosActivos = _context.Prestamo
                .Where(p => p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                .GroupBy(p => p.Id)
                .Where(g => g.Count() > 10) // Más de 10 préstamos en el período
                .Select(g => new
                {
                    Usuario = g.Key,
                    CantidadPrestamos = g.Count(),
                    Patron = "Alta frecuencia de préstamos",
                    Severidad = g.Count() > 20 ? "Alta" : "Media"
                })
                .ToList();

            patrones.AddRange(usuariosActivos);

            // Patrón inusual: espacios con reservas muy largas
            var reservasLargas = _context.ReservaEspacio
                .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                .Where(r => (r.FechaFin - r.FechaInicio).TotalHours > 8) // Más de 8 horas
                .GroupBy(r => r.IdEspacio)
                .Select(g => new
                {
                    IdEspacio = g.Key,
                    CantidadReservasLargas = g.Count(),
                    Patron = "Reservas de larga duración",
                    Severidad = g.Count() > 5 ? "Alta" : "Media"
                })
                .ToList();

            patrones.AddRange(reservasLargas);

            return patrones;
        }

        public object ConfigurarAlerta(string tipoAlerta, decimal umbral, bool activo)
        {
            // En una implementación real, esto se guardaría en una tabla de configuración
            return new
            {
                TipoAlerta = tipoAlerta,
                Umbral = umbral,
                Activo = activo,
                FechaConfiguracion = DateTime.Now,
                Mensaje = $"Alerta {tipoAlerta} configurada con umbral {umbral}"
            };
        }

        #endregion

        #region Métodos de Filtrado y Búsqueda

        public object FiltrarDatos(string tipoDatos, DateTime? fechaInicio, DateTime? fechaFin, string filtros)
        {
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;

            switch (tipoDatos.ToLower())
            {
                case "prestamos":
                    var prestamos = _context.Prestamo
                        .Where(p => p.FechadeCreacion >= inicio && p.FechadeCreacion <= fin)
                        .AsNoTracking()
                        .ToList();

                    if (!string.IsNullOrEmpty(filtros))
                    {
                        var filtrosArray = filtros.Split(',');
                        prestamos = prestamos.Where(p => filtrosArray.Contains(p.Estado.Descripcion)).ToList();
                    }

                    return prestamos.Select(p => new
                    {
                        p.IdPrestamo,
                        p.NumeroPrestamo,
                        Usuario = p.ApplicationUser.UserName,
                        Estado = p.Estado.Descripcion,
                        p.FechadeCreacion,
                        p.FechaDevolucion,
                        p.Devolucion
                    }).ToList();

                case "espacios":
                    var reservas = _context.ReservaEspacio
                        .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion <= fin)
                        .Include(r => r.Espacio)
                        .AsNoTracking()
                        .ToList();

                    return reservas.Select(r => new
                    {
                        r.IdReservaEspacio,
                        Espacio = r.Espacio.Nombre,
                        Usuario = r.ApplicationUser.UserName,
                        Estado = r.Estado.Descripcion,
                        r.FechaInicio,
                        r.FechaFin,
                        r.FechaCreacion
                    }).ToList();

                default:
                    return new { Error = "Tipo de datos no válido" };
            }
        }

        public ArchivoExportacion ExportarDatos(string tipoDatos, DateTime? fechaInicio, DateTime? fechaFin, string formato)
        {
            var datos = FiltrarDatos(tipoDatos, fechaInicio, fechaFin, null);
            
            // En una implementación real, aquí se generaría el archivo
            return new ArchivoExportacion
            {
                NombreArchivo = $"{tipoDatos}_{DateTime.Now:yyyyMMdd_HHmmss}.{formato}",
                TipoMIME = formato == "excel" ? "application/vnd.ms-excel" : "text/csv",
                Contenido = new byte[0], // Placeholder
                Tamaño = 0
            };
        }

        #endregion

        #region Métodos de Utilidad

        public object ObtenerFiltrosDisponibles(string tipoDatos)
        {
            switch (tipoDatos.ToLower())
            {
                case "prestamos":
                    return new
                    {
                        Estados = _context.Estado.Select(e => e.Descripcion).ToList(),
                        Motivos = _context.Prestamo.Where(p => !string.IsNullOrEmpty(p.MotivoPrestamo))
                                                  .Select(p => p.MotivoPrestamo)
                                                  .Distinct()
                                                  .ToList()
                    };

                case "espacios":
                    return new
                    {
                        Estados = _context.Estado.Select(e => e.Descripcion).ToList(),
                        Espacios = _context.Espacio.Select(e => e.Nombre).ToList()
                    };

                case "inventario":
                    return new
                    {
                        Categorias = _context.Categoria.Select(c => c.Nombre).ToList(),
                        Estados = _context.Estado.Select(e => e.Descripcion).ToList()
                    };

                default:
                    return new { Error = "Tipo de datos no válido" };
            }
        }

        public object ValidarRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaActual = DateTime.Now;
            var fechaMinima = fechaActual.AddYears(-2); // Máximo 2 años hacia atrás

            var validacion = new
            {
                Valido = true,
                Errores = new List<string>()
            };

            if (fechaInicio > fechaFin)
            {
                validacion.Errores.Add("La fecha de inicio no puede ser mayor que la fecha de fin");
            }

            if (fechaInicio < fechaMinima)
            {
                validacion.Errores.Add("La fecha de inicio no puede ser anterior a 2 años");
            }

            if (fechaFin > fechaActual)
            {
                validacion.Errores.Add("La fecha de fin no puede ser futura");
            }

            if ((fechaFin - fechaInicio).TotalDays > 365)
            {
                validacion.Errores.Add("El rango de fechas no puede ser mayor a 1 año");
            }

            return new
            {
                Valido = validacion.Errores.Count == 0,
                Errores = validacion.Errores
            };
        }

        #endregion

        #region Métodos Auxiliares

        private int GetWeekOfYear(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private double CalcularTendenciaLineal(List<Prestamo> prestamos)
        {
            if (prestamos.Count < 2) return 0;

            var dias = prestamos.Select((p, i) => (double)i).ToArray();
            var cantidades = prestamos.GroupBy(p => p.FechadeCreacion.Date)
                                     .OrderBy(g => g.Key)
                                     .Select(g => (double)g.Count())
                                     .ToArray();

            if (cantidades.Length < 2) return 0;

            var n = cantidades.Length;
            var sumX = dias.Sum();
            var sumY = cantidades.Sum();
            var sumXY = dias.Zip(cantidades, (x, y) => x * y).Sum();
            var sumX2 = dias.Sum(x => x * x);

            var pendiente = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            return pendiente;
        }

        private double CalcularNivelConfianza(List<Prestamo> prestamos)
        {
            if (prestamos.Count < 10) return 0.5;

            var cantidades = prestamos.GroupBy(p => p.FechadeCreacion.Date)
                                     .Select(g => g.Count())
                                     .ToArray();

            var promedio = cantidades.Average();
            var varianza = cantidades.Sum(x => Math.Pow(x - promedio, 2)) / cantidades.Length;
            var desviacion = Math.Sqrt(varianza);

            // Nivel de confianza basado en la consistencia de los datos
            var coeficienteVariacion = desviacion / promedio;
            return Math.Max(0.1, Math.Min(0.9, 1 - coeficienteVariacion));
        }

        private double CalcularIntensidad(int cantidad, int total)
        {
            if (total == 0) return 0;
            return Math.Min(1.0, cantidad / (double)total * 10); // Escala de 0 a 1
        }

        private double CalcularEficienciaEspacio(int reservas, double horasTotales, int capacidad)
        {
            if (capacidad == 0 || reservas == 0) return 0;
            
            var horasDisponibles = reservas * 24; // Asumiendo 24 horas por reserva
            var eficiencia = horasTotales / horasDisponibles;
            return Math.Min(1.0, eficiencia);
        }

        private double CalcularTasaDesgaste(int cantidadUsos, double tiempoPromedio)
        {
            if (cantidadUsos == 0) return 0;
            
            // Tasa de desgaste basada en cantidad de usos y tiempo promedio
            var factorUso = Math.Min(1.0, cantidadUsos / 100.0);
            var factorTiempo = Math.Min(1.0, tiempoPromedio / 30.0);
            
            return (factorUso + factorTiempo) / 2.0;
        }

        private double CalcularDesviacionEstandar(IEnumerable<int> valores)
        {
            var valoresArray = valores.ToArray();
            if (valoresArray.Length < 2) return 0;

            var promedio = valoresArray.Average();
            var varianza = valoresArray.Sum(x => Math.Pow(x - promedio, 2)) / valoresArray.Length;
            return Math.Sqrt(varianza);
        }

        private VariacionPeriodo CalcularVariacionPeriodo(string tipoAnalisis, DateTime fechaInicio, DateTime fechaFin)
        {
            switch (tipoAnalisis.ToLower())
            {
                case "prestamos":
                    var prestamos = _context.Prestamo
                        .Where(p => p.FechadeCreacion >= fechaInicio && p.FechadeCreacion <= fechaFin)
                        .Count();
                    return new VariacionPeriodo { Valor = prestamos, Variacion = 0, Tendencia = "Estable" };

                case "espacios":
                    var reservas = _context.ReservaEspacio
                        .Where(r => r.FechaCreacion >= fechaInicio && r.FechaCreacion <= fechaFin)
                        .Count();
                    return new VariacionPeriodo { Valor = reservas, Variacion = 0, Tendencia = "Estable" };

                default:
                    return new VariacionPeriodo { Valor = 0, Variacion = 0, Tendencia = "Estable" };
            }
        }

        #endregion
    }
}
