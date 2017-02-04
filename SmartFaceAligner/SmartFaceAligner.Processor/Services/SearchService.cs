using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BingLibrary.BingSdkFromSwaggerClient;
using Contracts;
using Contracts.Entity;
using Contracts.Interfaces;
using Newtonsoft.Json;
using XamlingCore.Portable.Util.TaskUtils;

namespace SmartFaceAligner.Processor.Services
{
    public class SearchService : ISearchService
    {
        private readonly IBingSdkFromSwaggerClient _bingSearch;
        private readonly IConfigurationService _configService;

        public SearchService(IBingSdkFromSwaggerClient bingSearch, IConfigurationService configService)
        {
            _bingSearch = bingSearch;
            _configService = configService;
        }

        public async Task<BingSearchResult> SearchImages(string image, int count, int offset = 0)
        {
            var result = await _bingSearch.SearchWithHttpMessagesAsync(image, count, offset, null, "Strict", "Share",
                _configService.BingSearchSubscriptionKey,
                _configService.BingSearchSubscriptionKey, null,
                default(CancellationToken));

            if (!result.Response.IsSuccessStatusCode)
            {
                return null;
            }

            var results =
                JsonConvert.DeserializeObject<BingSearchResult>(await result.Response.Content.ReadAsStringAsync());

            return results;
        }

        public async Task<int> SearchAndDownload(string image, string filePath, int count, int offset = 0, CancellationToken token = default(CancellationToken))
        {


            var dir = new DirectoryInfo(filePath);

            if (!dir.Exists)
            {
                dir.Create();
            }

            var client = new HttpClient();

            var downloadedCount = 0;

            var allResults = new List<bool>();

            var thisCount = count > 100 ? 100 : count;

            while (allResults.Count < count)
            {
                var results = await SearchImages(image, thisCount, allResults.Count);

                if (results == null)
                {
                    return 0;
                }

                async Task<bool> DownloadAndSave(string url, string fileName)
                {
                    var f = Path.Combine(dir.FullName, fileName);

                    Debug.WriteLine(url);

                    try
                    {
                        if (File.Exists(f))
                        {
                            return true;
                        }

                        var downloadResult = await client.GetByteArrayAsync(url);

                        File.WriteAllBytes(f, downloadResult);
                        downloadedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }

                    return true;

                }

                var tasks = new List<Task<bool>>();

                foreach (var r in results.value)
                {
                    var url = r.contentUrl;
                    tasks.Add(TaskThrottler.Get("Downloader", 20).Throttle(() => DownloadAndSave(url, $"{r.imageId}.{r.encodingFormat}")));
                }

                allResults.AddRange(await Task.WhenAll(tasks));

                if (results.value.Count < 100)
                {
                    break;
                }
            }

            return downloadedCount;
        }

    }
}
