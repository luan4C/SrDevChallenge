using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NUnit.Framework;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Application.features.Queries.DocumentoFiscal.GetDocumentoFiscal;
using SIEG.SrDevChallenge.Application.Models;
using SIEG.SrDevChallenge.Domain.Entities;
using SIEG.SrDevChallenge.Domain.Enums;
using SIEG.SrDevChallenge.Infrastructure.IoC;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SIEG.SrDevChallenge.IntegrationTests.Api;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            
            config.Sources.Clear();
            
        
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Mongo"] = "mongodb://localhost:27017/testdb",
                ["RabbitMqConfigurations:Host"] = "localhost",
                ["RabbitMqConfigurations:Port"] = "5672",
                ["RabbitMqConfigurations:User"] = "guest",
                ["RabbitMqConfigurations:Password"] = "guest",
                ["ApiKeySettings:ValidApiKeys:0:Key"] = "sieg-dev-api-key-2026",
                ["ApiKeySettings:ValidApiKeys:0:Name"] = "Test Key",
                ["ApiKeySettings:ValidApiKeys:0:IsActive"] = "true",
                ["ApiKeySettings:ValidApiKeys:0:Description"] = "Test API Key"
            });
        });
        
        builder.ConfigureServices(services =>
        {
            // Registra implementações de teste
            services.AddSingleton<IDocumentoFiscalRepository, InMemoryDocumentoFiscalRepository>();
            services.AddSingleton<IDocumentoFiscaisResumoMensalRepository, InMemoryDocumentoFiscaisResumoMensalRepository>();
            
            // Registra mock do IEventPublisher para testes
            services.AddSingleton<IEventPublisher, MockEventPublisher>();
        });
    }
}

