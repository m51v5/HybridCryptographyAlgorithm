using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HybridCryptographyAlgorithm
{
    enum UpdateStatus
    {
        UpToDate,
        Available,
        Unreachable,
    }

    record UpdateResult(UpdateStatus Status, string? LatestTag = null, string? ReleaseUrl = null);

    /// <summary>Asks GitHub for the latest release. Reads only; never downloads anything.</summary>
    static class UpdateChecker
    {
        static readonly HttpClient Http = CreateClient();

        static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HybridCryptographyAlgorithm", AppLinks.Version));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
        }

        public static async Task<UpdateResult> CheckAsync()
        {
            try
            {
                using HttpResponseMessage response = await Http.GetAsync(AppLinks.LatestReleaseApi);

                // No release published yet.
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new UpdateResult(UpdateStatus.UpToDate);
                }
                response.EnsureSuccessStatusCode();

                using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                string tag = json.RootElement.GetProperty("tag_name").GetString() ?? "";
                string url = json.RootElement.GetProperty("html_url").GetString() ?? AppLinks.Releases;

                return IsNewer(tag, AppLinks.Version)
                    ? new UpdateResult(UpdateStatus.Available, tag, url)
                    : new UpdateResult(UpdateStatus.UpToDate, tag);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
            {
                return new UpdateResult(UpdateStatus.Unreachable);
            }
        }

        /// <summary>True when tag, like "v1.2.0", is a higher version than current.</summary>
        public static bool IsNewer(string tag, string current)
        {
            string clean = tag.TrimStart('v', 'V').Split('-', '+')[0];
            return Version.TryParse(clean, out Version? latest)
                && Version.TryParse(current, out Version? mine)
                && latest > mine;
        }
    }
}
