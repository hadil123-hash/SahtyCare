using System;
using System.Collections.Generic;
using System.Linq;

namespace Sahty.Shared.Auth;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Pharmacist = "Pharmacist";
    public const string Patient = "Patient";

    public static readonly List<string> All = new() { Admin, Doctor, Pharmacist, Patient };

    public static string Normalize(string role)
    {
        if (string.Equals(role, "Medecin", StringComparison.OrdinalIgnoreCase))
            return Doctor;
        if (string.Equals(role, "Medecins", StringComparison.OrdinalIgnoreCase))
            return Doctor;
        if (string.Equals(role, "Pharmacien", StringComparison.OrdinalIgnoreCase))
            return Pharmacist;
        if (string.Equals(role, "Pharmaciens", StringComparison.OrdinalIgnoreCase))
            return Pharmacist;
        if (string.Equals(role, "Pharmacie", StringComparison.OrdinalIgnoreCase))
            return Pharmacist;
        if (string.Equals(role, "Pharmacy", StringComparison.OrdinalIgnoreCase))
            return Pharmacist;
        if (string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
            return Patient;
        return role;
    }

    public static IReadOnlyList<string> NormalizeAll(IEnumerable<string> roles)
        => roles.Select(Normalize).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}
