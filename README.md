# MyPortfolioBackend

Backend voor mijn portfoliosite met CRUD voor projecten en blogposts.

## Techniek en structuur

- .NET 10
- ASP.NET Core Web API met controllers
- Entity Framework Core
- Swagger UI via Swashbuckle.AspNetCore
- Database in Docker, bereikbaar op localhost:1433
- Gebruikte Docker-image: mcr.microsoft.com/azure-sql-edge

De code is georganiseerd in lagen:

- Controllers: routes en HTTP-resultaten.
- Services: applicatielogica, databasehandelingen en DTO-mapping.
- Data: ApplicationDbContext.
- Entities: databasemodellen Project en Blogpost.
- Dtos: invoer- en uitvoermodellen voor de API.
- Migrations: EF Core-databasemigraties.

Endpoints ontvangen en versturen DTO’s. Controllers gebruiken
services via constructorinjectie. De services en DbContext zijn
geregistreerd in Program.cs.

## Database configureren

Start de bestaande databasecontainer:

```bash
docker start sqlserver
```

Voeg in appsettings.json op het hoogste niveau deze instelling toe.
Vervang VUL_HIER_JE_WACHTWOORD_IN door het databasewachtwoord:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=MyPortfolioDb;User Id=sa;Password=VUL_HIER_JE_WACHTWOORD_IN;TrustServerCertificate=True;"
}
```

Behoud de bestaande Logging- en AllowedHosts-instellingen.
Zet een komma tussen de onderdelen van het JSON-object.

## Database aanmaken

De database wordt aangemaakt met EF Core-migraties.
De bestaande migratie heet InitialCreate.

Installeer de EF-tool als deze nog ontbreekt:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

Voer vanuit de projectmap uit:

```bash
dotnet restore
dotnet ef database update
```

## Applicatie starten

```bash
dotnet run
```

Open het adres uit de terminal met /swagger erachter.
Bij het huidige launch-profiel is dit:

http://localhost:5145/swagger

## Endpoints

Voor zowel /api/projects als /api/blogposts:

- GET: alle items ophalen.
- GET /{id}: één item ophalen.
- POST: een item aanmaken.
- PUT /{id}: een item wijzigen.
- DELETE /{id}: een item verwijderen.

GET, PUT en DELETE met een ontbrekend id geven 404.
De gegevens blijven na een herstart van de API bestaan.