using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using ExtensionGoo.Standard.Extensions;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using SmartFaceAligner.Processor.Entity;
using SmartFaceAligner.Processor.Messages;
using XamlingCore.Portable.Messages.XamlingMessenger;

namespace SmartFaceAligner.Processor.Services.FaceSmarts
{
    public class CognitiveServicesFaceService : ICognitiveServicesFaceService
    {
        private readonly FaceServiceClient _faceServiceClient;
        private readonly ILogService _logService;
        private readonly IFileRepo _fileRepo;
        private readonly IImageService _imageService;

        public CognitiveServicesFaceService(FaceServiceClient faceServiceClient,
            ILogService logService, IFileRepo fileRepo, 
            IImageService imageService)
        {
            _faceServiceClient = faceServiceClient;
            _logService = logService;
            _fileRepo = fileRepo;
            _imageService = imageService;
        }

        public async Task<List<ParsedFace>> ParseFace(Project p, byte[] image, Face[] parsedFacesExisting)
        {
            Face[] parsedFace = parsedFacesExisting;

            var _isThrottled = false;

            if (parsedFace == null)
            {
                try
                {
                    using (var ms = new MemoryStream(image))
                    {
                        parsedFace = await _faceServiceClient.DetectAsync(ms, true, true,
                        new FaceAttributeType[]
                        {
                            FaceAttributeType.Gender,
                            FaceAttributeType.Age,
                            FaceAttributeType.Smile,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.FacialHair,
                        });
                    }
                    
                }
                catch (FaceAPIException ex)
                {
                    if (ex.ErrorCode == Constants.Errors.RateLimitExceeded)
                    {
                        _logService.Log("Face API is throttling");
                        _isThrottled = true;
                        new ThrottlingMessage().Send();
                    }
                }
                catch (Exception ex)
                {
                    _logService.Log(ex.ToString());

                }
            }

            if (_isThrottled)
            {
                await Task.Delay(1000);
                return await ParseFace(p, image, parsedFacesExisting);
            }

            var listResults = new List<ParsedFace>();

            if (parsedFace != null && parsedFace.Length > 0)
            {
                var groupId = string.Format(Constants.CogServices.PersonGroupPattern, p.Id);

                IdentifyResult[] result = null;

                try
                {
                    result = await _faceServiceClient.IdentifyAsync(groupId, parsedFace.Select(_ => _.FaceId).Take(10).ToArray(), 5);
                }
                catch (FaceAPIException ex)
                {
                    if (ex.ErrorCode == Constants.Errors.RateLimitExceeded)
                    {
                        _logService.Log("Face API is throttling");
                        _isThrottled = true;
                        new ThrottlingMessage().Send();
                    }
                }
                catch (Exception ex)
                {
                    _logService.Log(ex.ToString());
                }

                if (_isThrottled)
                {
                    await Task.Delay(1000);
                    return await ParseFace(p, image, parsedFacesExisting);
                }

                if (result != null && result.Length > 0)
                {
                    foreach (var faceResult in result)
                    {
                        var parseResult = new ParsedFace
                        {
                            Face = parsedFace.FirstOrDefault(_ => _.FaceId == faceResult.FaceId)
                        };

                        //search for matching people we know about. 
                        var matchingPerson =
                            p.IdentityPeople.FirstOrDefault(
                                _ => faceResult.Candidates.Any(_2 => _2.PersonId == _.PersonId));
                        if (matchingPerson != null)
                        {
                            parseResult.IdentityPerson = matchingPerson;
                        }

                        listResults.Add(parseResult);
                    }

                    return listResults;
                }
            }

            return null;
        }

        public async Task RegisterPersonGroup(Project p, List<IdentityPerson> personGroups)
        {
            var groupId = string.Format(Constants.CogServices.PersonGroupPattern, p.Id);

            try
            {
                //var existing = await
                //    _faceServiceClient.GetPersonGroupAsync(groupId);
                await _faceServiceClient.DeletePersonGroupAsync(groupId);
            }
            catch
            {
                //all good that group doesnt exist
            }

            await _faceServiceClient.CreatePersonGroupAsync(groupId, groupId);

            foreach (var person in personGroups)
            {
                _logService.Log($"Request: Creating person \"{person.PersonName}\"");
                var createPersonResult = await _faceServiceClient.CreatePersonAsync(groupId, person.PersonName);
                var personId = createPersonResult.PersonId;
                person.PersonId = personId;

                //var Tasks

                async Task AddFace(FaceData face)
                {
                    try
                    {
                        var image = _imageService.GetImageFileBytes(face.FileName, true);

                        using (var stream = new MemoryStream(image))
                        {

                            _logService.Log($"Adding face: {face.FileName}");
                            try
                            {
                                var result = await _faceServiceClient.AddPersonFaceAsync(groupId, personId, stream);

                                _logService.Log($"Added face: {face.FileName}");

                                face.PersistedFaceId = result.PersistedFaceId;

                            }
                            catch (FaceAPIException ex)
                            {
                                _logService.Log(ex.ErrorMessage);
                                _logService.LogException(ex);
                            }
                            catch (Exception ex)
                            {
                                _logService.LogException(ex);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Log(ex.ToString());
                    }
                    
                }

                await person.Faces.WhenAllList(AddFace);
            }

            await _faceServiceClient.TrainPersonGroupAsync(groupId);

            // Wait until train completed
            while (true)
            {
                await Task.Delay(1000);
                var status = await _faceServiceClient.GetPersonGroupTrainingStatusAsync(groupId);
                _logService.Log($"Training: {status.Status}");
                if (status.Status != Status.Running)
                {
                    break;
                }
            }
        }
    }
}
