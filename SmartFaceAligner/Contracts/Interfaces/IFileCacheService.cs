using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IFileCacheService
    {
        Task<byte[]> ReadCache(string originalFile, string type);
        Task<string> SaveCache(string originalFile, string type, byte[] data);
        Task<string> GetCacheFileName(string originalFile, string type);
        Task<bool> ExistsInCache(string originalFile, string type);
    }
}