using System.Collections.Generic;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface IFaceService
    {
        void LocalDetectFaces(List<FaceData> faces);
    }
}