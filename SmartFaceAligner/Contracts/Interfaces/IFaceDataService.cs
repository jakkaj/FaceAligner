using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IFaceDataService
    {
        Task SetFaceData(FaceData f);
        Task<FaceData> GetFaceData(Project p, string fileName);
    }
}