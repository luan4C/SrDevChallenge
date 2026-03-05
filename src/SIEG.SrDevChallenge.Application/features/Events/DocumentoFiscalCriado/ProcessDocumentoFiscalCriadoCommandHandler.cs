using MediatR;
using Microsoft.Extensions.Logging;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.features.Events.DocumentoFiscalCriado;

public class ProcessDocumentoFiscalCriadoCommandHandler : IRequestHandler<ProcessDocumentoFiscalCriadoCommand>
{
    private readonly IDocumentoFiscaisResumoMensalRepository _resumoRepository;
    private readonly ILogger<ProcessDocumentoFiscalCriadoCommandHandler> _logger;

    public ProcessDocumentoFiscalCriadoCommandHandler(
        IDocumentoFiscaisResumoMensalRepository resumoRepository,
        ILogger<ProcessDocumentoFiscalCriadoCommandHandler> logger)
    {
        _resumoRepository = resumoRepository;
        _logger = logger;
    }

    public async Task Handle(ProcessDocumentoFiscalCriadoCommand request, CancellationToken cancellationToken)
    {
        var evento = request.Evento;
        
        _logger.LogInformation("Processing DocumentoFiscalCriado event for document {DocumentId}", evento.DocumentoFiscalId);

        try
        {
            var ano = evento.Data.Year;
            var mes = evento.Data.Month;

            if (!Enum.TryParse<TipoDocumentoFiscal>(evento.TipoDocumento, out var tipoDocumento))
            {
                _logger.LogWarning("Invalid document type: {TipoDocumento}", evento.TipoDocumento);
                return;
            }

            var resumo = await _resumoRepository.GetByAnoMesTipoAsync(ano, mes, tipoDocumento);
            
            if (resumo == null)
            {
                resumo = new DocumentoFiscaisResumoMensal(ano, mes, tipoDocumento);
                resumo.AdicionarDocumento(evento.ValorTotal);
                await _resumoRepository.AddAsync(resumo);
                
                _logger.LogInformation("Created new monthly summary for {Ano}/{Mes:D2} - {TipoDocumento}", 
                    ano, mes, tipoDocumento);
            }
            else
            {
                resumo.AdicionarDocumento(evento.ValorTotal);
                await _resumoRepository.UpdateAsync(resumo);
                
                _logger.LogInformation("Updated existing monthly summary for {Ano}/{Mes:D2} - {TipoDocumento}. " +
                    "New totals: {Quantidade} documents, ${ValorTotal}", 
                    ano, mes, tipoDocumento, resumo.QuantidadeDocumentos, resumo.ValorTotalDocumentos);
            }

            _logger.LogInformation("Successfully processed DocumentoFiscalCriado event for document {DocumentId}", 
                evento.DocumentoFiscalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DocumentoFiscalCriado event for document {DocumentId}", 
                evento.DocumentoFiscalId);
            throw;
        }
    }
}