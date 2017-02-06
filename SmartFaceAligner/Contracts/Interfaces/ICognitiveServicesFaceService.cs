using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Contracts.Entity;
using Microsoft.ProjectOxford.Face.Contract;

namespace Contracts.Interfaces
{
    public interface ICognitiveServicesFaceService
    {
        Task RegisterPersonGroup(Project p, List<FaceData> faces);
        Task<Face> ParseFace(Project p, Stream image);
    }
}