using MediatR;
using SIEG.SrDevChallenge.Domain.Events;

namespace SIEG.SrDevChallenge.Application.features.Events.DocumentoFiscalCriado;

public record ProcessDocumentoFiscalCriadoCommand(Domain.Events.DocumentoFiscalCriado Evento) : IRequest;