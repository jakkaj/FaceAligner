using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IImageService
    {
        Task<string> GetThumbFile(string fileName);
        byte[] GetImageFile(string fileName);
    }
}