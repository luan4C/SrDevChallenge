using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Diagnostics;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Exceptions;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;

public class DocumentoFiscalRepository(SrDevChallengeContext context) : BaseRepository<DocumentoFiscal>(context), IDocumentoFiscalRepository
{
    public override async Task AddAsync(DocumentoFiscal entity)
    {
        try
        {
            await base.AddAsync(entity);
        }
        catch (MongoBulkWriteException ex) when (MongoDbHelpers.IsDuplicatedKeyException(ex))
        {
            throw new ConflictException("Documento Fiscal com a mesma Chave de Acesso já existe.");
        }
    }
    
    public override async Task SaveChangesAsync()
    {
        try
        {
            await base.SaveChangesAsync();
        }
        catch (MongoBulkWriteException ex) when (MongoDbHelpers.IsDuplicatedKeyException(ex))
        {
            throw new ConflictException("Documento Fiscal com a mesma Chave de Acesso já existe.");
        }
    }

    public async Task<DocumentoFiscal?> GetByChaveAcessoAsync(string chaveacesso)
    {
        ArgumentException.ThrowIfNullOrEmpty(chaveacesso.Trim());
        return await _collection.FirstOrDefaultAsync(d => d.ChaveAcesso.Equals(chaveacesso));
    }

    public async Task<DocumentoFiscal?> GetById(Guid id)
    {
        return await _collection.FirstOrDefaultAsync(d => d.Id.Equals(id));
    }

    public async Task<DocumentoFiscal?> GetByHashAsync(string hashXml)
    {
        ArgumentException.ThrowIfNullOrEmpty(hashXml.Trim());
        return await _collection.FirstOrDefaultAsync(d => d.HashXml.Equals(hashXml));
    }
}
