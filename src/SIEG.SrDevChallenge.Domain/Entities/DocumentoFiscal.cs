using System;
using System.Security.Cryptography.X509Certificates;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Domain.Entities;

public class DocumentoFiscal
{
    public Guid Id { get; set; }
    public string ChaveAcesso { get; set; }
    public DateTime Data { get; set; }
    public TipoPessoaFiscal TipoEmissor {get; set;}
    public  string DocumentoEmissor { get;  set; } 

    public TipoPessoaFiscal TipoDestinatario {get; set;}
    public  string DocumentoDestinatario { get;  set; }

    public TipoDocumentoFiscal TipoDocumento { get;  set; } 

    public decimal ValorTotal { get;  set; } = 0;

    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public string XMLOriginal { get; set; }

    public string HashXml { get; set; }
}
