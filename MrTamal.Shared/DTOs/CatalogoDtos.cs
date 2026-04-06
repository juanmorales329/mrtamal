using MrTamal.Shared.Models;

namespace MrTamal.Shared.DTOs;

public record CatalogoDto(int Id, string Codigo, string Descripcion, TipoCatalogo Tipo, bool Activo);

public record CreateCatalogoRequest(string Codigo, string Descripcion, TipoCatalogo Tipo);

public record UpdateCatalogoRequest(string Codigo, string Descripcion, bool Activo);
