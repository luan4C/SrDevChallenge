using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Domain.Entities;

public class DocumentoFiscaisResumoMensal
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public TipoDocumentoFiscal TipoDocumento { get; set; }
    public int QuantidadeDocumentos { get; set; }
    public decimal ValorTotalDocumentos { get; set; }
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public DocumentoFiscaisResumoMensal()
    {
        Id = Guid.NewGuid();
    }

    public DocumentoFiscaisResumoMensal(int ano, int mes, TipoDocumentoFiscal tipoDocumento)
    {
        Id = Guid.NewGuid();
        Ano = ano;
        Mes = mes;
        TipoDocumento = tipoDocumento;
        QuantidadeDocumentos = 0;
        ValorTotalDocumentos = 0;
    }

    public void AdicionarDocumento(decimal valorDocumento)
    {
        QuantidadeDocumentos++;
        ValorTotalDocumentos += valorDocumento;
        UltimaAtualizacao = DateTime.UtcNow;
    }
}