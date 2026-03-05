# Sistema de Autenticação por API Key

Esta aplicação utiliza autenticação baseada em API Key para proteger todos os endpoints da API.

## Como usar

### 1. Via Header (Recomendado)
Adicione o header `X-API-Key` em todas as requisições:

```bash
curl -X GET "https://localhost:7000/api/documentos-fiscais" \
  -H "X-API-Key: dev-api-key-123456789"
```

### 2. Via Query Parameter
Alternativamente, você pode passar a API Key como parâmetro na URL:

```bash
curl -X GET "https://localhost:7000/api/documentos-fiscais?apikey=dev-api-key-123456789"
```

## API Keys Disponíveis

### Desenvolvimento (appsettings.Development.json)
- **Desenvolvimento**: `dev-api-key-123456789`
- **Teste**: `test-api-key-987654321`

### Produção (appsettings.json)
- **Padrão**: `sieg-dev-api-key-2024`
- **Produção**: `sieg-prod-api-key-2024`

## Endpoints Protegidos

Todos os endpoints da API requerem autenticação:

- `POST /api/documentos-fiscais` - Upload de XML
- `GET /api/documentos-fiscais` - Listar documentos
- `GET /api/documentos-fiscais/{id}` - Buscar por ID
- `DELETE /api/documentos-fiscais/{id}` - Remover documento

## Endpoints Liberados

Os seguintes endpoints não requerem autenticação:
- `/openapi` - Documentação da API
- `/health` - Health check
- `/swagger` - Interface Swagger
- `/favicon.ico` - Favicon

## Respostas de Erro

### API Key não fornecida (401)
```json
{
  "error": "Unauthorized",
  "message": "API Key é obrigatória",
  "timestamp": "2024-03-05T10:30:00.000Z"
}
```

### API Key inválida (401)
```json
{
  "error": "Unauthorized", 
  "message": "API Key inválida",
  "timestamp": "2024-03-05T10:30:00.000Z"
}
```

## Configuração

Para adicionar ou modificar API Keys, edite o arquivo `appsettings.json`:

```json
{
  "ApiKeySettings": {
    "ValidApiKeys": [
      {
        "Key": "sua-api-key-aqui",
        "Name": "Nome da Chave",
        "CreatedAt": "2024-01-01T00:00:00Z",
        "IsActive": true,
        "Description": "Descrição da chave"
      }
    ]
  }
}
```

## Logs de Segurança

O sistema registra automaticamente:
- Tentativas de acesso sem API Key
- Uso de API Keys inválidas 
- Acessos autorizados com sucesso

Verifique os logs para monitorar a segurança da aplicação.

## Variáveis de Ambiente

Para maior segurança em produção, configure as API Keys via variáveis de ambiente:

```bash
export ApiKeySettings__ValidApiKeys__0__Key="sua-chave-secreta"
export ApiKeySettings__ValidApiKeys__0__Name="Producao Key"
```