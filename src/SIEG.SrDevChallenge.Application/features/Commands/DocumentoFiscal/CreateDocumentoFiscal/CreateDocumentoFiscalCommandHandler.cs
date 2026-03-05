using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Events;
using SIEG.SrDevChallenge.Domain.Exceptions;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;

public class CreateDocumentoFiscalCommandHandler(IDocumentoFiscalRepository repository, IDocumentSchemaValidator schemaValidator, IEventPublisher eventPublisher) : IRequestHandler<CreateDocumentoFiscalCommand, Result<Guid>>
{
    private readonly IDocumentoFiscalRepository _repository = repository;
    private readonly IDocumentSchemaValidator schemaValidator = schemaValidator;
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public async Task<Result<Guid>> Handle(CreateDocumentoFiscalCommand request, CancellationToken cancellationToken)
    {
        DocumentoFiscalReader reader = new(request.XMLdoc);

        var result = await schemaValidator.ValidateAsync(reader.XmlOriginal, reader.Metadata.TipoDocumento);
        if (!result.IsValid)
        {
            //Todo criar estrutura melhor
            throw new ValidationException($"XML inválido para {reader.Metadata.TipoDocumento}", new Dictionary<string, string[]>
            {
                { "Validacoes", result.Errors.ToArray() },
               
            });
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
            TipoEmissor = reader.Metadata.TipoEmitente,
            HashXml = reader.HashXml,
            XMLOriginal = reader.XmlOriginal
        };

        try
        {
            await _repository.AddAsync(documentoFiscal);
   
            var evento = new DocumentoFiscalCriado(documentoFiscal);
            await _eventPublisher.PublishAsync(evento, cancellationToken: cancellationToken);
        }
        catch (ConflictException)
        {
            var existing = await _repository.GetByHashAsync(documentoFiscal.HashXml) ?? throw new NotFoundException($"Documento fiscal com hash {documentoFiscal.HashXml} não encontrado.");
            return new Result<Guid>(existing.Id, "Documento fiscal já existe.");
        }

        return new Result<Guid>(documentoFiscal.Id, "Documento fiscal criado com sucesso.") ;
    }
}
