using System;
using MediatR;

namespace SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;

public record CreateDocumentoFiscalCommand(string XMLdoc): IRequest<Unit>;
