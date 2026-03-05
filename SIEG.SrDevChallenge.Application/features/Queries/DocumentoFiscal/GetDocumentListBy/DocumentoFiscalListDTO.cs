using System;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentListBy;

public record DocumentoFiscalListDTO
{
    public Guid Id { get; init; }
    public string ChaveAcesso { get; init; } = string.Empty;
    public string DocumentoEmissor { get; init; } = string.Empty;
    public string DocumentoDestinatario { get; init; } = string.Empty;
    public DateTime DataEmissao { get; init; }
    public decimal ValorTotal { get; init; }
    public TipoDocumentoFiscal TipoDocumento { get; init; }
}
