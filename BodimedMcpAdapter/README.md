# Bodimed MCP Adapter

This project is a separate, minimal MCP-to-HTTP adapter. It does not reference or
modify `HomeApi`, does not access the database, and does not read or cache the
blood-test catalog.

## Runtime flow

1. An authenticated MCP client calls `create_patient`.
2. The adapter validates every argument.
3. With `confirmed=false`, it returns a confirmation-required result and makes no
   HTTP request.
4. With `confirmed=true`, it sends exactly one `POST` request to
   `/api/Bodimed/createPatient` on the configured HomeApi.

The service exposes:

- `POST /mcp` — stateless Streamable HTTP MCP endpoint, protected by JWT Bearer
  authentication and the configured write scope.
- `GET /health` — unauthenticated health endpoint.
- `GET /.well-known/oauth-protected-resource` — MCP protected-resource metadata.

## Configuration

All runtime values are environment variables. No secrets belong in source,
Dockerfiles, workflow files, command history, or logs.

| Variable | Required | Meaning |
| --- | --- | --- |
| `BODIMED_API_KEY` | Yes | Existing `X-Api-Key` value accepted by HomeApi. |
| `BODIMED_API_BASE_URL` | No | HomeApi base URL. Defaults to `https://home-api.jollyflower-51e4d941.northeurope.azurecontainerapps.io`. |
| `MCP_AUTH_ISSUER` | Yes | Exact trusted token issuer/authority. |
| `MCP_AUTH_AUDIENCE` | Yes | Expected access-token audience for this MCP resource. |
| `MCP_AUTH_AUTHORIZATION_SERVER` | Yes | OAuth authorization-server URL advertised to MCP clients. |
| `MCP_AUTH_PUBLIC_BASE_URL` | Yes | Public HTTPS origin of this adapter, without `/mcp`. |
| `MCP_AUTH_REQUIRED_SCOPE` | Yes | Scope required to invoke `create_patient`, for example `bodimed.write`. |

Production refuses to start when any required value is absent or when a configured
URL is not HTTPS. JWT signature, issuer, audience, expiry, and scope are validated.

## OAuth boundary and remaining blocker

The adapter is an OAuth-protected resource server; it is not an authorization
server and contains no login system. Before publication, choose and configure an
MCP-compatible external provider such as Microsoft Entra ID or Auth0. The provider
must issue JWT access tokens for the configured audience and scope, publish issuer
metadata/signing keys, and support the OAuth client flow required by the intended
MCP client.

Do not make the Container App public until that provider, all required runtime
configuration values, and a successful authenticated end-to-end test are in place.

## Tool schema

The SDK-generated input schema is:

```json
{
  "type": "object",
  "properties": {
    "fullName": { "description": "Patient full name. Required.", "type": "string" },
    "egn": { "description": "Patient EGN containing exactly 10 digits. Required.", "type": "string" },
    "phoneNumber": { "description": "Patient phone number. Required.", "type": "string" },
    "date": { "description": "Patient date and time in ISO 8601 format, including a UTC offset or Z.", "type": "string" },
    "note": { "description": "Final patient note. Required.", "type": "string" },
    "bloodTests": {
      "description": "At least one fully specified blood test selected by the user.",
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": { "description": "Positive Bodimed blood-test ID.", "type": "integer" },
          "name": { "description": "Exact blood-test name selected by the user.", "type": "string" },
          "bngPrice": { "description": "Non-negative price in BGN.", "type": "number" },
          "euroPrice": { "description": "Non-negative price in EUR.", "type": "number" },
          "hasPriority": { "description": "Whether the test has priority.", "type": "boolean" }
        },
        "required": ["id", "name", "bngPrice", "euroPrice", "hasPriority"]
      }
    },
    "confirmed": {
      "description": "Must be true only after the user explicitly confirms all exact values.",
      "type": "boolean",
      "default": false
    }
  },
  "required": ["fullName", "egn", "phoneNumber", "date", "note", "bloodTests"]
}
```

The tool annotations are `readOnlyHint=false`, `destructiveHint=true`,
`idempotentHint=false`, and `openWorldHint=true`. Explicit confirmation is also
enforced in server code.

## Outgoing JSON

For a confirmed request, the adapter sends this shape (synthetic values shown):

```json
{
  "id": 0,
  "fullName": "Test Patient",
  "egn": "1234567890",
  "phoneNumber": "+359000000000",
  "date": "2026-09-15T10:00:00+03:00",
  "note": "Test note",
  "patientBloodTests": [],
  "bloodTests": [
    {
      "id": 17,
      "name": "TSH",
      "bngPrice": 10.20,
      "euroPrice": 5.21,
      "hasPriority": true
    }
  ]
}
```

## Local verification

Set synthetic/development configuration in the current shell, then run:

```powershell
dotnet build .\BodimedMcpAdapter\BodimedMcpAdapter.csproj --configuration Release
dotnet test .\BodimedMcpAdapter\Tests\BodimedMcpAdapter.Tests.csproj --configuration Release
dotnet run --project .\BodimedMcpAdapter\BodimedMcpAdapter.csproj
```

Do not use a production API key for ordinary local tests. Automated tests use a
fake HTTP handler and never call HomeApi.

## Container and deployment preparation

Build locally from the repository root:

```powershell
docker build --file .\BodimedMcpAdapter\Dockerfile --tag bodimed-mcp-adapter:local .
```

The dedicated GitHub workflow runs only for changes under `BodimedMcpAdapter/**`
or to its own workflow. It updates only the existing MCP Container App named by
the repository variable `BODIMED_MCP_CONTAINER_APP_NAME`, using the commit SHA as
the image tag. It never creates Azure resources and never updates HomeApi or the
daily jobs.

Before enabling the workflow:

1. Configure the external OAuth provider.
2. Create and secure the dedicated Azure Container App outside this change.
3. Store `BODIMED_API_KEY` as an Azure Container Apps secret and reference it from
   the environment variable; configure the remaining variables on the app.
4. Set the GitHub repository variable `BODIMED_MCP_CONTAINER_APP_NAME`.
5. Verify `/health`, unauthenticated `/mcp` rejection, protected-resource metadata,
   token validation, scope enforcement, and one synthetic end-to-end patient in a
   safe non-production environment.

Never log patient names, EGN, phone numbers, notes, blood-test data, request bodies,
or `X-Api-Key`. Logging filters keep MCP and HTTP-client internals at Warning or
higher, and application error logs contain only a generic operation name and HTTP
status.
