using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IDocumentoFiscaisResumoMensalRepository : IRepository<DocumentoFiscaisResumoMensal>
{
    Task<DocumentoFiscaisResumoMensal?> GetByAnoMesTipoAsync(int ano, int mes, TipoDocumentoFiscal tipoDocumento);
}