[TestFixture]
public class DocumentosEndpointsTests
{
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;
    private IDocumentoFiscalRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _repository = _factory.Services.GetRequiredService<IDocumentoFiscalRepository>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        
        if (_repository is InMemoryDocumentoFiscalRepository inMemoryRepo)
        {
            inMemoryRepo.Clear();
        }
    }

    [Test]
    public async Task POST_DocumentosFiscais_ValidNFe_ShouldReturnCreated()
    {
        // Arrange
        var nfeXml = GetValidProcNFeXml();
        
        // Cria um arquivo temporário XML
        var xmlBytes = Encoding.UTF8.GetBytes(nfeXml);
        
        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(xmlBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        formData.Add(fileContent, "file", "nfe.xml");
        
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");

        // Act
        var response = await _client.PostAsync("/api/documentos-fiscais", formData);
       
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Error: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("item").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("message").GetString().Should().Be("Documento fiscal criado com sucesso.");
  
    }

    [Test]
    public async Task POST_DocumentosFiscais_InvalidXml_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidXml = "<xml>invalid</xml>";
       // Cria um arquivo temporário XML
        var xmlBytes = Encoding.UTF8.GetBytes(invalidXml);
        
        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(xmlBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        formData.Add(fileContent, "file", "invalid.xml");;
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");

        // Act
        var response = await _client.PostAsync("/api/documentos-fiscais", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent);

        problemDetails?.Errors.Should().ContainKey("DocumentoFiscal");
    }

    [Test]
    public async Task POST_DocumentosFiscais_MissingApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var nfeXml = GetValidProcNFeXml();
        var content = new StringContent(nfeXml, Encoding.UTF8, "application/xml");

        _client.DefaultRequestHeaders.Clear();     

        // Act
        var response = await _client.PostAsync("/api/documentos-fiscais", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task POST_DocumentosFiscais_InvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var nfeXml = GetValidProcNFeXml();
        var content = new StringContent(nfeXml, Encoding.UTF8, "application/xml");
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "invalid-key");

        // Act
        var response = await _client.PostAsync("/api/documentos-fiscais", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GET_DocumentosFiscais_WithValidData_ShouldReturnPaginatedResults()
    {
        // Arrange - Adicionar alguns documentos
        await SeedDocumentosAsync();
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");
      var dateQuery = DateTime.Now.ToString("yyyy-MM-dd");
        // Act
        var response = await _client
        .GetAsync($"/api/documentos-fiscais?documentoEmissor=14200166000187&datainicio={dateQuery}&datafim={dateQuery}&pagenumber=1&pagesize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        result.GetProperty("page").GetInt32().Should().Be(1);
        result.GetProperty("pageSize").GetInt32().Should().Be(5);
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GET_DocumentosFiscais_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedDocumentosAsync();
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");
        var dateQuery = DateTime.Now.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/documentos-fiscais?tipoDocumento=1&documentoEmissor=14200166000187&datainicio={dateQuery}&datafim={dateQuery}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        var items = result.GetProperty("items").EnumerateArray();
        foreach (var item in items)
        {
            item.GetProperty("tipoDocumento").GetUInt32().Should().Be(1);
        }
    }

    [Test]
    public async Task GET_DocumentosFiscais_ById_ExistingDocument_ShouldReturnDocument()
    {
        // Arrange
        var documento = new DocumentoFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = "35240314200166000187650010000001231234567890",
            Data = DateTime.Now,
            ValorTotal = 15000.50m,
            TipoDocumento = TipoDocumentoFiscal.NFe,
            XMLOriginal = GetValidProcNFeXml(),
            HashXml = "hash123",
            DocumentoEmissor = "14200166000187",
            DocumentoDestinatario = "12345678901",
            TipoEmissor = TipoPessoaFiscal.PJ,
            TipoDestinatario = TipoPessoaFiscal.PF
        };

        await _repository.AddAsync(documento);
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");

        // Act
        var response = await _client.GetAsync($"/api/documentos-fiscais/{documento.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Application.Models.Result<DocumentoFiscalDetailsDTO>>(responseContent, 
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
        result.Item.Id.Should().Be(documento.Id.ToString());        
        result.Item.ChaveAcesso.Should().Be("35240314200166000187650010000001231234567890");
    }

    [Test]
    public async Task GET_DocumentosFiscais_ById_NonExistingDocument_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-API-Key", "sieg-dev-api-key-2026");

        // Act
        var response = await _client.GetAsync($"/api/documentos-fiscais/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedDocumentosAsync()
    {
        var documentos = new[]
        {
            new DocumentoFiscal { Id = Guid.NewGuid(), ChaveAcesso = "chave001", Data = DateTime.Now, ValorTotal = 1000m, TipoDocumento = TipoDocumentoFiscal.NFe, XMLOriginal = "<xml1/>", HashXml = "hash1", DocumentoEmissor = "14200166000187", DocumentoDestinatario = "12345678901", TipoEmissor = TipoPessoaFiscal.PJ, TipoDestinatario = TipoPessoaFiscal.PF },
            new DocumentoFiscal { Id = Guid.NewGuid(), ChaveAcesso = "chave002", Data = DateTime.Now, ValorTotal = 2000m, TipoDocumento = TipoDocumentoFiscal.CTe, XMLOriginal = "<xml2/>", HashXml = "hash2", DocumentoEmissor = "14200166000187", DocumentoDestinatario = "12345678902", TipoEmissor = TipoPessoaFiscal.PJ, TipoDestinatario = TipoPessoaFiscal.PF },
            new DocumentoFiscal { Id = Guid.NewGuid(), ChaveAcesso = "chave003", Data = DateTime.Now, ValorTotal = 3000m, TipoDocumento = TipoDocumentoFiscal.NFSe, XMLOriginal = "<xml3/>", HashXml = "hash3", DocumentoEmissor = "14200166000187", DocumentoDestinatario = "12345678903", TipoEmissor = TipoPessoaFiscal.PJ, TipoDestinatario = TipoPessoaFiscal.PF },
            new DocumentoFiscal { Id = Guid.NewGuid(), ChaveAcesso = "chave004", Data = DateTime.Now, ValorTotal = 4000m, TipoDocumento = TipoDocumentoFiscal.NFe, XMLOriginal = "<xml4/>", HashXml = "hash4", DocumentoEmissor = "14200166000187", DocumentoDestinatario = "12345678904", TipoEmissor = TipoPessoaFiscal.PJ, TipoDestinatario = TipoPessoaFiscal.PF },
            new DocumentoFiscal { Id = Guid.NewGuid(), ChaveAcesso = "chave005", Data = DateTime.Now, ValorTotal = 5000m, TipoDocumento = TipoDocumentoFiscal.CTe, XMLOriginal = "<xml5/>", HashXml = "hash5", DocumentoEmissor = "14200166000187", DocumentoDestinatario = "12345678905", TipoEmissor = TipoPessoaFiscal.PJ, TipoDestinatario = TipoPessoaFiscal.PF }
        };

        foreach (var documento in documentos)
        {
            await _repository.AddAsync(documento);
        }
    }

    private static string GetValidProcNFeXml()
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "procNFe.xml"));
    }
}


public class InMemoryDocumentoFiscalRepository : IDocumentoFiscalRepository
{
    private readonly List<DocumentoFiscal> _documentos = new();


    public Task<DocumentoFiscal?> GetByIdAsync(Guid id)
    {
        var documento = _documentos.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(documento);
    }

    public Task<DocumentoFiscal?> GetByChaveAcessoAsync(string chaveAcesso)
    {
        var documento = _documentos.FirstOrDefault(d => d.ChaveAcesso == chaveAcesso);
        return Task.FromResult(documento);
    }

    public Task<DocumentoFiscal?> GetByHashAsync(string hashXml)
    {
        var documento = _documentos.FirstOrDefault(d => d.HashXml == hashXml);
        return Task.FromResult(documento);
    }

