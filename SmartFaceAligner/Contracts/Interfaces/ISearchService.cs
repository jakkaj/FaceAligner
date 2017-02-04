using System.Threading;
using Contracts.Entity;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ISearchService
    {
        Task<BingSearchResult> SearchImages(string image, int count, int offset = 0);
        Task<int> SearchAndDownload(string image, string filePath, int count, int offset = 0, CancellationToken token = default(CancellationToken));
    }
}