# SIEG Sr Dev Challenge - Sistema de Processamento de Documentos Fiscais

## 📋 Sobre o Projeto

Este projeto implementa um sistema completo para processamento de documentos fiscais (NFe, CTe, NFSe) com arquitetura orientada a eventos utilizando RabbitMQ. O sistema processa documentos XML, gera resumos mensais automatizados e oferece uma API REST robusta com autenticação por API Key.

## 🏗️ Arquitetura

### Padrões Arquiteturais
- **Clean Architecture**: Separação clara de responsabilidades em camadas
- **CQRS**: Command Query Responsibility Segregation com MediatR
- **Event-Driven Architecture**: Comunicação assíncrona via eventos
- **Repository Pattern**: Abstração da camada de dados
- **Dependency Injection**: Inversão de controle e injeção de dependências

### Tecnologias Utilizadas
- **.NET 8**: Framework principal
- **ASP.NET Core**: API REST
- **MediatR**: Mediador para CQRS
- **RabbitMQ**: Message broker para eventos
- **MongoDB**: Banco de dados NoSQL
- **Polly**: Resilience patterns (retry, circuit breaker)
- **FluentValidation**: Validação de dados
- **NUnit**: Framework de testes
- **Docker**: Containerização

## 📁 Estrutura do Projeto

```
SrDevChallenge/
├── src/
│   ├── SIEG.SrDevChallenge.Api/              # API REST Layer
│   │   ├── Configurations/                    # Configurações da aplicação
│   │   ├── Endpoints/                         # Minimal API endpoints
│   │   └── Middlewares/                       # Middlewares customizados
│   │
│   ├── SIEG.SrDevChallenge.Application/      # Application Layer
│   │   ├── Commands/                          # Commands do CQRS
│   │   ├── Queries/                          # Queries do CQRS
│   │   ├── Events/                           # Eventos de domínio
│   │   └── Contracts/                        # Interfaces
│   │
│   ├── SIEG.SrDevChallenge.Domain/          # Domain Layer
│   │   ├── Entities/                         # Entidades de domínio
│   │   ├── Enums/                            # Enumeradores
│   │   └── Exceptions/                       # Exceções customizadas
│   │
│   ├── SIEG.SrDevChallenge.Infrastructure/  # Infrastructure Layer
│   │   ├── Persistence/                      # Repositórios e contexto
│   │   ├── Messaging/                        # RabbitMQ publishers/consumers
│   │   └── Services/                         # Serviços de infraestrutura
│   │
│   └── SIEG.SrDevChallenge.CrossCutting/    # Cross-Cutting Concerns
│       └── Helpers/                          # Utilitários compartilhados
│
├── tests/
│   ├── SIEG.SrDevChallenge.UnitTests/        # Testes unitários
│   └── SIEG.SrDevChallenge.IntegrationTests/ # Testes de integração
│
├── docker-compose.yml                         # Orquestração de containers
└── README.md
```

## 🚀 Funcionalidades

### ✅ API REST
- **POST /documentos-fiscais**: Processa documentos XML (NFe, CTe, NFSe)
- **GET /documentos-fiscais**: Lista documentos com paginação e filtros
- **GET /documentos-fiscais/{id}**: Busca documento por ID
- **Autenticação**: API Key via header `X-API-Key`
- **Validação**: Schema XML e regras de negócio
- **Tratamento de Erros**: Global exception handler

### ✅ Processamento de Eventos
- **Publisher**: Publica eventos `DocumentoFiscalCriado` no RabbitMQ
- **Consumer**: Consome eventos e processa resumos mensais
- **Resilience**: Retry com backoff exponencial e circuit breaker
- **Dead Letter Queue**: Tratamento de mensagens com falha

### ✅ Resumos Mensais Automatizados
- **Agregação**: Conta documentos por mês/ano/tipo
- **Totalização**: Soma valores totais dos documentos
- **Atualização**: Incrementa resumos existentes ou cria novos

### ✅ Persistência
- **MongoDB**: Armazenamento de documentos e resumos
- **Repository Pattern**: Abstração da camada de dados
- **Indexes**: Otimização de consultas

## ⚙️ Configuração e Execução

### Pré-requisitos
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 ou VS Code

### 1. Clonar o Repositório
```bash
git clone <repository-url>
cd SrDevChallenge
```

### 2. Executar com Docker Compose
```bash
# Subir toda a infraestrutura
docker-compose up -d

# Verificar se os serviços estão rodando
docker-compose ps
```

### 3. Executar Localmente (Desenvolvimento)
```bash
# Subir apenas a infraestrutura
docker-compose up -d rabbitmq mongodb

# Executar a aplicação
cd src/SIEG.SrDevChallenge.Api
dotnet run
```

### 4. Configuração de Variáveis de Ambiente
```bash
# API Configuration
ApiKey=your-api-key-here
ConnectionStrings__DefaultConnection=mongodb://localhost:27017/sieg-challenge

# RabbitMQ Configuration
RabbitMQ__ConnectionString=amqp://guest:guest@localhost:5672
RabbitMQ__ExchangeName=fiscal-documents-exchange
RabbitMQ__QueueName=documento-fiscal-criado-queue
```

