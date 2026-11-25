# MyPortfolioBackend

Portfolio-API (.NET 10) met CRUD voor projecten en blogposts,
beveiligd met ASP.NET Core Identity en getest met xUnit.

## Wie mag wat

| Rol    | Hoe krijg je de rol?                   | Rechten                              |
|--------|----------------------------------------|--------------------------------------|
| Gast   | Automatisch bij registratie            | Alleen lezen                         |
| Editor | Lokaal via de seeder of SQL (stap 4)   | Lezen, aanmaken, wijzigen, verwijderen |
| Admin  | Lokaal via de seeder of SQL (stap 4)   | Lezen, aanmaken, wijzigen, verwijderen |

- GET-routes zijn publiek.
- POST, PUT en DELETE hebben `[Authorize(Policy = Policies.CanEdit)]`. De policy `CanEdit`
  (in `Program.cs`) vereist de rol Editor **of** Admin; Admin staat er expliciet in.
- Publieke registratie geeft altijd alleen de rol Gast; Editor of Admin kan daar nooit bij.
- Niet ingelogd → **401**. Ingelogd zonder Editor/Admin → **403**.
- Na een rolwijziging moet de gebruiker **opnieuw inloggen**, omdat de rollen in de
  cookie staan die bij het inloggen is gemaakt.

## 1. Database starten

De eerste keer maak je de SQL Server-container aan (kies zelf een sterk wachtwoord):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<JOUW_WACHTWOORD>" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Daarna start je hem steeds met:

```bash
docker start sqlserver
```

## 2. Connectionstring instellen (niet in de repository)

Voer dit uit in de map van `MyPortfolioBackend.csproj`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MyPortfolioDb;User Id=sa;Password=<JOUW_WACHTWOORD>;TrustServerCertificate=True;"
```

## 3. Migraties toepassen

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update
```

Dit maakt de tabellen voor projecten, blogposts en Identity aan. De rollen
Gast (Id 1), Editor (Id 2) en Admin (Id 3) worden automatisch toegevoegd.

## 4. Gewone gebruiker, Editor en Admin maken

**Editor en Admin (lokaal script):** zet hun gegevens in user-secrets. Bij `dotnet run`
in Development maakt `Data/DbSeeder.cs` deze gebruikers aan en geeft ze hun rol.
De wachtwoorden staan dus niet in de repository.

```bash
dotnet user-secrets set "Seed:EditorEmail" "editor@portfolio.nl"
dotnet user-secrets set "Seed:EditorPassword" "<EIGEN_WACHTWOORD>"
dotnet user-secrets set "Seed:AdminEmail" "admin@portfolio.nl"
dotnet user-secrets set "Seed:AdminPassword" "<EIGEN_WACHTWOORD>"
```

**Gewone gebruiker (Gast):** registreer via `POST /api/auth/register` in Swagger.

**Alternatief met SQL:** registreer een gebruiker en geef hem daarna in de database een rol:

```sql
-- Editor (RoleId 2), Admin is RoleId 3
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, '2' FROM AspNetUsers WHERE Email = 'editor@portfolio.nl';
```

Na een rolwijziging moet de gebruiker **opnieuw inloggen**. Met `GET /api/auth/me`
zie je met welke gebruiker en rollen je bent ingelogd.

## 5. Starten

```bash
dotnet run
```

Open http://localhost:5145/swagger. Na `POST /api/auth/login` stuurt de browser de
cookie automatisch mee.

## 6. Tests uitvoeren

```bash
dotnet test MyPortfolioBackend.slnx
```

De tests starten de echte API met `WebApplicationFactory`, maar gebruiken een aparte
SQLite-database in het geheugen. De gewone database wordt niet geraakt.

## Routes en tests

