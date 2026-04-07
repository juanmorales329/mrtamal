namespace MrTamal.Shared.DTOs;

public record MetaAnualDto(
    int Id, int Anio, decimal MetaVentas, bool ExcluirDomingos,
    int? SucursalId, string? SucursalNombre
);

public record CreateMetaAnualRequest(int Anio, decimal MetaVentas, bool ExcluirDomingos, int? SucursalId);

public record DiaNolaboralDto(int Id, DateTime Fecha, string? Descripcion);
public record CreateDiaNolaboralRequest(DateTime Fecha, string? Descripcion, int? SucursalId);

public record ResumenProyectado(
    int Anio,
    decimal MetaAnual,
    int DiasLaborables,
    decimal VentaReal,
    decimal PorcentajeCumplimiento,
    // Metas por período
    decimal MetaDiaria,
    decimal MetaSemanal,
    decimal MetaMensual,
    decimal MetaBimestral,
    decimal MetaTrimestral,
    decimal MetaCuatrimestral,
    decimal MetaSemestral,
    // Desglose mensual
    List<DesgloseMes> DesgloseMensual,
    // Días no laborables
    List<DiaNolaboralDto> DiasNoLaborables,
    // Cuatrimestres fijos (100k cada uno)
    List<ResumenCuatrimestre> Cuatrimestres
);

public record ResumenCuatrimestre(
    int Numero,
    string Periodo,
    decimal Meta,
    decimal VentaReal,
    decimal Diferencia,
    decimal PorcentajeCumplimiento
);

public record DesgloseMes(
    int Mes,
    string NombreMes,
    int DiasLaborables,
    decimal MetaMes,
    decimal VentaReal,
    decimal Diferencia,
    bool Cumplido
);
