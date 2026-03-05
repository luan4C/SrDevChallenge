using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using SIEG.SrDevChallenge.CrossCutting.Helpers;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Domain.Exceptions;


namespace SIEG.SrDevChallenge.Application.Models;

public class DocumentoFiscalReader : IDisposable
{
    public XmlReader XmlReader { get; protected set; } = default!;
    public string HashXml { get; }
    public string XmlOriginal { get; }
    public DocumentoFiscalMetadata Metadata { get; }
    private static TipoDocumentoFiscal IdentificaTipoDocumentoFiscal(XmlReader reader)
    {
        var rootLocal = reader.LocalName;
        var ns = reader.NamespaceURI ?? "";

        if (rootLocal is "NFe" or "enviNFe" or "procNFe" or "nfeProc")
            return TipoDocumentoFiscal.NFe;

        if (rootLocal is "CTe" or "enviCTe" or "cteProc" or "procCTe")
            return TipoDocumentoFiscal.CTe;

        if (ns.Contains("nfe", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.NFe;

        if (ns.Contains("cte", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.CTe;

        if (ns.Contains("nfse", StringComparison.OrdinalIgnoreCase) ||
            ns.Contains("abrasf", StringComparison.OrdinalIgnoreCase))
            return TipoDocumentoFiscal.NFSe;

        throw new ValidationException("Estrutura inválida", new Dictionary<string, string[]>
        {
            { "DocumentoFiscal", _invalidFormatMessage }
        });
    }

    private DocumentoFiscalMetadata ExtractMetadata(string xml)
    {
        var metadata = new DocumentoFiscalMetadata();

        using var reader = XmlReader.Create(new StringReader(xml), _readerSettings);
        reader.MoveToContent();

        metadata.TipoDocumento = IdentificaTipoDocumentoFiscal(reader);

        int emitDepth = -1;
        int destDepth = -1;


        int icmsTotDepth = -1;
        int vPrestDepth = -1;
        int totalDepth = -1;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "vNF")
                {
                    var s = reader.ReadElementContentAsString();

                    if (DecimalHelpers.TryParseDecimalCultureAgnostic(s, out var v))
                    {
                        metadata.ValorTotal = v;
                        break;
                    }
                }

                if (reader.LocalName == "emit") emitDepth = reader.Depth;
                if (reader.LocalName == "dest") destDepth = reader.Depth;
                if (reader.LocalName == "total")
                    totalDepth = reader.Depth;

                if (reader.LocalName == "ICMSTot" &&
                    totalDepth != -1 &&
                    reader.Depth > totalDepth)
                {
                    icmsTotDepth = reader.Depth;
                }

                if (reader.LocalName == "vPrest")
                    vPrestDepth = reader.Depth;

                switch (reader.LocalName)
                {
                    case "infNFe":
                    case "infCte":
                        var id = reader.GetAttribute("Id");
                        if (!string.IsNullOrEmpty(id))
                            metadata.ChaveAcesso = id.Replace("NFe", "").Replace("CTe", "");
                        break;

                    case "dhEmi":
                    case "dEmi":
                        var data = reader.ReadElementContentAsString();
                        if (DateTime.TryParse(data, out var parsed))
                            metadata.DataEmissao ??= parsed;
                        break;

                    case "CNPJ":
                    case "CPF":
                        var tipoPessoa = reader.LocalName == "CNPJ" ? TipoPessoaFiscal.PJ : TipoPessoaFiscal.PF;
                        var doc = reader.ReadElementContentAsString();

                        var insideEmit = emitDepth != -1 && reader.Depth > emitDepth;
                        var insideDest = destDepth != -1 && reader.Depth > destDepth;

                        if (insideEmit && metadata.DocumentoEmitente == null)
                        {
                            metadata.DocumentoEmitente = doc;
                            metadata.TipoEmitente = tipoPessoa;
                        }
                        else if (insideDest && metadata.DocumentoDestinatario == null)
                        {
                            metadata.DocumentoDestinatario = doc;
                            metadata.TipoDestinatario = tipoPessoa;
                        }
                        break;
                    case "vNF":
                        if (metadata.TipoDocumento == TipoDocumentoFiscal.NFe &&
                            metadata.ValorTotal == default &&
                            icmsTotDepth != -1 &&
                            reader.Depth > icmsTotDepth)
                        {
                            var s = reader.ReadElementContentAsString();

                            if (DecimalHelpers.TryParseDecimalCultureAgnostic(s, out var v))
                                metadata.ValorTotal = v;
                        }
                        break;


                    case "vPrest":
                        vPrestDepth = reader.Depth;
                        break;

                    case "vTPrest":
                        if (metadata.TipoDocumento == TipoDocumentoFiscal.CTe &&
                            metadata.ValorTotal == default &&
                            vPrestDepth != -1 && reader.Depth > vPrestDepth)
                        {
                            var s = reader.ReadElementContentAsString();
                            if (DecimalHelpers.TryParseDecimalCultureAgnostic(s, out var v))
                                metadata.ValorTotal = v;
                        }
                        break;


                    case "ValorServicos":
                    case "ValorLiquidoNfse":
                    case "ValorNfse":
                        if (metadata.TipoDocumento == TipoDocumentoFiscal.NFSe && metadata.ValorTotal == null)
                        {
                            var s = reader.ReadElementContentAsString();
                            if (DecimalHelpers.TryParseDecimalCultureAgnostic(s, out var v))
                                metadata.ValorTotal = v;
                        }
                        break;
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {

                if (emitDepth != -1 && reader.LocalName == "emit" && reader.Depth == emitDepth)
                    emitDepth = -1;

                if (destDepth != -1 && reader.LocalName == "dest" && reader.Depth == destDepth)
                    destDepth = -1;

                if (icmsTotDepth != -1 && reader.LocalName == "ICMSTot" && reader.Depth == icmsTotDepth)
                    icmsTotDepth = -1;

                if (vPrestDepth != -1 && reader.LocalName == "vPrest" && reader.Depth == vPrestDepth)
                    vPrestDepth = -1;
            }

            if (metadata.DataEmissao != null &&
                metadata.DocumentoEmitente != null &&
                metadata.DocumentoDestinatario != null &&
                (metadata.TipoDocumento == TipoDocumentoFiscal.NFSe || metadata.ChaveAcesso != null) &&
                metadata.ValorTotal != default)
            {
                break;
            }
        }
        return metadata;
    }
    private readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreWhitespace = true
    };
    private static readonly string[] _invalidFormatMessage = ["Tipo de documento fiscal não identificado. Verifique se o XML é válido e se segue os padrões de NFe, CTe ou NFSe."];

    public DocumentoFiscalReader(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML não pode ser vazio.", nameof(xml));

        XmlOriginal = xml;
        using (var tempReader = XmlReader.Create(new StringReader(xml), _readerSettings))
        {
            tempReader.MoveToContent();

            Metadata = ExtractMetadata(xml);
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

public class DocumentoFiscalMetadata
{
    public string? ChaveAcesso { get; set; }
    public TipoPessoaFiscal TipoEmitente { get; set; }
    public string? DocumentoEmitente { get; set; }
    public TipoPessoaFiscal TipoDestinatario { get; set; }
    public string? DocumentoDestinatario { get; set; }
    public DateTime? DataEmissao { get; set; }
    public decimal ValorTotal { get; set; }
    public TipoDocumentoFiscal TipoDocumento { get; set; }
}