using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IPersonGroupService
    {
        Task CopyTo(FaceData faceData, Project p, string groupName);
    }
}