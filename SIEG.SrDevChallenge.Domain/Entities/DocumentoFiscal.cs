using System;
using System.Security.Cryptography.X509Certificates;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Domain.Entities;

public class DocumentoFiscal
{
    public Guid Id { get; set; }
    public string ChaveAcesso { get; set; }
    public DateTime Data { get; private set; }
    
    public  string DocumentoEmissor { get;  set; } 

    public  string DocumentoDestinatario { get;  set; }

    public TipoDocumentoFiscal TipoDocumento { get;  protected set; } 

    public decimal ValorTotal { get;  set; } = 0;

    public DateTime CriadoEm { get; private set; } = DateTime.Now;

    public DateTime AtualizadoEm { get; set; } = DateTime.Now;

    public string XMLOriginal { get; set; }

    public string HashXml { get; set; }
}
