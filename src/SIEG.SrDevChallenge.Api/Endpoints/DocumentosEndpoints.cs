using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;
using SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.RemoveDocumentoFIscal;
using SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.UpdateDocumentoFiscal;
using SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentListBy;
using SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentoFiscal;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Enums;
using Sprache;

namespace SIEG.SrDevChallenge.Api.Endpoints;

public static class DocumentosEndpoints
{
    public static void MapDocumentosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/documentos-fiscais")
            .WithTags("Documentos Fiscais")
            .WithDescription("Endpoints para gerenciamento de documentos fiscais eletrônicos")
            .WithOpenApi();

        group.DisableAntiforgery();

      
        group.MapPost("/", UploadXML)
            .WithName("CreateDocumentoFiscal")
            .WithSummary("Upload de arquivo XML para criação de documento fiscal")
            .WithDescription("""
                Envia um arquivo XML contendo os dados do documento fiscal a ser criado. Suporta NFe, NFSe e CTe.
                
                **Tipos de documento suportados:**
                - NFe (Nota Fiscal Eletrônica)
                - NFSe (Nota Fiscal de Serviços Eletrônica)  
                - CTe (Conhecimento de Transporte Eletrônico)
                
                **Validações realizadas:**
                - Formato do arquivo deve ser XML
                - Content-Type deve ser 'application/xml'
                - XML deve estar em formato válido
                - Chave de acesso deve ser única
                
                **Rate Limiting:** Limitado a 100 requests por minuto
                """)
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<Result<Guid>>(200)
            .Produces<ValidationProblemDetails>(400)
            .Produces(401)
            .Produces(429)
            .Produces(500)
            .RequireRateLimiting("fixed");

        
        group.MapGet("/", GetPagedList)
            .WithName("GetDocumentosFiscaisList")
            .WithSummary("Consulta de documentos fiscais com filtros e paginação")
            .WithDescription("""
                Permite consultar a lista de documentos fiscais aplicando filtros por data, emissor, 
                destinatário e tipo de documento. Os resultados são paginados.
                
                **Filtros disponíveis:**
                - Data de início e fim (obrigatório)
                - Documento do emissor (obrigatório)
                - Documento do destinatário (opcional)
                - Tipo de documento fiscal (opcional: 1-NFe, 2-NFSe, 3-CTe)
                
                **Paginação:**
                - PageNumber: Número da página (padrão: 1)
                - PageSize: Itens por página (padrão: 10, máximo: 100)
                """)
            .Produces<PagedResult<object>>(200)
            .Produces<ProblemDetails>(400)
            .Produces(401)
            .Produces(500);

       
        group.MapGet("/{id:guid}", GetById)
            .WithName("GetDocumentoFiscalById")
            .WithSummary("Consulta de documento fiscal por ID")
            .WithDescription("""
                Permite consultar os detalhes completos de um documento fiscal específico utilizando seu ID único.
                
                **Informações retornadas:**
                - Dados completos do documento fiscal
                - Informações do emissor e destinatário
                - Itens do documento
                - Valores e totais
                - Dados de auditoria (criação/atualização)
                """)
            .Produces<Result<object>>(200)
            .Produces(404)
            .Produces(401)
            .Produces(500);

    
        group.MapDelete("/{id:guid}", Remove)
            .WithName("RemoveDocumentoFiscal")
            .WithSummary("Remoção de documento fiscal por ID")
            .WithDescription("""
                Permite remover um documento fiscal específico utilizando seu ID único.
                
                **ATENÇÃO:** Esta operação é irreversível e deve ser utilizada com cautela.
                
                **Rate Limiting:** Limitado a 100 requests por minuto
                """)
            .Produces<Result<bool>>(200)
            .Produces(404)
            .Produces(401)
            .Produces(429)
            .Produces(500)
            .RequireRateLimiting("fixed");

        group.MapPut("/{id:guid}", UpdateDocument)
            .WithName("UpdateDocumentoFiscal")
            .WithSummary("Atualização de documento fiscal por ID")
            .WithDescription("""
                Permite atualizar um documento fiscal existente utilizando seu ID único e enviando um novo arquivo XML.
                
                **Validações realizadas:**
                - Documento deve existir
                - Formato do arquivo deve ser XML
                - Content-Type deve ser 'application/xml'
                - XML deve estar em formato válido
                
                **Rate Limiting:** Limitado a 100 requests por minuto
                """)
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<Result<bool>>(200)
            .Produces<ProblemDetails>(400) 
            .Produces(404)
            .Produces(401)
            .Produces(429)
            .Produces(500)
            .RequireRateLimiting("fixed");
    }

    private static async Task<IResult> Remove([FromServices] IMediator mediatr, [FromRoute] Guid id)
    {
        var query = new RemoveDocumentoFiscalCommand(id);
        var result = await mediatr.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById([FromServices] IMediator mediatr, [FromRoute] Guid id)
    {
        var query = new GetDocumentoFiscalQuery(id);
        var result = await mediatr.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPagedList(
        [FromServices] IMediator mediatr,
        [FromQuery] DateTime DataInicio,
        [FromQuery] DateTime DataFim, [FromQuery] string DocumentoEmissor,
        [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10,
        [FromQuery] string? DocumentoDestinatario = default, [FromQuery] TipoDocumentoFiscal? TipoDocumento = default
        )
    {
        var query = new GetDocumentListByQuery(DataInicio, DataFim, DocumentoEmissor, PageNumber, PageSize, DocumentoDestinatario, TipoDocumento);
        var result = await mediatr.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> UploadXML([FromForm] IFormFile file, IMediator mediatr)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new ProblemDetails { Title = "Arquivo XML não enviado." });

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new ProblemDetails { Title = "Arquivo deve ser XML." });

        var allowedMimeTypes = new[] { "application/xml" };
        if (!allowedMimeTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new ProblemDetails { Title = "Tipo MIME não permitido." });
        }
        string xml;

        using var reader = new StreamReader(file.OpenReadStream());
        xml = await reader.ReadToEndAsync();

        var result = await mediatr.Send(new CreateDocumentoFiscalCommand(xml));
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateDocument([FromRoute] Guid id, [FromForm] IFormFile file, IMediator mediatr)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new ProblemDetails { Title = "Arquivo XML não enviado." });

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new ProblemDetails { Title = "Arquivo deve ser XML." });

        var allowedMimeTypes = new[] { "application/xml" };
        if (!allowedMimeTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new ProblemDetails { Title = "Tipo MIME não permitido." });
        }

        string xml;
        using var reader = new StreamReader(file.OpenReadStream());
        xml = await reader.ReadToEndAsync();

        var command = new UpdateDocumentoFiscalCommand(id, xml);
        var result = await mediatr.Send(command);
        return Results.Ok(result);
    }
}
