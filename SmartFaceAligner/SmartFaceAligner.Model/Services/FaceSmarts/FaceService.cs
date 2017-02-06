using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using Microsoft.ProjectOxford.Face;
using StartFaceAligner.FaceSmarts;

namespace SmartFaceAligner.Processor.Services.FaceSmarts
{
    public class FaceService : IFaceService
    {
        private readonly FaceServiceClient _faceServiceClient;
        private readonly IFaceDataService _faceDataService;

        public FaceService(FaceServiceClient faceServiceClient, IFaceDataService faceDataService)
        {
            _faceServiceClient = faceServiceClient;
            _faceDataService = faceDataService;
        }

        public void LocalDetectFaces(List<FaceData> faces)
        {
            var trimmed = faces.Where(_ => !_.HasFace.HasValue);

            foreach (var item in trimmed)
            {
                var result = LocalFaceDetector.HasFace(item.FileName);
                item.HasFace = result;
                _faceDataService.SetFaceData(item);
            }
        }

        //public async Task<bool> CreateFaceGroup(string baseDirectory)
        //{
            
        //}
    }
}
