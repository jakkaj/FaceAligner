using System;
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

        public async Task PrepAlign(Project p)
        {
            await _fileManagementService.DeleteFiles(p, ProjectFolderTypes.RecPerson);
        }
 
        public async Task Align(Project p, FaceData face1, FaceData face2)
        {
            if (face1.Face == null || face2.Face == null)
            {
                return;
            }

            if (!FaceFilter.SimpleCheck(face1.Face, face2.Face))
            {
                return;
            }

            var a = new ImageAligner();

            var img1 = await _fileRepo.ReadBytes(face1.FileName);
            var img2 = await _fileRepo.ReadBytes(face2.FileName);

            var result = await a.AlignImages(img1, img2, face1.Face, face2.Face);

            if (result == null)
            {
                return;
            }

            var folderSave = await _projectService.GetFolder(p, ProjectFolderTypes.Aligned);

            await _fileRepo.Write(
                await _fileRepo.GetOffsetFile(folderSave.FolderPath, Path.GetFileName(face2.FileName)), result);
        }

        public async Task CognitiveDetectFace(Project p, FaceData face)
        {
            _logService.Log($"Parsing face: {face.FileName}");
            try
            {
                using (var img = Image.FromFile(face.FileName))
                {
                
                    using (var imgResized = ImageTools.ResizeImage(img, img.Width / 2, img.Height / 2))
                    {
                        var f = new FileInfo(Path.GetTempFileName());

                        imgResized.Save(f.FullName);

                        var fUse = face.FileName;

                        if (img.Width > 1280)
                        {
                            fUse = f.FullName;
                        }

                        using (var stream = await _fileRepo.ReadStream(fUse))
                        {
                            var fResult = await _cognitiveFaceService.ParseFace(p, stream);
                            if (fResult != null)
                            {
                                face.Face = fResult;
                            }
                            
                        }
                        f.Delete();
                        await _faceDataService.SetFaceData(face);
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                _logService.Log($"Response: {ex.ErrorCode}. {ex.ErrorMessage}");
            }
            catch (Exception ex)
            {
                _logService.Log("It's probably nothing: " + ex.ToString());
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
