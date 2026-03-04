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

        var result = await schemaValidator.ValidateAsync(reader.XmlOriginal, reader.Metadata.TipoDocumento);
        if (!result.IsValid)
        {
            //Todo criar estrutura melhor
            throw new InvalidDataException($"Xml de {reader.Metadata.TipoDocumento} inválido");
        }
        Domain.Entities.DocumentoFiscal documentoFiscal = new()
        {                       
            DocumentoEmissor = reader.Metadata.DocumentoEmitente,
            DocumentoDestinatario = reader.Metadata.DocumentoDestinatario,
            TipoDocumento = reader.Metadata.TipoDocumento,
            Data = reader.Metadata.DataEmissao.Value,
            ChaveAcesso = reader.Metadata.ChaveAcesso,
            ValorTotal = reader.Metadata.ValorTotal,
            TipoDestinatario = reader.Metadata.TipoDestinatario,
            TipoEmissor = reader.Metadata.TipoEmitente
        };
        
        await _repository.AddAsync(documentoFiscal);

        return Unit.Value;
    }
}
