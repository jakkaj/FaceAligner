using System.Drawing;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IImageService
    {
        Task<string> GetThumbFile(string fileName);
        Image GetImageFile(string fileName);
        byte[] GetImageFileBytes(string fileName);
    }
}