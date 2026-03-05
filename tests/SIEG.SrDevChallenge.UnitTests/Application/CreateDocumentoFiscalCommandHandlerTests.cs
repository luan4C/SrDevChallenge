using FluentAssertions;
using Moq;
using NUnit.Framework;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Domain.Exceptions;

namespace SIEG.SrDevChallenge.UnitTests.Application;

[TestFixture]
public class CreateDocumentoFiscalCommandHandlerTests
{
    private Mock<IDocumentoFiscalRepository> _repositoryMock;
    private Mock<IDocumentSchemaValidator> _validatorMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private CreateDocumentoFiscalCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IDocumentoFiscalRepository>();
        _validatorMock = new Mock<IDocumentSchemaValidator>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        
        _handler = new CreateDocumentoFiscalCommandHandler(
            _repositoryMock.Object,
            _validatorMock.Object,
            _eventPublisherMock.Object);
    }

    [Test]
    public async Task Handle_ValidProcNFe_ShouldCreateDocumentoFiscal()
    {
        // Arrange
        var validProcNFeXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                    <infNFe Id="NFe35200314200166000187550010000000046550010008">
                        <ide>
                            <cUF>35</cUF>
                            <cNF>55001000</cNF>
                            <natOp>VENDA DE MERCADORIA</natOp>
                            <mod>55</mod>
                            <serie>1</serie>
                            <nNF>4</nNF>
                            <dhEmi>2020-03-14T10:30:00-03:00</dhEmi>
                            <tpNF>1</tpNF>
                            <idDest>1</idDest>
                            <cMunFG>3550308</cMunFG>
                            <tpImp>1</tpImp>
                            <tpEmis>1</tpEmis>
                            <cDV>8</cDV>
                            <tpAmb>2</tpAmb>
                            <finNFe>1</finNFe>
                            <indFinal>1</indFinal>
                            <indPres>9</indPres>
                            <procEmi>0</procEmi>
                            <verProc>4.00</verProc>
                        </ide>
                        <emit>
                            <CNPJ>14200166000187</CNPJ>
                            <xNome>EMPRESA TESTE LTDA</xNome>
                            <enderEmit>
                                <xLgr>RUA DO COMERCIO</xLgr>
                                <nro>123</nro>
                                <xBairro>CENTRO</xBairro>
                                <cMun>3550308</cMun>
                                <xMun>SAO PAULO</xMun>
                                <UF>SP</UF>
                                <CEP>01234567</CEP>
                            </enderEmit>
                        </emit>
                        <dest>
                            <CPF>12345678901</CPF>
                            <xNome>CLIENTE TESTE</xNome>
                        </dest>
                        <det nItem="1">
                            <prod>
                                <cProd>001</cProd>
                                <xProd>PRODUTO TESTE</xProd>
                                <vProd>1500.00</vProd>
                            </prod>
                        </det>
                        <total>
                            <ICMSTot>
                                <vNF>1500.00</vNF>
                            </ICMSTot>
                        </total>
                    </infNFe>
                </NFe>
                <protNFe xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                    <infProt>
                        <tpAmb>2</tpAmb>
                        <chNFe>35200314200166000187550010000000046550010008</chNFe>
                        <dhRecbto>2020-03-14T10:35:00-03:00</dhRecbto>
                        <nProt>135200000000001</nProt>
                        <cStat>100</cStat>
                        <xMotivo>Autorizado o uso da NF-e</xMotivo>
                    </infProt>
                </protNFe>
            </nfeProc>
            """;

        var command = new CreateDocumentoFiscalCommand(validProcNFeXml);
  
        var validationResult = new DocumentSchemaValidationResult();
        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<TipoDocumentoFiscal>()))
            .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscal>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
            
        result.Message.Should().Be("Documento fiscal criado com sucesso.");

        _repositoryMock.Verify(r => r.AddAsync(It.Is<DocumentoFiscal>(df => 
            df.ChaveAcesso == "35200314200166000187550010000000046550010008" &&
            df.DocumentoEmissor == "14200166000187" &&
            df.DocumentoDestinatario == "12345678901" &&
            df.ValorTotal == 1500.00m &&
            df.TipoDocumento == TipoDocumentoFiscal.NFe &&
            df.TipoEmissor == TipoPessoaFiscal.PJ &&
            df.TipoDestinatario == TipoPessoaFiscal.PF
        )), Times.Once);

        _eventPublisherMock.Verify(e => e.PublishAsync(
            It.IsAny<SIEG.SrDevChallenge.Domain.Events.DocumentoFiscalCriado>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_InvalidProcNFe_ShouldThrowValidationException()
    {
        // Arrange
        var invalidXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                    <infNFe Id="NFe_INVALID">
                        <!-- XML inválido sem campos obrigatórios -->
                    </infNFe>
                </NFe>
            </nfeProc>
            """;

        var command = new CreateDocumentoFiscalCommand(invalidXml);

         var validationResult = new DocumentSchemaValidationResult();
         validationResult.Errors.Add("Campo obrigatório 'emit/CNPJ' não encontrado");
        validationResult.Errors.Add("Campo obrigatório 'total/ICMSTot/vNF' inválido");
        
        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<TipoDocumentoFiscal>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("XML inválido para NFe");
        exception.Errors.Should().ContainKey("Validacoes");
        exception.Errors["Validacoes"].Should().Contain("Campo obrigatório 'emit/CNPJ' não encontrado");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<DocumentoFiscal>()), Times.Never);
    }

    [Test]
    public async Task Handle_DuplicateProcNFe_ShouldReturnExistingId()
    {
        // Arrange
        var duplicateXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                    <infNFe Id="NFe35201234567890123456789012345678901234567890">
                        <ide>
                            <dhEmi>2024-03-05T14:30:00-03:00</dhEmi>
                        </ide>
                        <emit>
                            <CNPJ>12345678000195</CNPJ>
                        </emit>
                        <dest>
                            <CPF>98765432100</CPF>
                        </dest>
                        <total>
                            <ICMSTot>
                                <vNF>2500.75</vNF>
                            </ICMSTot>
                        </total>
                    </infNFe>
                </NFe>
                <protNFe xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                    <infProt>
                        <chNFe>35201234567890123456789012345678901234567890</chNFe>
                        <cStat>100</cStat>
                        <xMotivo>Autorizado o uso da NF-e</xMotivo>
                    </infProt>
                </protNFe>
            </nfeProc>
            """;

        var command = new CreateDocumentoFiscalCommand(duplicateXml);
        var validationResult = new DocumentSchemaValidationResult();


        var existingDocument = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = "35201234567890123456789012345678901234567890",
            HashXml = "existing_hash"
        };
        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<TipoDocumentoFiscal>()))
            .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscal>()))
            .ThrowsAsync(new ConflictException("Documento fiscal com este hash já existe"));
            
        _repositoryMock.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(existingDocument);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Item.Should().Be(existingDocument.Id);
        result.Message.Should().Be("Documento fiscal já existe.");
    }

    [Test]
    public async Task Handle_ProcNFeWithCnpjDestinatario_ShouldHandleCorrectly()
    {
        // Arrange
        var xmlWithCnpjDest = """
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                    <infNFe Id="NFe35200314200166000187550010000000046550010008">
                        <ide>
                            <dhEmi>2020-03-14T10:30:00-03:00</dhEmi>
                        </ide>
                        <emit>
                            <CNPJ>14200166000187</CNPJ>
                        </emit>
                        <dest>
                            <CNPJ>98765432000123</CNPJ>
                        </dest>
                        <total>
                            <ICMSTot>
                                <vNF>3200.50</vNF>
                            </ICMSTot>
                        </total>
                    </infNFe>
                </NFe>
                <protNFe xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
                    <infProt>
                        <chNFe>35200314200166000187550010000000046550010008</chNFe>
                        <cStat>100</cStat>
                        <xMotivo>Autorizado o uso da NF-e</xMotivo>
                    </infProt>
                </protNFe>
            </nfeProc>
            """;

        var command = new CreateDocumentoFiscalCommand(xmlWithCnpjDest);
        var validationResult = new DocumentSchemaValidationResult();
        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<TipoDocumentoFiscal>()))
            .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<DocumentoFiscal>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();       

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<DocumentoFiscal>()), Times.Once);
    }
}