    public Task<DocumentoFiscal?> GetById(Guid id)
    {
        var documento = _documentos.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(documento);
    }

    
    public Task<IEnumerable<DocumentoFiscal>> GetAllAsync(int page, int size, TipoDocumentoFiscal? tipoDocumento = null)
    {
        var query = _documentos.AsQueryable();
        
        if (tipoDocumento.HasValue)
        {
            query = query.Where(d => d.TipoDocumento == tipoDocumento.Value);
        }

        var result = query
            .Skip((page - 1) * size)
            .Take(size)
            .AsEnumerable();

        return Task.FromResult(result);
    }

    public Task<int> CountAsync(TipoDocumentoFiscal? tipoDocumento = null)
    {
        var query = _documentos.AsQueryable();
        
        if (tipoDocumento.HasValue)
        {
            query = query.Where(d => d.TipoDocumento == tipoDocumento.Value);
        }

        return Task.FromResult(query.Count());
    }

    
    public IEnumerable<DocumentoFiscal> GetByFilter(Expression<Func<DocumentoFiscal, bool>> filter)
    {
        var compiledFilter = filter.Compile();
        return _documentos.Where(compiledFilter);
    }

    public Task<IEnumerable<DocumentoFiscal>> GetByFilterAsync(Expression<Func<DocumentoFiscal, bool>> filter)
    {
        var result = GetByFilter(filter);
        return Task.FromResult(result);
    }

    public IQueryable<DocumentoFiscal> GetIQueryable()
    {
        return _documentos.AsQueryable();
    }

    public void Add(DocumentoFiscal entity)
    {
        _documentos.Add(entity);
    }

    public Task AddAsync(DocumentoFiscal entity)
    {
        entity.Id = Guid.NewGuid();
        _documentos.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(DocumentoFiscal entity)
    {
        var existing = _documentos.FirstOrDefault(d => d.Id == entity.Id);
        if (existing != null)
        {
            _documentos.Remove(existing);
            _documentos.Add(entity);
        }
    }

    public Task UpdateAsync(DocumentoFiscal entity)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    public void Delete(DocumentoFiscal entity)
    {
        _documentos.Remove(entity);
    }

    public Task DeleteAsync(DocumentoFiscal entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var documento = _documentos.FirstOrDefault(d => d.Id == id);
        if (documento != null)
        {
            _documentos.Remove(documento);
        }
        return Task.CompletedTask;
    }

    public void SaveChanges()
    {
       
    }

    public Task SaveChangesAsync()
    {
        
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _documentos.Clear();
    }
}

public class InMemoryDocumentoFiscaisResumoMensalRepository : IDocumentoFiscaisResumoMensalRepository
{
    private readonly List<DocumentoFiscaisResumoMensal> _resumos = new();


    public Task<DocumentoFiscaisResumoMensal?> GetByAnoMesTipoAsync(int ano, int mes, TipoDocumentoFiscal tipoDocumento)
    {
        var resumo = _resumos.FirstOrDefault(r => r.Ano == ano && r.Mes == mes && r.TipoDocumento == tipoDocumento);
        return Task.FromResult(resumo);
    }

    public IEnumerable<DocumentoFiscaisResumoMensal> GetByFilter(Expression<Func<DocumentoFiscaisResumoMensal, bool>> filter)
    {
        var compiledFilter = filter.Compile();
        return _resumos.Where(compiledFilter);
    }

    public Task<IEnumerable<DocumentoFiscaisResumoMensal>> GetByFilterAsync(Expression<Func<DocumentoFiscaisResumoMensal, bool>> filter)
    {
        var result = GetByFilter(filter);
        return Task.FromResult(result);
    }

    public IQueryable<DocumentoFiscaisResumoMensal> GetIQueryable()
    {
        return _resumos.AsQueryable();
    }

    public void Add(DocumentoFiscaisResumoMensal entity)
    {
        _resumos.Add(entity);
    }

    public Task AddAsync(DocumentoFiscaisResumoMensal entity)
    {
        _resumos.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(DocumentoFiscaisResumoMensal entity)
    {
        var existing = _resumos.FirstOrDefault(r => r.Id == entity.Id);
        if (existing != null)
        {
            _resumos.Remove(existing);
            _resumos.Add(entity);
        }
    }

    public Task UpdateAsync(DocumentoFiscaisResumoMensal entity)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    public void Delete(DocumentoFiscaisResumoMensal entity)
    {
        _resumos.Remove(entity);
    }

    public Task DeleteAsync(DocumentoFiscaisResumoMensal entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var resumo = _resumos.FirstOrDefault(r => r.Id == id);
        if (resumo != null)
        {
            _resumos.Remove(resumo);
        }
        return Task.CompletedTask;
    }

    public void SaveChanges()
    {
        
    }

    public Task SaveChangesAsync()
    {
       
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _resumos.Clear();
    }
}

public class MockEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T eventData, string? queueName = default, CancellationToken cancellationToken = default) where T : class
    {
        
        return Task.CompletedTask;
    }
}
