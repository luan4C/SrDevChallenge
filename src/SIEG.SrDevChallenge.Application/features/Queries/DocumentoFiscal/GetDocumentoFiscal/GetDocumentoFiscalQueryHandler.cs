using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Exceptions;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentoFiscal;

public class GetDocumentoFiscalQueryHandler(IDocumentoFiscalRepository documentoFiscalRepository) : IRequestHandler<GetDocumentoFiscalQuery, Result<DocumentoFiscalDetailsDTO>>
{
    private readonly IDocumentoFiscalRepository _documentoFiscalRepository = documentoFiscalRepository;

    public async Task<Result<DocumentoFiscalDetailsDTO>> Handle(GetDocumentoFiscalQuery request, CancellationToken cancellationToken)
    {
        var documentoFiscal = await _documentoFiscalRepository.GetById(request.Id) 
        ?? throw new NotFoundException($"Documento fiscal com ID {request.Id} não encontrado.");

        var documentoFiscalDetails = new DocumentoFiscalDetailsDTO
        {
            Id = documentoFiscal.Id,
            ChaveAcesso = documentoFiscal.ChaveAcesso,
            Data = documentoFiscal.Data,
            TipoEmissor = documentoFiscal.TipoEmissor,
            DocumentoEmissor = documentoFiscal.DocumentoEmissor,
            TipoDestinatario = documentoFiscal.TipoDestinatario,
            DocumentoDestinatario = documentoFiscal.DocumentoDestinatario,
            TipoDocumento = documentoFiscal.TipoDocumento,
            ValorTotal = documentoFiscal.ValorTotal,
            CriadoEm = documentoFiscal.CriadoEm,
            AtualizadoEm = documentoFiscal.AtualizadoEm,
            XMLOriginal = documentoFiscal.XMLOriginal,
            HashXml = documentoFiscal.HashXml
        };

        return new Result<DocumentoFiscalDetailsDTO>(documentoFiscalDetails, "Documento fiscal retornado com sucesso.");
    }
}
