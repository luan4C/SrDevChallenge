# SrDev Challenge – Fiscal Document Processing API

API para ingestão, processamento e consulta de **documentos fiscais (NF-e, CT-e e NFSe)** a partir de arquivos XML.

O sistema processa documentos fiscais, extrai metadados relevantes, armazena os dados em banco e publica eventos para processamento assíncrono.

Além da extração de metadados também há a validação do esquema xml do arquivo enviado.

O objetivo do projeto é demonstrar boas práticas de engenharia de software, incluindo:

- Clean Architecture
- Domain Driven Design (DDD)
- Processamento eficiente de XML
- Arquitetura orientada a eventos
- Resiliência e retry
- Rate limiting

---

# Eventos
- Evento de documento criado alimenta uma collection "documentosFiscaisResumoMensal"

# Tecnologias utilizadas

- .NET 10
- ASP.NET Core
- MongoDB
- RabbitMQ
- MediatR
- Polly
- Docker


---

# Arquitetura

O projeto segue o padrão **Clean Architecture**, separando responsabilidades em camadas.
``` 
src/
├── SIEG.SrDevChallenge.Api
├── SIEG.SrDevChallenge.Application
├── SIEG.SrDevChallenge.Domain
├── SIEG.SrDevChallenge.Infrastructure
├── SIEG.SrDevChallenge.CrossCutting
└── SIEG.SrDevChallenge.ArchitecturalTests
└── SIEG.SrDevChallenge.IntegrationTests
└── SIEG.SrDevChallenge.UnitTests
```

# Executando o projeto

## 1. Instalar .NET 10

Baixar runtime ou SDK:

https://dotnet.microsoft.com/en-us/download/dotnet/10.0

Verificar instalação:
```
dotnet --version
```
## 2. Criar .env na raiz do projeto baseado no exemplo do [.env.example](.env.example)
Importante para o projeto rodar com as configurações certas e o container também

## 3. Subir dependências (Docker)

O projeto utiliza:

- MongoDB
- RabbitMQ

Executar:
```
docker-compose up -d
```

## 3. Rodar a aplicação
```
dotnet restore
dotnet build
dotnet run --project src/SIEG.SrDevChallenge.Api
```

# Testes
```
dotnet test
```

# Decisões de arquitetura e Tecnologia

MongoDB foi escolhido para permitir armazenamento flexível de documentos fiscais XML.

RabbitMQ foi utilizado para desacoplar processamento e permitir escalabilidade.

XmlReader foi utilizado para evitar alto consumo de memória ao processar XML grandes.

A Clean Architecture com CQRS (mediatr) foi escolhida para aumentar o nível de desacoplamento, melhorando segregação de responsabilidades e testabilidade.


# Possíveis melhorias
- Observabilidade com OpenTelemetry
- Implementação de Cache
- Dinamização na validação de documentos fiscais, o projeto hoje valida apenas o procNFE, procCTE e NFSe
- Testes de carga com k6
- Adicionar mais casos de teste de integração e unitário.

# Autor

Luan Albuquerque