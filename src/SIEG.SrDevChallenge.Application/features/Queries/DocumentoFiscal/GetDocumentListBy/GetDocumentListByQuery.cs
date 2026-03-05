using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentListBy;

public record GetDocumentListByQuery(DateTime DataInicio, DateTime DataFim, string DocumentoEmissor,
int PageNumber = 1, int PageSize = 10, 
string? DocumentoDestinatario = default, TipoDocumentoFiscal? TipoDocumento = default
): IRequest<PagedResult<DocumentoFiscalListDTO>>;
