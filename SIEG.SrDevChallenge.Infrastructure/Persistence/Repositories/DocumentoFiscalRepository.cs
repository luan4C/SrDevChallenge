using System;
using Microsoft.EntityFrameworkCore;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;

public class DocumentoFiscalRepository(SrDevChallengeContext context) : BaseRepository<DocumentoFiscal>(context), IDocumentoFiscalRepository
{
    public async Task<DocumentoFiscal?> GetByChaveAcessoAsync(string chaveacesso)
    {
        ArgumentException.ThrowIfNullOrEmpty(chaveacesso.Trim());
        return await _collection.FirstOrDefaultAsync(d => d.ChaveAcesso.Equals(chaveacesso));
    }

    public async Task<DocumentoFiscal?> GetById(Guid id)
    {        
        return await _collection.FirstOrDefaultAsync(d => d.Id.Equals(id));
    }
}
