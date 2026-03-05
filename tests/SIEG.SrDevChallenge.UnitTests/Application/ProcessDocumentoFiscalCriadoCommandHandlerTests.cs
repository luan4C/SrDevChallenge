using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.features.Events.DocumentoFiscalCriado;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Domain.Events;

namespace SIEG.SrDevChallenge.UnitTests.Application;

[TestFixture]
public class ProcessDocumentoFiscalCriadoCommandHandlerTests
{
    private Mock<IDocumentoFiscaisResumoMensalRepository> _resumoRepositoryMock;
    private Mock<ILogger<ProcessDocumentoFiscalCriadoCommandHandler>> _loggerMock;
    private ProcessDocumentoFiscalCriadoCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _resumoRepositoryMock = new Mock<IDocumentoFiscaisResumoMensalRepository>();
        _loggerMock = new Mock<ILogger<ProcessDocumentoFiscalCriadoCommandHandler>>();
        
        _handler = new ProcessDocumentoFiscalCriadoCommandHandler(
            _resumoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_NewMonthlyResume_ShouldCreateNewEntry()
    {
        // Arrange
        var evento = new DocumentoFiscalCriado
        {
            DocumentoFiscalId = Guid.NewGuid(),
            ChaveAcesso = "35200314200166000187550010000000046550010008",
            Data = new DateTime(2024, 3, 15),
            DocumentoEmissor = "14200166000187",
            DocumentoDestinatario = "12345678901",
            ValorTotal = 1500.75m,
            CriadoEm = DateTime.UtcNow,
            TipoDocumento = "NFe"
        };

        var command = new ProcessDocumentoFiscalCriadoCommand(evento);

        _resumoRepositoryMock.Setup(r => r.GetByAnoMesTipoAsync(2024, 3, TipoDocumentoFiscal.NFe))
            .ReturnsAsync((DocumentoFiscaisResumoMensal?)null);

        _resumoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscaisResumoMensal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _resumoRepositoryMock.Verify(r => r.AddAsync(It.Is<DocumentoFiscaisResumoMensal>(resumo =>
            resumo.Ano == 2024 &&
            resumo.Mes == 3 &&
            resumo.TipoDocumento == TipoDocumentoFiscal.NFe &&
            resumo.QuantidadeDocumentos == 1 &&
            resumo.ValorTotalDocumentos == 1500.75m
        )), Times.Once);

        _resumoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DocumentoFiscaisResumoMensal>()), Times.Never);
    }

    [Test]
    public async Task Handle_ExistingMonthlyResume_ShouldUpdateEntry()
    {
        // Arrange
        var evento = new DocumentoFiscalCriado
        {
            DocumentoFiscalId = Guid.NewGuid(),
            Data = new DateTime(2024, 2, 28),
            ValorTotal = 750.25m,
            TipoDocumento = "NFe"
        };

        var existingResumo = new DocumentoFiscaisResumoMensal(2024, 2, TipoDocumentoFiscal.NFe);
        existingResumo.AdicionarDocumento(1000.00m); // Já tem 1 documento
        existingResumo.AdicionarDocumento(500.50m);  // Já tem 2 documentos

        var command = new ProcessDocumentoFiscalCriadoCommand(evento);

        _resumoRepositoryMock.Setup(r => r.GetByAnoMesTipoAsync(2024, 2, TipoDocumentoFiscal.NFe))
            .ReturnsAsync(existingResumo);

        _resumoRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DocumentoFiscaisResumoMensal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _resumoRepositoryMock.Verify(r => r.UpdateAsync(It.Is<DocumentoFiscaisResumoMensal>(resumo =>
            resumo.QuantidadeDocumentos == 3 &&
            resumo.ValorTotalDocumentos == 2250.75m // 1000 + 500.50 + 750.25
        )), Times.Once);

        _resumoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DocumentoFiscaisResumoMensal>()), Times.Never);
    }

    [Test]
    public async Task Handle_InvalidTipoDocumento_ShouldLogWarningAndReturn()
    {
        // Arrange
        var evento = new DocumentoFiscalCriado
        {
            DocumentoFiscalId = Guid.NewGuid(),
            Data = new DateTime(2024, 3, 15),
            ValorTotal = 1500.75m,
            TipoDocumento = "INVALID_TYPE"
        };

        var command = new ProcessDocumentoFiscalCriadoCommand(evento);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _resumoRepositoryMock.Verify(r => r.GetByAnoMesTipoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TipoDocumentoFiscal>()), Times.Never);
        _resumoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DocumentoFiscaisResumoMensal>()), Times.Never);
        _resumoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DocumentoFiscaisResumoMensal>()), Times.Never);
    }

    [Test]
    public async Task Handle_CTeEvent_ShouldCreateCorrectResumeType()
    {
        // Arrange
        var evento = new DocumentoFiscalCriado
        {
            DocumentoFiscalId = Guid.NewGuid(),
            Data = new DateTime(2024, 1, 10),
            ValorTotal = 2500.00m,
            TipoDocumento = "CTe"
        };

        var command = new ProcessDocumentoFiscalCriadoCommand(evento);

        _resumoRepositoryMock.Setup(r => r.GetByAnoMesTipoAsync(2024, 1, TipoDocumentoFiscal.CTe))
            .ReturnsAsync((DocumentoFiscaisResumoMensal?)null);

        _resumoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscaisResumoMensal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _resumoRepositoryMock.Verify(r => r.AddAsync(It.Is<DocumentoFiscaisResumoMensal>(resumo =>
            resumo.TipoDocumento == TipoDocumentoFiscal.CTe &&
            resumo.ValorTotalDocumentos == 2500.00m
        )), Times.Once);
    }

    [Test]
    public async Task Handle_NFSeEvent_ShouldCreateCorrectResumeType()
    {
        // Arrange
        var evento = new DocumentoFiscalCriado
        {
            DocumentoFiscalId = Guid.NewGuid(),
            Data = new DateTime(2024, 12, 25),
            ValorTotal = 850.30m,
            TipoDocumento = "NFSe"
        };

        var command = new ProcessDocumentoFiscalCriadoCommand(evento);

        _resumoRepositoryMock.Setup(r => r.GetByAnoMesTipoAsync(2024, 12, TipoDocumentoFiscal.NFSe))
            .ReturnsAsync((DocumentoFiscaisResumoMensal?)null);

        _resumoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscaisResumoMensal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _resumoRepositoryMock.Verify(r => r.AddAsync(It.Is<DocumentoFiscaisResumoMensal>(resumo =>
            resumo.TipoDocumento == TipoDocumentoFiscal.NFSe &&
            resumo.Ano == 2024 &&
            resumo.Mes == 12
        )), Times.Once);
    }
}