namespace MrTamal.Shared.DTOs;

public record MovimientoDto(
    int Id,
    DateTime Fecha,
    string CodigoCatalogo,
    string DescripcionCatalogo,
    decimal Cantidad,
    string? Notas,
    string SimboloMoneda = "$"
);

public record CreateMovimientoRequest(
    DateTime Fecha,
    int CatalogoId,
    decimal Cantidad,
    string? Notas
);

public record UpdateMovimientoRequest(
    DateTime Fecha,
    int CatalogoId,
    decimal Cantidad,
    string? Notas
);
