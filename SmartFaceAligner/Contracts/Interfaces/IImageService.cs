using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IImageService
    {
        Task<string> GetThumbFile(string fileName, CancellationToken token);
        (Image, double) GetImageFile(string fileName, bool doResize, CancellationToken token = default(CancellationToken));
        byte[] GetImageFileBytes(string fileName, bool doResize, CancellationToken token = default(CancellationToken));
    }
}