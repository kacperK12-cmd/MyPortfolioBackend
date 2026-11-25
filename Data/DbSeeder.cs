using Microsoft.AspNetCore.Identity;

namespace MyPortfolioBackend.Data;

// Maakt lokaal de rollen en een Editor- en Admin-gebruiker aan.
// E-mail en wachtwoord komen uit user-secrets, dus er staan geen wachtwoorden in de repository.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var rol in new[] { "Gast", "Editor", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        await MaakGebruikerAsync(userManager, config["Seed:EditorEmail"], config["Seed:EditorPassword"], "Editor");
        await MaakGebruikerAsync(userManager, config["Seed:AdminEmail"], config["Seed:AdminPassword"], "Admin");
    }

    private static async Task MaakGebruikerAsync(
        UserManager<IdentityUser> userManager, string? email, string? wachtwoord, string rol)
    {
        // Geen gegevens in user-secrets? Dan slaan we deze gebruiker over.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(wachtwoord))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, wachtwoord);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Seed-gebruiker {email} kon niet worden gemaakt: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.CheckPasswordAsync(user, wachtwoord))
        {
            // Bestaande gebruiker met een ander wachtwoord: lokaal gelijkzetten aan user-secrets.
            await userManager.RemovePasswordAsync(user);
            var result = await userManager.AddPasswordAsync(user, wachtwoord);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Wachtwoord van {email} kon niet worden ingesteld: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, rol))
        {
            await userManager.AddToRoleAsync(user, rol);
        }
    }
}
