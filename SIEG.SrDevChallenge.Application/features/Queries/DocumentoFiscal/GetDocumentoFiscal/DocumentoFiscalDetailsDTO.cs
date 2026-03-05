using System;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentoFiscal;

public class DocumentoFiscalDetailsDTO
{
    public string ChaveAcesso { get; set; }
    public DateTime Data { get; set; }
    public TipoPessoaFiscal TipoEmissor { get; set; }
    public string DocumentoEmissor { get; set; }

    public TipoPessoaFiscal TipoDestinatario { get; set; }
    public string DocumentoDestinatario { get; set; }

    public TipoDocumentoFiscal TipoDocumento { get; set; }

    public decimal ValorTotal { get; set; } = 0;

    public DateTime CriadoEm { get;  set; }

    public DateTime AtualizadoEm { get; set; }

    public string XMLOriginal { get; set; }

    public string HashXml { get; set; }
}
