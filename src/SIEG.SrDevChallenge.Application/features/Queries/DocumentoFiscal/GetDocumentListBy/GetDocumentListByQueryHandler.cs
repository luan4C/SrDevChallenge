using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentListBy;

public class GetDocumentListByQueryHandler(IDocumentoFiscalRepository documentoFiscalRepository) :
 IRequestHandler<GetDocumentListByQuery, PagedResult<DocumentoFiscalListDTO>>
{
    private readonly IDocumentoFiscalRepository _documentoFiscalRepository = documentoFiscalRepository;

    public Task<PagedResult<DocumentoFiscalListDTO>> Handle(GetDocumentListByQuery request, CancellationToken cancellationToken)
    {
        var queryable = _documentoFiscalRepository.GetIQueryable();
        var dataTimeInicio = request.DataInicio;
        var dataTimeFim = new DateTime(request.DataFim.Year, request.DataFim.Month, request.DataFim.Day, 23, 59, 59);

        //limpar cnpj

        queryable = queryable.Where(d =>
            d.Data >= dataTimeInicio && d.Data <= dataTimeFim &&
            d.DocumentoEmissor == request.DocumentoEmissor && 
            (string.IsNullOrEmpty(request.DocumentoDestinatario) || d.DocumentoDestinatario == request.DocumentoDestinatario) &&
            (!request.TipoDocumento.HasValue || d.TipoDocumento == request.TipoDocumento.Value));

        var totalCount = queryable.Count();

        var items = queryable
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DocumentoFiscalListDTO
            {
                Id = d.Id,
                ChaveAcesso = d.ChaveAcesso,
                DocumentoEmissor = d.DocumentoEmissor,
                DocumentoDestinatario = d.DocumentoDestinatario,
                DataEmissao = d.Data,
                ValorTotal = d.ValorTotal,
                TipoDocumento = d.TipoDocumento
            }).ToList();

        return Task.FromResult(new PagedResult<DocumentoFiscalListDTO>(items, request.PageNumber, request.PageSize, totalCount));
        
    }
}
