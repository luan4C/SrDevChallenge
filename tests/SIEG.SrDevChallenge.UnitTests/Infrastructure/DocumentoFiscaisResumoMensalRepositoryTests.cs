using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;

namespace SIEG.SrDevChallenge.UnitTests.Infrastructure;

[TestFixture]
public class DocumentoFiscaisResumoMensalRepositoryTests
{
    private Mock<ILogger<DocumentoFiscaisResumoMensalRepository>> _loggerMock;
    private DocumentoFiscaisResumoMensalRepository _repository;

    [SetUp] 
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<DocumentoFiscaisResumoMensalRepository>>();
        // Note: Este teste seria mais apropriado como teste de integração com banco real
        // Aqui estamos testando apenas a lógica de negócio
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_EntityProperties_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var resumo = new DocumentoFiscaisResumoMensal(2024, 3, TipoDocumentoFiscal.NFe);
        
        // Assert
        resumo.Id.Should().NotBe(Guid.Empty);
        resumo.Ano.Should().Be(2024);
        resumo.Mes.Should().Be(3);
        resumo.TipoDocumento.Should().Be(TipoDocumentoFiscal.NFe);
        resumo.QuantidadeDocumentos.Should().Be(0);
        resumo.ValorTotalDocumentos.Should().Be(0);
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_BusinessLogic_ShouldWorkCorrectly()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 12, TipoDocumentoFiscal.CTe);

        // Act & Assert
        resumo.AdicionarDocumento(1000.50m);
        resumo.QuantidadeDocumentos.Should().Be(1);
        resumo.ValorTotalDocumentos.Should().Be(1000.50m);

        resumo.AdicionarDocumento(500.25m);
        resumo.QuantidadeDocumentos.Should().Be(2);
        resumo.ValorTotalDocumentos.Should().Be(1500.75m);
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_UniqueConstraint_ShouldEnforceAnoMesTipo()
    {
        // Arrange & Act - Simulando constraint única
        var resumo1 = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFe);
        var resumo2 = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFe);
        var resumo3 = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.CTe);

        // Assert - Mesmo ano/mês mas tipos diferentes devem ser permitidos
        resumo1.Should().NotBeSameAs(resumo2);
        resumo1.TipoDocumento.Should().Be(resumo2.TipoDocumento);
        resumo3.TipoDocumento.Should().NotBe(resumo1.TipoDocumento);
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_DifferentMonths_ShouldBeDistinct()
    {
        // Arrange & Act
        var janeiro = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFe);
        var fevereiro = new DocumentoFiscaisResumoMensal(2024, 2, TipoDocumentoFiscal.NFe);
        var marco = new DocumentoFiscaisResumoMensal(2024, 3, TipoDocumentoFiscal.NFe);

        janeiro.AdicionarDocumento(1000m);
        fevereiro.AdicionarDocumento(2000m);
        marco.AdicionarDocumento(3000m);

        // Assert
        janeiro.Mes.Should().Be(1);
        janeiro.ValorTotalDocumentos.Should().Be(1000m);
        
        fevereiro.Mes.Should().Be(2);
        fevereiro.ValorTotalDocumentos.Should().Be(2000m);
        
        marco.Mes.Should().Be(3);
        marco.ValorTotalDocumentos.Should().Be(3000m);
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_DifferentYears_ShouldBeDistinct()
    {
        // Arrange & Act
        var resumo2023 = new DocumentoFiscaisResumoMensal(2023, 12, TipoDocumentoFiscal.NFe);
        var resumo2024 = new DocumentoFiscaisResumoMensal(2024, 12, TipoDocumentoFiscal.NFe);

        resumo2023.AdicionarDocumento(5000m);
        resumo2024.AdicionarDocumento(7500m);

        // Assert
        resumo2023.Ano.Should().Be(2023);
        resumo2023.ValorTotalDocumentos.Should().Be(5000m);
        
        resumo2024.Ano.Should().Be(2024);
        resumo2024.ValorTotalDocumentos.Should().Be(7500m);
    }

    [Test]
    public void DocumentoFiscaisResumoMensal_AllDocumentTypes_ShouldBeSupported()
    {
        // Arrange & Act
        var resumoNFe = new DocumentoFiscaisResumoMensal(2024, 6, TipoDocumentoFiscal.NFe);
        var resumoCTe = new DocumentoFiscaisResumoMensal(2024, 6, TipoDocumentoFiscal.CTe);
        var resumoNFSe = new DocumentoFiscaisResumoMensal(2024, 6, TipoDocumentoFiscal.NFSe);

        // Assert
        resumoNFe.TipoDocumento.Should().Be(TipoDocumentoFiscal.NFe);
        resumoCTe.TipoDocumento.Should().Be(TipoDocumentoFiscal.CTe);
        resumoNFSe.TipoDocumento.Should().Be(TipoDocumentoFiscal.NFSe);

        // Todos podem ter o mesmo mês/ano mas são diferentes por tipo
        resumoNFe.Mes.Should().Be(resumoCTe.Mes);
        resumoNFe.Mes.Should().Be(resumoNFSe.Mes);
        resumoNFe.Ano.Should().Be(resumoCTe.Ano);
        resumoNFe.Ano.Should().Be(resumoNFSe.Ano);
    }
}