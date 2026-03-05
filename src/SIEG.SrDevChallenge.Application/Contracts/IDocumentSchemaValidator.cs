using System;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IDocumentSchemaValidator
{
    Task<DocumentSchemaValidationResult> ValidateAsync(string xmlContent, TipoDocumentoFiscal? tipoDocumento = null);
}

public class DocumentSchemaValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors {get;} = [];
}