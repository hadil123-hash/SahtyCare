using System.Collections.Generic;

namespace Sahty.Web.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Pharmacist = "Pharmacist";
    public const string Patient = "Patient";

    // Compatibility aliases for existing UI labels.
    public const string Medecin = Doctor;
    public const string Pharmacien = Pharmacist;
    public const string Pharmacy = Pharmacist;
    public const string Client = Patient;

    public static readonly List<string> All = new() { Admin, Doctor, Pharmacist, Patient };
}
