using System;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Infrastructure.Services;

public class DocumentoFiscalXMLSchemaValidator : IDocumentSchemaValidator
{
    public Task ValidateAsync(string xmlContent, TipoDocumentoFiscal tipoDocumento)
    {
        throw new NotImplementedException();
    }
}
