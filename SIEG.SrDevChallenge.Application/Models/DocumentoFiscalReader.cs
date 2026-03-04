using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using SIEG.SrDevChallenge.Domain.Enums;

namespace SIEG.SrDevChallenge.Application.Models;

public class DocumentoFiscalReader : IDisposable
{   
    public TipoDocumentoFiscal TipoDocumento { get;}
    public XmlReader XmlReader {get; protected set;} = default!;
    public string HashXml {get;}
    public string XmlOriginal {get;}    
    private static TipoDocumentoFiscal IdentificaTipoDocumentoFiscal(XmlReader reader)
    {
        var rootLocal = reader.LocalName;
        var ns = reader.NamespaceURI ?? "";

        if(rootLocal is "NFe" or "enviNFe" or "procNFe" or "nfeProc")
            return TipoDocumentoFiscal.NFe;

        if(rootLocal is "CTe" or "enviCTe" or "cteProc" or "procCTe")
            return TipoDocumentoFiscal.CTe;    

        if (ns.Contains("nfe", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.NFe;

        if (ns.Contains("cte", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.CTe;

        if (ns.Contains("nfse", StringComparison.OrdinalIgnoreCase) ||
            ns.Contains("abrasf", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.NFSe;

        throw new ArgumentException("Não foi possivel identificar o tipo do arquivo xml");
    }
    private readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreWhitespace = true
    };
    public DocumentoFiscalReader(string  xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML não pode ser vazio.", nameof(xml));

        XmlOriginal = xml;
        using (var tempReader = XmlReader.Create(new StringReader(xml), _readerSettings))
        {
            tempReader.MoveToContent();

            TipoDocumento = IdentificaTipoDocumentoFiscal(tempReader);         
        }
        //Validar XML
        XmlReader = XmlReader.Create(new StringReader(xml), _readerSettings);
        HashXml = GenerateHashXml(xml);
    }
    private static string GenerateHashXml(string xml)
    {
        var doc = new XmlDocument
        {
            PreserveWhitespace = true
        };

        doc.LoadXml(xml);
  
        var transform = new XmlDsigC14NTransform();
        transform.LoadInput(doc);

        using var stream = (Stream)transform.GetOutput(typeof(Stream));
        using var sha = SHA256.Create();

        var hash = sha.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }
    public void Dispose()
    {
        XmlReader.Dispose();
    }
}
