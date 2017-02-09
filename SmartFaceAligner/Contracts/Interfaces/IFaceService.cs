using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Entity;
using Microsoft.ProjectOxford.Face.Contract;

namespace Contracts.Interfaces
{
    public interface IFaceService
    {
        void LocalDetectFaces(List<FaceData> faces);
        
        Task<ValueTuple<bool, long>> CognitiveDetectFace(Project p, FaceData face);
        Task Align(Project p, FaceData faceData1, FaceData faceData2, Face face1, Face face2);
        Task PrepAlign(Project p);

        /// <summary>
        /// Send off to cognitive services for learning
        /// </summary>
        /// <param name="p"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        Task<bool> TrainPersonGroups(Project p);

        Task PostAlign(Project p);
    }
}