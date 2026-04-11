namespace DamiFYP.Extensions;

public static class SomeExtension
{
    public static string SomeExtensionMethod(this string s) => $"{s}, {s.Length}";
}