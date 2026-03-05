using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.RemoveDocumentoFIscal;

public record RemoveDocumentoFiscalCommand(Guid Id) : IRequest<Result<object>>;
