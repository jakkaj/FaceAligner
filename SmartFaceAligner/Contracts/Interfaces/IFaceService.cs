using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IFaceService
    {
        void LocalDetectFaces(List<FaceData> faces);
        Task<bool> SetPersonGroupPhotos(Project p, List<FaceData> faces);
        Task CognitiveDetectFace(Project p, FaceData face);
        Task Align(Project p, FaceData face1, FaceData face2);
        Task PrepAlign(Project p);
    }
}