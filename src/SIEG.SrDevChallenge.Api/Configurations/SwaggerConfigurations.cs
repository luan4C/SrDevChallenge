using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class SwaggerConfigurations
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "SIEG - Documentos Fiscais API",
                Description = "API para gerenciamento de documentos fiscais eletrônicos",               
            });
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key necessária para acessar os endpoints. Use o header: X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
            {
                [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
            });
                

            // Configura o swagger para usar o OpenAPI 3.0
            c.EnableAnnotations();
            
            // Configurações de filtros personalizados
            c.SchemaFilter<EnumSchemaFilter>();
            c.DocumentFilter<SwaggerDocumentFilter>();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIEG Documentos Fiscais API v1");
                c.RoutePrefix = "swagger";
                c.DisplayRequestDuration();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(2);
                c.DefaultModelExpandDepth(2);
                c.EnableDeepLinking();
                c.ShowExtensions();
            });
        }

        return app;
    }
}

// Filtro para melhorar a documentação de enums
public class EnumSchemaFilter : ISchemaFilter
{    

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);
            
            for (int i = 0; i < enumNames.Length; i++)
            {
                schema.Enum.Add($"{Convert.ToInt32(enumValues.GetValue(i))} - {enumNames[i]}");
            }
        }
    }
}

// Filtro para personalizar a documentação
public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove esquemas não utilizados
        var schemas = swaggerDoc.Components.Schemas.Where(x => !x.Key.StartsWith("Microsoft.")).ToList();
        swaggerDoc.Components.Schemas.Clear();
        foreach (var schema in schemas)
        {
            swaggerDoc.Components.Schemas.Add(schema.Key, schema.Value);
        }
    }
}