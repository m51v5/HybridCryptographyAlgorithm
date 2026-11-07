using System.Diagnostics;
using System.Reflection;

namespace HybridCryptographyAlgorithm
{
    /// <summary>Every address the app opens or reads, in one place.</summary>
    static class AppLinks
    {
        public const string Repository = "https://github.com/m51v5/HybridCryptographyAlgorithm";
        public const string Releases = Repository + "/releases";
        public const string Security = Repository + "/blob/main/SECURITY.md";
        public const string License = Repository + "/blob/main/LICENSE";
        public const string LatestReleaseApi = "https://api.github.com/repos/m51v5/HybridCryptographyAlgorithm/releases/latest";
        public const string Website = "https://m51v5.com";
        public const string Telegram = "https://t.me/m51v5";
        public const string Contact = "https://m51v5.com/contact";

        /// <summary>The app's version as major.minor.patch.</summary>
        public static string Version
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
        }

        /// <summary>The bug form with the version already filled in.</summary>
        public static string ReportProblem => $"{Repository}/issues/new?template=bug_report.yml&version={Version}";

        public static void Open(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
