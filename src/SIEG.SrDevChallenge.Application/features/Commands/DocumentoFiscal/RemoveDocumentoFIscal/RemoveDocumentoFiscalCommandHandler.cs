using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Exceptions;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.RemoveDocumentoFIscal;

public class RemoveDocumentoFiscalCommandHandler(IDocumentoFiscalRepository documentoFiscalRepository) : IRequestHandler<RemoveDocumentoFiscalCommand, Result<object>>
{
    private readonly IDocumentoFiscalRepository _documentoFiscalRepository = documentoFiscalRepository;

    public async     Task<Result<object>> Handle(RemoveDocumentoFiscalCommand request, CancellationToken cancellationToken)
    {
        var documentoFiscal = await _documentoFiscalRepository.GetById(request.Id) ?? throw new NotFoundException($"Documento fiscal com ID {request.Id} não encontrado.");

        _documentoFiscalRepository.Delete(documentoFiscal);
        await _documentoFiscalRepository.SaveChangesAsync();
        
        return new Result<object>(default, "Documento fiscal removido com sucesso.");
    }
}
