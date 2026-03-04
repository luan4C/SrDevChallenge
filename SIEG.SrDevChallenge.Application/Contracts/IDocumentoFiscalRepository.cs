using System;
using SIEG.SrDevChallenge.Domain.Entities;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IDocumentoFiscalRepository : IRepository<DocumentoFiscal>
{
    Task<DocumentoFiscal?> GetByChaveAcessoAsync(string chaveacesso);
    Task<DocumentoFiscal?> GetById(Guid id);
    
}
