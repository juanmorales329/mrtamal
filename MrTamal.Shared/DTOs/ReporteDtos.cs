namespace MrTamal.Shared.DTOs;

public record ReporteRequest(
    TipoReporte Tipo,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    int? Anio,
    int? Mes,
    int? AnioComparacion
);

public enum TipoReporte
{
    Diario,
    Semanal,
    Mensual,
    ComparacionAnual,
    ComparacionMensual
}

public record ReporteResumen(
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal Balance,
    List<ReporteDetalle> Ingresos,
    List<ReporteDetalle> Egresos,
    List<ReporteComparacion>? Comparaciones
);

public record ReporteDetalle(
    DateTime Fecha,
    string Codigo,
    string Descripcion,
    decimal Cantidad,
    string? Notas
);

public record ReporteComparacion(
    string Periodo,
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal Balance
);
