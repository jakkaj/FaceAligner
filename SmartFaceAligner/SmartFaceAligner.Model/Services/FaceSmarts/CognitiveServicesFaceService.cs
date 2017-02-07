using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
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

namespace SmartFaceAligner.Processor.Services.FaceSmarts
{
    public class CognitiveServicesFaceService : ICognitiveServicesFaceService
    {
        private readonly FaceServiceClient _faceServiceClient;
        private readonly ILogService _logService;
        private readonly IFileRepo _fileRepo;

        public CognitiveServicesFaceService(FaceServiceClient faceServiceClient, 
            ILogService logService, IFileRepo fileRepo)
        {
            _faceServiceClient = faceServiceClient;
            _logService = logService;
            _fileRepo = fileRepo;
        }

        public async Task<List<ParsedFace>> ParseFace(Project p, Stream image)
        {
            var parsedFace = await _faceServiceClient.DetectAsync(image, true, true,
                new FaceAttributeType[]
                {
                    //FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    //FaceAttributeType.Smile,
                    //FaceAttributeType.Glasses,
                    FaceAttributeType.HeadPose,
                    //FaceAttributeType.FacialHair,
                    
                });

            var listResults = new List<ParsedFace>();

            if (parsedFace != null && parsedFace.Length > 0)
            {
                var groupId = string.Format(Constants.CogServices.PersonGroupPattern, p.Id);
                var result = await _faceServiceClient.IdentifyAsync(groupId, parsedFace.Select(_ => _.FaceId).ToArray());
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
                _logService.Log($"Request: Creating person \"{groupId}\"");
                var createPersonResult = await _faceServiceClient.CreatePersonAsync(groupId, Constants.CogServices.DefaultPerson);
                var personId = createPersonResult.PersonId;
                person.PersonId = personId;
               
                //var Tasks

                async Task AddFace(FaceData face)
                {
                    using (var stream = await _fileRepo.ReadStream(face.FileName))
                    {
                        _logService.Log($"Adding face: {face.FileName}");
                        var result = await _faceServiceClient.AddPersonFaceAsync(groupId, personId, stream);

                        _logService.Log($"Added face: {face.FileName}");

                        face.PersistedFaceId = result.PersistedFaceId;
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
