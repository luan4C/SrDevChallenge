using FluentAssertions;
using NUnit.Framework;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.UnitTests.Domain;

[TestFixture]
public class DocumentoFiscaisResumoMensalTests
{
    [Test]
    public void Constructor_WithParameters_ShouldInitializeCorrectly()
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
        resumo.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        resumo.UltimaAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var resumo = new DocumentoFiscaisResumoMensal();

        // Assert
        resumo.Id.Should().NotBe(Guid.Empty);
        resumo.Ano.Should().Be(0);
        resumo.Mes.Should().Be(0);
        resumo.TipoDocumento.Should().Be(default(TipoDocumentoFiscal));
        resumo.QuantidadeDocumentos.Should().Be(0);
        resumo.ValorTotalDocumentos.Should().Be(0);
    }

    [Test]
    public async Task AdicionarDocumento_ShouldIncrementCountersAndUpdateTimestamp()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 3, TipoDocumentoFiscal.NFe);
        var valorDocumento = 1500.75m;
        var timestampAntes = resumo.UltimaAtualizacao;

        // Pequeno delay para garantir diferença de timestamp
        await Task.Delay(10);

        // Act
        resumo.AdicionarDocumento(valorDocumento);

        // Assert
        resumo.QuantidadeDocumentos.Should().Be(1);
        resumo.ValorTotalDocumentos.Should().Be(valorDocumento);
        resumo.UltimaAtualizacao.Should().BeAfter(timestampAntes);
    }

    [Test]
    public void AdicionarDocumento_MultipleTimes_ShouldAccumulateValues()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 3, TipoDocumentoFiscal.NFe);

        // Act
        resumo.AdicionarDocumento(100.50m);
        resumo.AdicionarDocumento(200.75m);
        resumo.AdicionarDocumento(50.25m);

        // Assert
        resumo.QuantidadeDocumentos.Should().Be(3);
        resumo.ValorTotalDocumentos.Should().Be(351.50m);
    }

    [Test]
    public void AdicionarDocumento_WithZeroValue_ShouldIncrementCountOnly()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 12, TipoDocumentoFiscal.CTe);

        // Act
        resumo.AdicionarDocumento(0m);

        // Assert
        resumo.QuantidadeDocumentos.Should().Be(1);
        resumo.ValorTotalDocumentos.Should().Be(0m);
    }

    [Test]
    public void AdicionarDocumento_WithNegativeValue_ShouldAllowNegativeTotal()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFSe);

        // Act
        resumo.AdicionarDocumento(-50.00m);

        // Assert
        resumo.QuantidadeDocumentos.Should().Be(1);
        resumo.ValorTotalDocumentos.Should().Be(-50.00m);
    }

    [Test]
    public void Constructor_WithDifferentDocumentTypes_ShouldMaintainType()
    {
        // Arrange & Act
        var resumoNFe = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFe);
        var resumoCTe = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.CTe);
        var resumoNFSe = new DocumentoFiscaisResumoMensal(2024, 1, TipoDocumentoFiscal.NFSe);

        // Assert
        resumoNFe.TipoDocumento.Should().Be(TipoDocumentoFiscal.NFe);
        resumoCTe.TipoDocumento.Should().Be(TipoDocumentoFiscal.CTe);
        resumoNFSe.TipoDocumento.Should().Be(TipoDocumentoFiscal.NFSe);
    }

    [Test]
    public void AdicionarDocumento_LargeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 6, TipoDocumentoFiscal.NFe);

        // Act
        resumo.AdicionarDocumento(999999999.99m);
        resumo.AdicionarDocumento(888888888.88m);

        // Assert
        resumo.QuantidadeDocumentos.Should().Be(2);
        resumo.ValorTotalDocumentos.Should().Be(1888888888.87m);
    }

    [Test]
    public async Task AdicionarDocumento_MultipleCallsQuickly_ShouldUpdateTimestampsCorrectly()
    {
        // Arrange
        var resumo = new DocumentoFiscaisResumoMensal(2024, 8, TipoDocumentoFiscal.CTe);

        // Act
        resumo.AdicionarDocumento(100m);
        await Task.Delay(5);
        var timestamp1 = resumo.UltimaAtualizacao;
        
        await Task.Delay(10);
        resumo.AdicionarDocumento(200m);
        var timestamp2 = resumo.UltimaAtualizacao;

        // Assert
        timestamp2.Should().BeAfter(timestamp1);
        resumo.QuantidadeDocumentos.Should().Be(2);
        resumo.ValorTotalDocumentos.Should().Be(300m);
    }
}