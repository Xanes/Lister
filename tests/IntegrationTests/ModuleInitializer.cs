using System.Runtime.CompilerServices;
using VerifyTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Use DiffPlex for better diffing output
        VerifyDiffPlex.Initialize();

        // Optional: Configure other Verify settings here, e.g.,
        // VerifyBase.DontScrubGuids();
        // VerifyBase.AutoVerify();
    }
} 