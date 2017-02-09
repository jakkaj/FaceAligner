using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IFaceDataService
    {
        Task SetFaceData(FaceData f, bool save = true);
        Task<FaceData> GetFaceData(Project p, string fileName);
        Task<List<FaceData>> GetFaceData(Project p );
        Task Save(Project p);
    }
}