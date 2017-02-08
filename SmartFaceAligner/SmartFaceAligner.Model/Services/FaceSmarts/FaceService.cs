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
            await _fileManagementService.DeleteFiles(p, ProjectFolderTypes.Aligned);
        }

        public async Task Align(Project p, FaceData faceData1, FaceData faceData2, Face face1, Face face2)
        {

            var folderSave = await _fileManagementService.GetFolder(p, ProjectFolderTypes.Aligned);



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

            var fSource = await _fileRepo.GetOffsetFile(folderSave.FolderPath, Path.GetFileName(faceData1.FileName));

            if (!await _fileRepo.FileExists(fSource))
            {
                await _fileRepo.Write(fSource, _crop(img1, face1));
            }

            var result = await a.AlignImages(img1, img2, face1, face2);

            if (result == null)
            {
                return;
            }

            await _fileRepo.Write(
                await _fileRepo.GetOffsetFile(folderSave.FolderPath, Path.GetFileName(faceData2.FileName)), _crop(result, face1));
        }

        byte[] _crop(byte[] img, Face face)
        {
            using (var ms = new MemoryStream(img))
            {
                Rectangle cropRect = new Rectangle
                {
                    Height = face.FaceRectangle.Height + 100,
                    Width = face.FaceRectangle.Width + 100,

                    X = face.FaceRectangle.Left - 50,
                    Y = face.FaceRectangle.Top - 50

                };


                Bitmap src = Image.FromStream(ms) as Bitmap;
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }

                using (var ms2 = new MemoryStream())
                {
                    target.Save(ms2, ImageFormat.Jpeg);
                    return ms2.ToArray();
                }

            }

        }

        public async Task<(bool, long)> CognitiveDetectFace(Project p, FaceData face)
        {
            _logService.Log($"Parsing face: {face.FileName}");
            try
            {
                using (var img = Image.FromFile(face.FileName))
                {

                    var imgResizeData = ImageTools.ResizeImage(img, img.Width > 1280 ? 1280 : img.Width);
                    var imgResized = imgResizeData.Item1;

                    var f = new FileInfo(Path.GetTempFileName());

                    imgResized.Save(f.FullName, ImageFormat.Jpeg);

                    var fUse = f.FullName;

                    var len = new FileInfo(fUse).Length;

                    var resultLocalCheck = LocalFaceDetector.HasFace(fUse);
                    if (!resultLocalCheck)
                    {
                        face.HasBeenScanned = true;
                        await _faceDataService.SetFaceData(face);
                        _logService.Log($"Skipping becasue no face detected local: {face.FileName}");
                        return (false, 0);
                    }

                    _logService.Log($"Uploading: {len} bytes");


                    using (var stream = await _fileRepo.ReadStream(fUse))
                    {
                        var fResult = await _cognitiveFaceService.ParseFace(p, stream);

                        if (fResult != null)
                        {

                            face.ParsedFaces = fResult.ToArray();
                            _adjustImageLandmarks(face.ParsedFaces, imgResizeData.Item2);
                        }

                        face.HasBeenScanned = true;
                    }

                    //if (face.ParsedFaces != null)
                    //{
                    //    using (var stream = await _fileRepo.ReadStream(face.FileName))
                    //    {
                    //        var fResult = await _cognitiveFaceService.ParseFace(p, stream);

                    //        if (fResult != null)
                    //        {

                    //            //face.ParsedFaces = fResult.ToArray();
                    //            //_adjustImageLandmarks(face.ParsedFaces, imgResizeData.Item2);
                    //        }

                    //        face.HasBeenScanned = true;
                    //    }
                    //}



                    f.Delete();

                    await _faceDataService.SetFaceData(face);
                    return (true, len);
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

        /// <summary>
        /// The image we sent to the server may have been reduced so we need to correc the points. 
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="aspect"></param>
        void _adjustImageLandmarks(ParsedFace[] faces, double aspect)
        {
            foreach (var f in faces)
            {
                //thank goodness for OSS (https://github.com/Microsoft/ProjectOxford-ClientSDK/blob/master/Face/Windows/ClientLibrary/Contract/FaceLandmarks.cs)

                var l = f.Face.FaceLandmarks;

                f.Face.FaceRectangle = new FaceRectangle
                {
                    Height = Convert.ToInt32(Convert.ToDouble(f.Face.FaceRectangle.Height) * aspect),
                    Width = Convert.ToInt32(Convert.ToDouble(f.Face.FaceRectangle.Width) * aspect),
                    Top = Convert.ToInt32(Convert.ToDouble(f.Face.FaceRectangle.Top) * aspect),
                    Left = Convert.ToInt32(Convert.ToDouble(f.Face.FaceRectangle.Left) * aspect),
                };

                _fixFeatureCoordinate(l.PupilLeft, aspect);
                _fixFeatureCoordinate(l.PupilRight, aspect);
                _fixFeatureCoordinate(l.NoseTip, aspect);
                _fixFeatureCoordinate(l.MouthLeft, aspect);
                _fixFeatureCoordinate(l.MouthRight, aspect);
                _fixFeatureCoordinate(l.EyebrowLeftOuter, aspect);
                _fixFeatureCoordinate(l.EyebrowLeftInner, aspect);
                _fixFeatureCoordinate(l.EyeLeftOuter, aspect);
                _fixFeatureCoordinate(l.EyeLeftTop, aspect);
                _fixFeatureCoordinate(l.EyeLeftBottom, aspect);
                _fixFeatureCoordinate(l.EyeLeftInner, aspect);
                _fixFeatureCoordinate(l.EyebrowRightInner, aspect);
                _fixFeatureCoordinate(l.EyebrowRightOuter, aspect);
                _fixFeatureCoordinate(l.EyeRightInner, aspect);
                _fixFeatureCoordinate(l.EyeRightTop, aspect);
                _fixFeatureCoordinate(l.EyeRightBottom, aspect);
                _fixFeatureCoordinate(l.EyeRightOuter, aspect);
                _fixFeatureCoordinate(l.NoseRootLeft, aspect);
                _fixFeatureCoordinate(l.NoseRootRight, aspect);
                _fixFeatureCoordinate(l.NoseLeftAlarTop, aspect);
                _fixFeatureCoordinate(l.NoseRightAlarTop, aspect);
                _fixFeatureCoordinate(l.NoseLeftAlarOutTip, aspect);
                _fixFeatureCoordinate(l.NoseRightAlarOutTip, aspect);
                _fixFeatureCoordinate(l.UpperLipTop, aspect);
                _fixFeatureCoordinate(l.UpperLipBottom, aspect);
                _fixFeatureCoordinate(l.UnderLipTop, aspect);
                _fixFeatureCoordinate(l.UnderLipBottom, aspect);
            }
        }

        void _fixFeatureCoordinate(FeatureCoordinate coord, double aspect)
        {
            coord.X = coord.X * aspect;
            coord.Y = coord.Y * aspect;
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
