using System;
using MediatR;
using SIEG.SrDevChallenge.Application.Models;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;

public record CreateDocumentoFiscalCommand(string XMLdoc): IRequest<Result<Guid>>;
