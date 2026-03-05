using Microsoft.EntityFrameworkCore;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;

public class DocumentoFiscaisResumoMensalRepository(SrDevChallengeContext context) : BaseRepository<DocumentoFiscaisResumoMensal>(context), IDocumentoFiscaisResumoMensalRepository
{
    public async Task<DocumentoFiscaisResumoMensal?> GetByAnoMesTipoAsync(int ano, int mes, TipoDocumentoFiscal tipoDocumento)
    {
        return await _collection.FirstOrDefaultAsync(r => 
            r.Ano == ano && 
            r.Mes == mes && 
            r.TipoDocumento == tipoDocumento);
    }
}