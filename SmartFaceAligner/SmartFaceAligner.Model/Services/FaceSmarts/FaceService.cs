using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
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
 
        public async Task Align(Project p, FaceData faceData1, FaceData faceData2, Face face1, Face face2)
        {
            if (face1 == null || face2 == null)
            {
                return;
            }

            if (!FaceFilter.SimpleCheck(face1, face2))
            {
                return;
            }

            var a = new ImageAligner();

            var img1 = await _fileRepo.ReadBytes(faceData1.FileName);
            var img2 = await _fileRepo.ReadBytes(faceData2.FileName);

            var result = await a.AlignImages(img1, img2, face1, face2);

            if (result == null)
            {
                return;
            }

            var folderSave = await _fileManagementService.GetFolder(p, ProjectFolderTypes.Aligned);

            await _fileRepo.Write(
                await _fileRepo.GetOffsetFile(folderSave.FolderPath, Path.GetFileName(faceData2.FileName)), result);
        }

        public async Task<(bool, long)> CognitiveDetectFace(Project p, FaceData face)
        {
            _logService.Log($"Parsing face: {face.FileName}");
            _logService.Log($"Parsing face: {face.FileName}");
            try
            {
                using (var img = Image.FromFile(face.FileName))
                {
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Jpeg);
                        var fUse = new FileInfo(Path.GetTempFileName());
                        img.Save(fUse.FullName, ImageFormat.Jpeg);

                        var len = ms.Length;

                        var resultLocalCheck = LocalFaceDetector.HasFace(fUse.FullName);
                        if (!resultLocalCheck)
                        {
                            face.HasBeenScanned = true;
                            await _faceDataService.SetFaceData(face);
                            _logService.Log($"Skipping becasue no face detected local: {face.FileName}");
                            return (false, 0);
                        }

                        _logService.Log($"Uploading: {len} bytes");

                        var fResult = await _cognitiveFaceService.ParseFace(p, ms);

                        if (fResult != null)
                        {
                            face.ParsedFaces = fResult.ToArray();
                        }
                        face.HasBeenScanned = true;

                        fUse.Delete();

                        await _faceDataService.SetFaceData(face);
                        return (true, len);
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                _logService.Log($"Cognitive response: {ex.ErrorCode}. {ex.ErrorMessage}");
            }
            catch (Exception ex)
            {
                _logService.Log("It's probably nothing: " + ex.ToString());
            }

            return (false, 0);

        }

        public void LocalDetectFaces(List<FaceData> faces)
        {
            var trimmed = faces.Where(_ => !_.HasBeenScanned);

            foreach (var item in trimmed)
            {
                var result = LocalFaceDetector.HasFace(item.FileName);
                item.HasBeenScanned = result;
                _faceDataService.SetFaceData(item);
            }
        }

        /// <summary>
        /// Send off to cognitive services for learning
        /// </summary>
        /// <param name="p"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public async Task<bool> TrainPersonGroups(Project p)
        {
            await _cognitiveFaceService.RegisterPersonGroup(p, p.IdentityPeople);
            await _projectService.SetProject(p);

            return true;
        }

        
    }

}
