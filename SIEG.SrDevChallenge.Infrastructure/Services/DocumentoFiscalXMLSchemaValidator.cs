using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Infrastructure.Services;

public class DocumentoFiscalXMLSchemaValidator : IDocumentSchemaValidator
{       
    public async Task<DocumentSchemaValidationResult> ValidateAsync(string xmlContent, TipoDocumentoFiscal? tipoDocumento = null)
    {
        
        DocumentoFiscalReader reader = new (xmlContent);
        tipoDocumento ??= reader.TipoDocumento;
        var result = new DocumentSchemaValidationResult();
        var schemas = GetSchema(tipoDocumento.Value);

        var settings = new XmlReaderSettings
        { 
            XmlResolver = null,
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
            ValidationFlags =
                XmlSchemaValidationFlags.ReportValidationWarnings |
                XmlSchemaValidationFlags.ProcessIdentityConstraints |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation
        };

        settings.ValidationEventHandler += (_, e) =>
        {
            result.Errors.Add("{e.Severity}: {e.Message}");
        };

        using var validationReader = XmlReader.Create(new StringReader(reader.XmlOriginal), settings);

        // Consumir o reader dispara a validação
        while (await validationReader.ReadAsync()) { }

        return result;
    }

    private XmlSchemaSet GetSchema(TipoDocumentoFiscal tipoDocumento)
    {
        var rootpath = Path.Combine(AppContext.BaseDirectory, "Schemas");
        var path = tipoDocumento switch
        {
            TipoDocumentoFiscal.NFe => Path.Combine(rootpath, "NFe-4.0", "nfe_v4.00.xsd"),
            TipoDocumentoFiscal.CTe => Path.Combine(rootpath, "CTe-4.0", "cte_v4.00.xsd"),
            TipoDocumentoFiscal.NFSe => Path.Combine(rootpath, "NFSe-1.0", "NFSe_v1.00.xsd"),
            _ => throw new ArgumentException("Parâmentro inválido ao buscar esquema de validação")
        };

        var schemaSet = new XmlSchemaSet { XmlResolver = null};

        using var xsd = XmlReader.Create(path, new() {XmlResolver = null});
        schemaSet.Add(null, xsd);
        schemaSet.Compile();
        return schemaSet;
    }

}
