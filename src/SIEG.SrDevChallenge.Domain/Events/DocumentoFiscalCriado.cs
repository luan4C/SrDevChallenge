using SIEG.SrDevChallenge.Domain.Entities;

namespace SIEG.SrDevChallenge.Domain.Events;

public class DocumentoFiscalCriado
{
    public Guid DocumentoFiscalId { get; set; }
    public string ChaveAcesso { get; set; }
    public DateTime Data { get; set; }
    public string DocumentoEmissor { get; set; }
    public string DocumentoDestinatario { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime CriadoEm { get; set; }
    public string TipoDocumento { get; set; }

    public DocumentoFiscalCriado(DocumentoFiscal documentoFiscal)
    {
        DocumentoFiscalId = documentoFiscal.Id;
        ChaveAcesso = documentoFiscal.ChaveAcesso;
        Data = documentoFiscal.Data;
        DocumentoEmissor = documentoFiscal.DocumentoEmissor;
        DocumentoDestinatario = documentoFiscal.DocumentoDestinatario;
        ValorTotal = documentoFiscal.ValorTotal;
        CriadoEm = documentoFiscal.CriadoEm;
        TipoDocumento = documentoFiscal.TipoDocumento.ToString();
    }

    // Construtor parameterless para serialização
    public DocumentoFiscalCriado() { }
}