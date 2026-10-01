# Bridge Task – Countries & Cities API

An ASP.NET Core (.NET 10) Web API for CRUD operations on countries and cities, with pagination, filtering and sorting.

## Running it

```bash
dotnet run --project src/BridgeTask.Api --launch-profile http
```

- Swagger UI: http://localhost:5035/swagger
- The SQLite database (`bridgetask.db`) is created on startup and seeded with sample data (turn this off with `Database:Seed=false`).

To run the integration tests:

```bash
dotnet test
```

### Postman

Import [postman/BridgeTask.postman_collection.json](postman/BridgeTask.postman_collection.json). Its `baseUrl` variable defaults to `http://localhost:5035`. Run the whole collection in order with the Collection Runner. The create requests save the new ids for the requests that follow, and the error cases use the seeded data. From the command line:

```bash
npx newman run postman/BridgeTask.postman_collection.json
```

## Endpoints

| Method | Route                                | Description                                    |
|--------|--------------------------------------|------------------------------------------------|
| POST   | `/api/countries`                     | Create a country                               |
| GET    | `/api/countries/{id}`                | Get a country by id                            |
| GET    | `/api/countries`                     | List countries (paged, filtered, sorted)       |
| PUT    | `/api/countries/{id}`                | Update a country                               |
| DELETE | `/api/countries/{id}`                | Delete a country (409 if it still has cities)  |
| POST   | `/api/cities`                        | Create a city                                  |
| GET    | `/api/cities/{id}`                   | Get a city by id                               |
| GET    | `/api/cities`                        | List cities (paged, filtered, sorted)          |
| PUT    | `/api/cities/{id}`                   | Update a city (can move it to another country) |
| DELETE | `/api/cities/{id}`                   | Delete a city                                  |
| GET    | `/api/countries/{countryId}/cities`  | List one country's cities (paged)              |

### Query parameters

All list endpoints accept:

- `pageNumber` (default 1, must be at least 1)
- `pageSize` (default 10, 1–100)
- `sortBy`
- `sortDirection` (`asc` or `desc`)

| Endpoint                            | Filters                                                   | `sortBy`                                    |
|-------------------------------------|-----------------------------------------------------------|---------------------------------------------|
| `/api/countries`                    | `search` (name or code), `code` (exact)                   | `name`, `code`, `createdAt`                 |
| `/api/cities`                       | `search` (name), `countryId`, `minPopulation`, `maxPopulation` | `name`, `population`, `country`, `createdAt` |
| `/api/countries/{countryId}/cities` | `search` (name)                                           | `name`, `population`, `country`, `createdAt` |

Searches are case-insensitive partial matches.

Example: `GET /api/cities?search=al&minPopulation=1000000&sortBy=population&sortDirection=desc&pageSize=5`

### Paged response

```json
{
  "items": [ ... ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Validation & errors

Every error response uses the RFC 7807 `application/problem+json` format and includes a `traceId`.

| Status | When                                                                                       |
|--------|--------------------------------------------------------------------------------------------|
| 400    | Validation failed (FluentValidation). The `errors` field lists the problems for each field. |
| 404    | The country or city doesn't exist, including a `countryId` in the request body that doesn't exist. |
| 409    | Duplicate country name or code, duplicate city name within the same country, or deleting a country that still has cities. |
| 500    | Unexpected error. The details are logged, not returned.                                    |

Business rules:

- **Country:** `name` is required, at most 100 characters, and unique regardless of case. `code` is a 2-letter ISO 3166-1 alpha-2 code, unique, and stored in uppercase.
- **City:** `name` is required, at most 100 characters, and unique within its country regardless of case. `population` is optional and must be at least 0. `countryId` must refer to an existing country.
- Input is trimmed. Uniqueness is also enforced by database indexes.

## Project structure

```
src/BridgeTask.Api
├── Controllers/     thin HTTP layer (routing, status codes)
├── Services/        business logic and data access (ICountryService, ICityService)
├── Dtos/            request/response/query contracts (entities are never exposed)
├── Validators/      FluentValidation rules
├── Entities/        domain model
├── Data/            EF Core DbContext, entity configurations, seeding
├── Common/          pagination, exceptions, validation filter
├── Middleware/      global exception handler → ProblemDetails
└── Extensions/      DI registration
tests/BridgeTask.Api.Tests   integration tests (WebApplicationFactory + a separate SQLite database for each test)
```
