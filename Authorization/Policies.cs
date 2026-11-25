namespace MyPortfolioBackend.Authorization;

// Namen van de autorisatiepolicies, zodat we geen losse strings gebruiken.
public static class Policies
{
    // Editor en Admin mogen projecten en blogposts aanmaken, wijzigen en verwijderen.
    // Admin staat er expliciet in: Admin krijgt niet automatisch de rechten van Editor.
    public const string CanEdit = "CanEdit";
}
