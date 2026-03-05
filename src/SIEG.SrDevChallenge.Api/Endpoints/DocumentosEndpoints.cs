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
            .WithDescription("Endpoints para gerenciamento de documentos fiscais");
            
        group.DisableAntiforgery();
        
        group.MapPost("/", UploadXML).AddOpenApiOperationTransformer((operation,context, _) =>
        {
            operation.Summary = "Upload de arquivo XML para criação de documento fiscal";
            operation.Description = "Envia um arquivo XML contendo os dados do documento fiscal a ser criado. O arquivo deve estar no formato correto para que o documento seja processado e armazenado.";
            return Task.CompletedTask;
        });
            
        group.MapGet("/", GetPagedList).AddOpenApiOperationTransformer((operation,context , _) =>
        {
            operation.Summary = "Consulta de documentos fiscais com filtros e paginação";
            operation.Description = "Permite consultar a lista de documentos fiscais com base em filtros como data de emissão, emissor, destinatário e tipo de documento. Os resultados são paginados para facilitar a navegação.";
            return Task.CompletedTask;
        });
            
        group.MapGet("/{id:guid}", GetById).AddOpenApiOperationTransformer((operation, context,_) =>
        {
            operation.Summary = "Consulta de documento fiscal por ID";
            operation.Description = "Permite consultar os detalhes de um documento fiscal específico utilizando seu ID único (GUID). Retorna as informações completas do documento fiscal, incluindo dados do emissor, destinatário, itens e valores.";
            return Task.CompletedTask;
        }); 
            
        group.MapDelete("/{id:guid}", Remove).AddOpenApiOperationTransformer((operation, context, _) =>
        {
            operation.Summary = "Remoção de documento fiscal por ID";
            operation.Description = "Permite remover um documento fiscal específico utilizando seu ID único (GUID). Esta operação é irreversível e deve ser utilizada com cautela.";
            return Task.CompletedTask;
        });
        
        group.MapPut("/{id:guid}", UpdateDocument).AddOpenApiOperationTransformer((operation, context, _) =>
        {
            operation.Summary = "Atualização de documento fiscal por ID";
            operation.Description = "Permite atualizar um documento fiscal existente utilizando seu ID único (GUID) e enviando um novo arquivo XML. O documento será validado antes da atualização.";
            return Task.CompletedTask;
        });
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

    private static async Task<IResult> UploadXML([FromForm]  IFormFile file, IMediator mediatr)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Arquivo XML não enviado.");

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Arquivo deve ser XML.");  

            string xml;

        using var reader = new StreamReader(file.OpenReadStream());
        xml = await reader.ReadToEndAsync();

        var result = await mediatr.Send(new CreateDocumentoFiscalCommand(xml));
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateDocument([FromRoute] Guid id, [FromForm] IFormFile file, IMediator mediatr)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Arquivo XML não enviado.");

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Arquivo deve ser XML.");

        string xml;
        using var reader = new StreamReader(file.OpenReadStream());
        xml = await reader.ReadToEndAsync();

        var command = new UpdateDocumentoFiscalCommand(id, xml);
        var result = await mediatr.Send(command);
        return Results.Ok(result);
    }
}
