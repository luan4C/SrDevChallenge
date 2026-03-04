using System;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IDocumentSchemaValidator
{
    Task ValidateAsync(string xmlContent, TipoDocumentoFiscal tipoDocumento);
}
