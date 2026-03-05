using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Exceptions;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.UpdateDocumentoFiscal;

public class UpdateDocumentoFiscalCommandHandler(IDocumentoFiscalRepository repository, IDocumentSchemaValidator schemaValidator) 
    : IRequestHandler<UpdateDocumentoFiscalCommand, Result<object>>
{
    private readonly IDocumentoFiscalRepository _repository = repository;
    private readonly IDocumentSchemaValidator _schemaValidator = schemaValidator;

    public async Task<Result<object>> Handle(UpdateDocumentoFiscalCommand request, CancellationToken cancellationToken)
    {
        
        var existingDocument = await _repository.GetById(request.Id)
        ?? throw new NotFoundException($"Documento fiscal com ID {request.Id} não foi encontrado.");


        DocumentoFiscalReader reader = new(request.XMLdoc);
        var validationResult = await _schemaValidator.ValidateAsync(reader.XmlOriginal, reader.Metadata.TipoDocumento);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException($"XML inválido para {reader.Metadata.TipoDocumento}", new Dictionary<string, string[]>
            {
                { "Validacoes", validationResult.Errors.ToArray() }
            });
        }

        // Atualiza os campos do documento existente
        existingDocument.DocumentoEmissor = reader.Metadata.DocumentoEmitente;
        existingDocument.DocumentoDestinatario = reader.Metadata.DocumentoDestinatario;
        existingDocument.TipoDocumento = reader.Metadata.TipoDocumento;
        existingDocument.Data = reader.Metadata.DataEmissao.Value;
        existingDocument.ChaveAcesso = reader.Metadata.ChaveAcesso;
        existingDocument.ValorTotal = reader.Metadata.ValorTotal;
        existingDocument.TipoDestinatario = reader.Metadata.TipoDestinatario;
        existingDocument.TipoEmissor = reader.Metadata.TipoEmitente;
        existingDocument.HashXml = reader.HashXml;
        existingDocument.XMLOriginal = reader.XmlOriginal;
        existingDocument.AtualizadoEm = DateTime.UtcNow;

        try
        {
            await _repository.UpdateAsync(existingDocument);
            return new Result<object>(existingDocument.Id,"Documento fiscal atualizado com sucesso.");
        }
        catch (ConflictException)
        {
            throw new ConflictException("Já existe um documento fiscal com esse hash XML.");
        }
    }
}
