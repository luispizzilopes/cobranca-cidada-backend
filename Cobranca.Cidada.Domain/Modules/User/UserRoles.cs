namespace Cobranca.Cidada.Domain.Modules.User;

public static class UserRoles
{
    public const string Citizen = "Citizen";
    public const string Curator = "Curator";
    public const string Admin = "Admin";

    public static readonly string[] All = [Citizen, Curator, Admin];

    /// <summary>
    /// Papéis efetivos de uma conta. "Admin" vem apenas da flag <c>IsAdmin</c> do usuário:
    /// um papel "Admin" avulso na tabela de papéis do Identity não concede nem exibe nada.
    /// </summary>
    public static IReadOnlyList<string> Resolve(bool isAdmin, IEnumerable<string> storedRoles)
    {
        var roles = storedRoles.Where(role => role != Admin).Distinct().ToList();
        if (isAdmin)
            roles.Add(Admin);
        return roles;
    }
}
