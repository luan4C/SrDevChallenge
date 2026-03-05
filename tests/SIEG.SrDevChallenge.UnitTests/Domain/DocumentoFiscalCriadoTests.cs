using FluentAssertions;
using NUnit.Framework;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Domain.Events;

namespace SIEG.SrDevChallenge.UnitTests.Domain;

[TestFixture]
public class DocumentoFiscalCriadoTests
{
    [Test]
    public void Constructor_WithDocumentoFiscal_ShouldMapAllProperties()
    {
        // Arrange
        var documentoFiscal = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = "35200314200166000187550010000000046550010008",
            Data = new DateTime(2024, 3, 15, 10, 30, 0),
            DocumentoEmissor = "14200166000187",
            DocumentoDestinatario = "12345678901",
            ValorTotal = 1500.75m,            
            TipoDocumento = TipoDocumentoFiscal.NFe
        };

        // Act
        var evento = new DocumentoFiscalCriado(documentoFiscal);

        // Assert
        evento.DocumentoFiscalId.Should().Be(documentoFiscal.Id);
        evento.ChaveAcesso.Should().Be(documentoFiscal.ChaveAcesso);
        evento.Data.Should().Be(documentoFiscal.Data);
        evento.DocumentoEmissor.Should().Be(documentoFiscal.DocumentoEmissor);
        evento.DocumentoDestinatario.Should().Be(documentoFiscal.DocumentoDestinatario);
        evento.ValorTotal.Should().Be(documentoFiscal.ValorTotal);
        evento.CriadoEm.Should().Be(documentoFiscal.CriadoEm);
        evento.TipoDocumento.Should().Be("NFe");
    }

    [Test]
    public void Constructor_WithCTe_ShouldMapTipoDocumentoCorrectly()
    {
        // Arrange
        var documentoCTe = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = "43200314200166000187570010000000047570010009",
            TipoDocumento = TipoDocumentoFiscal.CTe,
            ValorTotal = 2500.00m
        };

        // Act
        var evento = new DocumentoFiscalCriado(documentoCTe);

        // Assert
        evento.TipoDocumento.Should().Be("CTe");
        evento.DocumentoFiscalId.Should().Be(documentoCTe.Id);
        evento.ValorTotal.Should().Be(2500.00m);
    }

    [Test]
    public void Constructor_WithNFSe_ShouldMapTipoDocumentoCorrectly()
    {
        // Arrange
        var documentoNFSe = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = "12345678901234567890123456789012345678901234",
            TipoDocumento = TipoDocumentoFiscal.NFSe,
            ValorTotal = 850.25m
        };

        // Act
        var evento = new DocumentoFiscalCriado(documentoNFSe);

        // Assert
        evento.TipoDocumento.Should().Be("NFSe");
        evento.DocumentoFiscalId.Should().Be(documentoNFSe.Id);
        evento.ValorTotal.Should().Be(850.25m);
    }

    [Test]
    public void Constructor_WithZeroValues_ShouldMapCorrectly()
    {
        // Arrange
        var documento = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ValorTotal = 0m,
            TipoDocumento = TipoDocumentoFiscal.NFe
        };

        // Act
        var evento = new DocumentoFiscalCriado(documento);

        // Assert
        evento.ValorTotal.Should().Be(0m);
        evento.TipoDocumento.Should().Be("NFe");
    }

    [Test]
    public void ParameterlessConstructor_ShouldCreateValidInstance()
    {
        // Act
        var evento = new DocumentoFiscalCriado();

        // Assert
        evento.Should().NotBeNull();
        evento.DocumentoFiscalId.Should().Be(Guid.Empty);
        evento.ChaveAcesso.Should().BeNull();
        evento.DocumentoEmissor.Should().BeNull();
        evento.DocumentoDestinatario.Should().BeNull();
        evento.TipoDocumento.Should().BeNull();
        evento.ValorTotal.Should().Be(0m);
        evento.Data.Should().Be(DateTime.MinValue);
        evento.CriadoEm.Should().Be(DateTime.MinValue);
    }

    [Test]
    public void Constructor_WithCompleteDocumento_ShouldPreserveAllData()
    {
        // Arrange
       
        var documento = new DocumentoFiscal
        {
            Id = new Guid("12345678-1234-1234-1234-123456789012"),
            ChaveAcesso = "35202401013456789000100550010000001234567890",
            Data = new DateTime(2024, 1, 15, 14, 30, 45),
            DocumentoEmissor = "01345678000100",
            DocumentoDestinatario = "98765432100",
            ValorTotal = 9875.43m,            
            TipoDocumento = TipoDocumentoFiscal.NFe
        };

        // Act
        var evento = new DocumentoFiscalCriado(documento);

        // Assert
        evento.DocumentoFiscalId.Should().Be(new Guid("12345678-1234-1234-1234-123456789012"));
        evento.ChaveAcesso.Should().Be("35202401013456789000100550010000001234567890");
        evento.Data.Should().Be(new DateTime(2024, 1, 15, 14, 30, 45));
        evento.DocumentoEmissor.Should().Be("01345678000100");
        evento.DocumentoDestinatario.Should().Be("98765432100");
        evento.ValorTotal.Should().Be(9875.43m);    
        evento.TipoDocumento.Should().Be("NFe");
    }

    [Test]
    public void Constructor_WithLargeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var documento = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ValorTotal = 999999999.99m,
            TipoDocumento = TipoDocumentoFiscal.CTe
        };

        // Act
        var evento = new DocumentoFiscalCriado(documento);

        // Assert
        evento.ValorTotal.Should().Be(999999999.99m);
        evento.TipoDocumento.Should().Be("CTe");
    }
}