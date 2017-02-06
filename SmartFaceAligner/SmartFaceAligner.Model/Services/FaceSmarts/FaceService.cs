using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using Microsoft.ProjectOxford.Face;
using SmartFaceAligner.Processor.Entity;
using StartFaceAligner.FaceSmarts;

namespace SmartFaceAligner.Processor.Services.FaceSmarts
{
    public class FaceService : IFaceService
    {
        private readonly FaceServiceClient _faceServiceClient;
        private readonly IFaceDataService _faceDataService;
        private readonly IFileManagementService _fileManagementService;

        public FaceService(FaceServiceClient faceServiceClient, IFaceDataService faceDataService, IFileManagementService fileManagementService)
        {
            _faceServiceClient = faceServiceClient;
            _faceDataService = faceDataService;
            _fileManagementService = fileManagementService;
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

        public async Task<bool> SetPersonGroupPhotos(Project p, List<FaceData> faces)
        {
            await _fileManagementService.DeleteFiles(p, ProjectFolderTypes.RecPerson);

            foreach (var f in faces)
            {
                await _fileManagementService.CopyToFolder(p, f.FileName, ProjectFolderTypes.RecPerson);
            }

            return true;
        }

        //public async Task<bool> CreateFaceGroup(string baseDirectory)
        //{

        //}
    }
}