| Route | Wie | Resultaten | Tests |
|-------|-----|------------|-------|
| `GET /api/projects` | Iedereen | 200 | `GetAll_ZonderLogin_Geeft200MetProjecten` |
| `GET /api/projects/{id}` | Iedereen | 200, 404 | `GetById_BestaandId_Geeft200MetJuisteProject`, `GetById_OnbekendId_Geeft404` |
| `POST /api/projects` | Editor, Admin | 201, 400, 401, 403 | `Post_AlsEditorOfAdmin_Geeft201EnSlaatOp` (Editor én Admin), `Post_OngeldigeInvoer_Geeft400`, `Post_ZonderLogin_Geeft401EnSlaatNietsOp`, `Post_AlsGast_Geeft403EnSlaatNietsOp` |
| `PUT /api/projects/{id}` | Editor, Admin | 200, 401, 403, 404 | `Put_AlsEditorOfAdmin_Geeft200EnWijzigtProject` (Editor én Admin), `Put_ZonderLogin_Geeft401EnWijzigtNiets`, `Put_AlsGast_Geeft403EnWijzigtNiets`, `Put_OnbekendId_Geeft404` |
| `DELETE /api/projects/{id}` | Editor, Admin | 204, 401, 403, 404 | `Delete_AlsEditorOfAdmin_Geeft204EnVerwijdertProject` (Editor én Admin), `Delete_ZonderLogin_Geeft401EnVerwijdertNiets`, `Delete_AlsGast_Geeft403EnVerwijdertNiets`, `Delete_OnbekendId_Geeft404` |
| `GET /api/blogposts` | Iedereen | 200 | `GetAll_ZonderLogin_Geeft200MetBlogposts` |
| `GET /api/blogposts/{id}` | Iedereen | 200, 404 | `GetById_BestaandId_Geeft200MetJuisteBlogpost`, `GetById_OnbekendId_Geeft404` |
| `POST /api/blogposts` | Editor, Admin | 201, 400, 401, 403 | `Post_AlsEditorOfAdmin_Geeft201EnSlaatOp` (Editor én Admin), `Post_OngeldigeInvoer_Geeft400`, `Post_ZonderLogin_Geeft401EnSlaatNietsOp`, `Post_AlsGast_Geeft403EnSlaatNietsOp` |
| `PUT /api/blogposts/{id}` | Editor, Admin | 200, 401, 403, 404 | `Put_AlsEditorOfAdmin_Geeft200EnWijzigtBlogpost` (Editor én Admin), `Put_ZonderLogin_Geeft401EnWijzigtNiets`, `Put_AlsGast_Geeft403EnWijzigtNiets`, `Put_OnbekendId_Geeft404` |
| `DELETE /api/blogposts/{id}` | Editor, Admin | 204, 401, 403, 404 | `Delete_AlsEditorOfAdmin_Geeft204EnVerwijdertBlogpost` (Editor én Admin), `Delete_ZonderLogin_Geeft401EnVerwijdertNiets`, `Delete_AlsGast_Geeft403EnVerwijdertNiets`, `Delete_OnbekendId_Geeft404` |
| `POST /api/auth/register` | Iedereen | 200 (rol Gast), 400 | `Register_NieuweGebruiker_Geeft200EnRolGast`, `Register_ZwakWachtwoord_Geeft400` |
| `POST /api/auth/login` | Iedereen | 200 + cookie, 401 | `Login_JuisteGegevens_Geeft200EnCookie`, `Login_FoutWachtwoord_Geeft401` |
| `GET /api/auth/me` | Aangemeld | 200, 401 | `Me_AlsEditor_Geeft200MetRolEditor`, `Me_ZonderLogin_Geeft401` |

De testklassen staan in `MyPortfolioBackend.Tests/` (`ProjectsTests.cs`, `BlogpostsTests.cs`, `AuthTests.cs`).

**Coverage:** alle tien CRUD-routes en de registratie- en loginroute zijn via HTTP getest
tegen de echte API en de echte Identity-configuratie. Voor elke schrijfroute (POST, PUT,
DELETE) is getest:

- als Editor én als Admin → gelukt, en de database is echt aangepast;
- zonder login → 401, en de database is niet veranderd;
- als Gast → 403, en de database is niet veranderd;
- ongeldige invoer (400) of een onbekend id (404).

Registratie controleert dat de nieuwe gebruiker alleen de rol Gast krijgt; login controleert
dat de API een Identity-cookie teruggeeft. Elke test heeft de blokken Arrange, Act en Assert.
