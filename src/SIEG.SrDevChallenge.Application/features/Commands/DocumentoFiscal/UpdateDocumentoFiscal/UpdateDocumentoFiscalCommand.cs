using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.UpdateDocumentoFiscal;

public record UpdateDocumentoFiscalCommand(Guid Id, string XMLdoc) : IRequest<Result<object>>;