## 🧪 Testes

### Executar Todos os Testes
```bash
# Executar testes unitários
dotnet test tests/SIEG.SrDevChallenge.UnitTests/

# Executar testes de integração
dotnet test tests/SIEG.SrDevChallenge.IntegrationTests/

# Executar todos os testes com coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Estrutura de Testes
- **Testes Unitários**: Validam lógicas isoladas com mocks
- **Testes de Integração**: Validam fluxos end-to-end com testcontainers
- **Dados de Teste**: Utilizam formato procNFe/procCTe real

### Cobertura de Testes
- ✅ Commands e Handlers
- ✅ Entidades de Domínio
- ✅ Eventos e Mensageria  
- ✅ API Endpoints
- ✅ Middlewares
- ✅ Repositórios

## 🔧 Uso da API

### Autenticação
Todas as requisições devem incluir o header:
```
X-API-Key: your-api-key-here
```

### Exemplos de Uso

#### Processar NFe
```bash
curl -X POST "http://localhost:5000/documentos-fiscais" \
  -H "X-API-Key: your-api-key-here" \
  -H "Content-Type: application/xml" \
  -d @nfe-example.xml
```

#### Listar Documentos
```bash
curl -X GET "http://localhost:5000/documentos-fiscais?page=1&size=10&tipoDocumento=NFe" \
  -H "X-API-Key: your-api-key-here"
```

#### Buscar por ID
```bash
curl -X GET "http://localhost:5000/documentos-fiscais/{id}" \
  -H "X-API-Key: your-api-key-here"
```

### Formatos Suportados
- **NFe**: Nota Fiscal Eletrônica (modelo 65)
- **CTe**: Conhecimento de Transporte Eletrônico (modelo 57)  
- **NFSe**: Nota Fiscal de Serviço Eletrônica

O sistema aceita documentos nos formatos:
- `<nfeProc>` (NFe com protocolo de autorização)
- `<cteProc>` (CTe com protocolo de autorização)
- `<CompNfse>` (NFSe completa)

## 🛠️ Monitoramento

### RabbitMQ Management
```
URL: http://localhost:15672
User: guest
Password: guest
```

### MongoDB Compass
```
Connection String: mongodb://localhost:27017
Database: sieg-challenge
```

### Logs da Aplicação
```bash
# Visualizar logs em tempo real
docker-compose logs -f api

# Logs do RabbitMQ Consumer
docker-compose logs -f api | grep "RabbitMqEventConsumer"
```

## 🔄 Estratégias de Resilência

### Retry com Polly
- **Exponential Backoff**: Delay crescente entre tentativas
- **Jitter**: Randomização para evitar thundering herd
- **Max Retries**: Limite configurável de tentativas

### Circuit Breaker
- **Threshold**: Número de falhas para abrir o circuit
- **Duration**: Tempo em estado aberto
- **Half-Open**: Tenta uma requisição para testar recuperação

### Dead Letter Queue
- **DLX**: Mensagens com falha vão para Dead Letter Exchange
- **TTL**: Time to live configurável
- **Reprocessing**: Possibilidade de reprocessar mensagens

## 📊 Métricas e Observabilidade

### Health Checks
```bash
# Verificar saúde da aplicação
curl http://localhost:5000/health

# Verificar saúde detalhada
curl http://localhost:5000/health/ready
```

### Métricas de Negócio
- Total de documentos processados
- Documentos por tipo (NFe, CTe, NFSe)
- Resumos mensais gerados
- Taxa de sucesso/falha

## 🚦 Troubleshooting

### Problemas Comuns

#### RabbitMQ Connection Failed
```bash
# Verificar se o RabbitMQ está rodando
docker-compose ps rabbitmq

# Reiniciar RabbitMQ
docker-compose restart rabbitmq
```

#### MongoDB Connection Issues
```bash
# Verificar logs do MongoDB
docker-compose logs mongodb

# Testar conexão
mongosh mongodb://localhost:27017/sieg-challenge
```

#### API Key Unauthorized
- Verificar se o header `X-API-Key` está presente
- Validar se a API Key está correta no `appsettings.json`

### Logs de Debug
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SIEG.SrDevChallenge": "Debug",
      "RabbitMQ": "Debug"
    }
  }
}
```

## 📈 Próximos Passos

### Melhorias Futuras
- [ ] Implementar OpenTelemetry para observabilidade
- [ ] Adicionar cache distribuído (Redis)
- [ ] Implementar rate limiting
- [ ] Adicionar compressão de mensagens
- [ ] Implementar backup automático de dados
- [ ] Criar dashboard de monitoramento

### Scalabilidade
- [ ] Horizontal scaling do consumer
- [ ] Sharding do MongoDB
- [ ] Load balancer para API
- [ ] CDN para assets estáticos

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👥 Equipe

- **Desenvolvedor**: [Seu Nome]
- **Email**: [seu.email@exemplo.com]
- **LinkedIn**: [seu-perfil-linkedin]

---

**SIEG Sr Dev Challenge** - Sistema de Processamento de Documentos Fiscais com Arquitetura Event-Driven