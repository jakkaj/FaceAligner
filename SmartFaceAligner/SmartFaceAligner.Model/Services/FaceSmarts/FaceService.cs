using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private readonly IProjectService _projectService;
        private readonly ICognitiveServicesFaceService _cognitiveFaceService;
        private readonly IFileRepo _fileRepo;
        private readonly ILogService _logService;

        public FaceService(FaceServiceClient faceServiceClient,
            IFaceDataService faceDataService, 
            IFileManagementService fileManagementService, 
            IProjectService projectService,
            ICognitiveServicesFaceService cognitiveFaceService,
            IFileRepo fileRepo,
            ILogService logService)
        {
            _faceServiceClient = faceServiceClient;
            _faceDataService = faceDataService;
            _fileManagementService = fileManagementService;
            _projectService = projectService;
            _cognitiveFaceService = cognitiveFaceService;
            _fileRepo = fileRepo;
            _logService = logService;
        }

        public async Task CognitiveDetectFace(Project p, FaceData face)
        {
            _logService.Log($"Parsing face: {face.FileName}");
            using (var img = Image.FromFile(face.FileName))
            {
                using (var imgResized = ImageTools.ResizeImage(img, img.Width / 4, img.Height / 4))
                {
                    var f = new FileInfo(Path.GetTempFileName());

                    imgResized.Save(f.FullName);

                    using (var stream = await _fileRepo.ReadStream(f.FullName))
                    {
                        var fResult = await _cognitiveFaceService.ParseFace(p, stream);
                        if (fResult != null)
                        {
                            var isFace = p.PersonId == fResult.FaceId;
                        }
                        face.Face = fResult;
                    }
                    f.Delete();
                    await _faceDataService.SetFaceData(face);
                }
            }
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

            await _cognitiveFaceService.RegisterPersonGroup(p, faces);

            await _projectService.SetProject(p);

            return true;
        }

        
    }

}
