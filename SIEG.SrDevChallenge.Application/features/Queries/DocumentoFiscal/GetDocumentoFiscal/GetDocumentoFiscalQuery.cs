using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentoFiscal;

public record GetDocumentoFiscalQuery(Guid Id) : IRequest<Result<DocumentoFiscalDetailsDTO>>;
