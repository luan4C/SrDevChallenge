using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;

public class CreateDocumentoFiscalCommandHandler(IDocumentoFiscalRepository repository, IDocumentSchemaValidator schemaValidator) : IRequestHandler<CreateDocumentoFiscalCommand, Unit>
{
    private readonly IDocumentoFiscalRepository _repository = repository;
    private readonly IDocumentSchemaValidator schemaValidator = schemaValidator;

    public async Task<Unit> Handle(CreateDocumentoFiscalCommand request, CancellationToken cancellationToken)
    {
        DocumentoFiscalReader reader = new(request.XMLdoc);

        var result = await schemaValidator.ValidateAsync(reader.XmlOriginal, reader.TipoDocumento);
        
        throw new NotImplementedException();
    }
}
