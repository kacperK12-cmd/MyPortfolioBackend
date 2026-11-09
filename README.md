# MyPortfolioBackend

Portfolio-API (.NET 10) met CRUD voor projecten en blogposts,
beveiligd met ASP.NET Core Identity en getest met xUnit.

## Wie mag wat

| Rol    | Hoe krijg je de rol?                   | Rechten                              |
|--------|----------------------------------------|--------------------------------------|
| Gast   | Automatisch bij registratie            | Alleen lezen                         |
| Editor | Handmatig in de database (zie stap 4)  | Lezen, aanmaken, wijzigen, verwijderen |
| Admin  | Handmatig in de database (zie stap 4)  | Lezen, aanmaken, wijzigen, verwijderen |

- GET-routes zijn publiek.
- POST, PUT en DELETE hebben `[Authorize(Roles = "Editor,Admin")]`.
- Niet ingelogd → **401**. Ingelogd zonder Editor/Admin → **403**.
- Na een rolwijziging moet de gebruiker **opnieuw inloggen**, omdat de rollen in de
  cookie staan die bij het inloggen is gemaakt.

## 1. Database starten

```bash
docker start sqlserver
```

## 2. Connectionstring instellen (niet in de repository)

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

1. Start de API (`dotnet run`) en registreer drie gebruikers via
   `POST /api/auth/register` in Swagger. Ze krijgen alle drie de rol Gast.
2. Geef in de database (bijv. Azure Data Studio) de rol Editor en Admin:

```sql
-- Editor (RoleId 2)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, '2' FROM AspNetUsers WHERE Email = 'editor@portfolio.nl';

-- Admin (RoleId 3)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, '3' FROM AspNetUsers WHERE Email = 'admin@portfolio.nl';
```

3. Log daarna (opnieuw) in met deze gebruikers.

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
| `POST /api/projects` | Editor, Admin | 201, 400, 401, 403 | `Post_AlsEditorOfAdmin_Geeft201EnSlaatOp`, `Post_OngeldigeInvoer_Geeft400`, `Post_ZonderLogin_Geeft401EnSlaatNietsOp`, `Post_AlsGast_Geeft403EnSlaatNietsOp` |
| `PUT /api/projects/{id}` | Editor, Admin | 200, 404 | `Put_AlsEditor_Geeft200EnWijzigtProject`, `Put_OnbekendId_Geeft404` |
| `DELETE /api/projects/{id}` | Editor, Admin | 204, 401 | `Delete_AlsAdmin_Geeft204EnVerwijdertProject`, `Delete_ZonderLogin_Geeft401EnVerwijdertNiets` |
| `GET /api/blogposts` | Iedereen | 200 | `GetAll_ZonderLogin_Geeft200MetBlogposts` |
| `GET /api/blogposts/{id}` | Iedereen | 200 | `GetById_BestaandId_Geeft200MetJuisteBlogpost` |
| `POST /api/blogposts` | Editor, Admin | 201, 403 | `Post_AlsEditorOfAdmin_Geeft201EnSlaatOp`, `Post_AlsGast_Geeft403EnSlaatNietsOp` |
| `PUT /api/blogposts/{id}` | Editor, Admin | 200 | `Put_AlsEditor_Geeft200EnWijzigtBlogpost` |
| `DELETE /api/blogposts/{id}` | Editor, Admin | 204, 404 | `Delete_AlsEditor_Geeft204EnVerwijdertBlogpost`, `Delete_OnbekendId_Geeft404` |
| `POST /api/auth/register` | Iedereen | 200 (rol Gast), 400 | `Register_NieuweGebruiker_Geeft200EnRolGast`, `Register_ZwakWachtwoord_Geeft400` |
| `POST /api/auth/login` | Iedereen | 200 + cookie, 401 | `Login_JuisteGegevens_Geeft200`, `Login_FoutWachtwoord_Geeft401` |

**Coverage:** elke route is minstens één keer via HTTP getest. Bij POST, PUT en DELETE
controleren de tests ook de database: na POST staat het item erin, na PUT is het
gewijzigd, na DELETE is het weg, en bij 401/403 is er niets veranderd. Ongeldige invoer
(400), een onbekend id (404), geweigerde verzoeken (401/403) en schrijven als Editor én
Admin zijn getest. Elke test heeft de blokken Arrange, Act en Assert.
