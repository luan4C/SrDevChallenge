using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SIEG.SrDevChallenge.Application.features.Commands.DocumentoFiscal.CreateDocumentoFiscal;

namespace SIEG.SrDevChallenge.Api.Endpoints;

public static class DocumentosEndpoints
{
    public static void MapDocumentosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/documentos-fiscais");
        group.DisableAntiforgery();
        group.MapPost("/", UploadXML).AllowAnonymous();
    }

    private static async Task<IResult> UploadXML([FromForm] IFormFile file, IMediator mediatr)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Arquivo XML não enviado.");

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Arquivo deve ser XML.");  

            string xml;

        using var reader = new StreamReader(file.OpenReadStream());
        xml = await reader.ReadToEndAsync();

        await mediatr.Send(new CreateDocumentoFiscalCommand(xml));
        return Results.Accepted(value: "Documento salvo com sucesso!");
    }
}
