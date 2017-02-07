using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using IntegrationTests.Glue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Tests
{
    [TestClass]
    public class FaceRecogniseTests : TestBase
    {
        [TestMethod]
        public async Task SetUpFaces()
        {
            var pService = Resolve<IProjectService>();
            var fileManagementService = Resolve<IFileManagementService>();
            var faceDataService = Resolve<IFaceDataService>();
            var faceService = Resolve<IFaceService>();
            var fileRepo = Resolve<IFileRepo>();

            var fCogService = Resolve<ICognitiveServicesFaceService>();

            var p = await pService.OpenProject(@"C:\Users\jak\Documents\FaceProjects\OldApple");

            var faceFilesA = new string[]
            {
                "IMG_0086.JPG",
                "IMG_0143.JPG",
                "IMG_0971.JPG",
                "IMG_0134.JPG",
                "IMG_0128.JPG"
            };

            var faceFilesB = new string[]
            {
                "IMG_0089.JPG",
                "IMG_0207.JPG",
                "IMG_0204.JPG",
                "IMG_0292.JPG",
                "IMG_2086.JPG"
            };

            var files = await fileManagementService.GetFiles(p, ProjectFolderTypes.Staging);

            var faceDataA = new List<FaceData>();
            var faceDataB = new List<FaceData>();
            var dataAll = new List<FaceData>();

            foreach (var f in files)
            {
                var dataLoaded = await faceDataService.GetFaceData(p, f);
                dataAll.Add(dataLoaded);
                foreach (var ff in faceFilesA)
                {
                    if (f.IndexOf(ff) != -1)
                    {
                        var d = await faceDataService.GetFaceData(p, f);
                        faceDataA.Add(d);
                    }
                }

                foreach (var ff in faceFilesB)
                {
                    if (f.IndexOf(ff) != -1)
                    {
                        var d = await faceDataService.GetFaceData(p, f);
                        faceDataB.Add(d);
                    }
                }
            }

            if (p.IdentityPeople != null)
            {
                foreach (var person in p.IdentityPeople.ToArray())
                {
                    await pService.RemoveIdentityPerson(person);
                }
            }

            await pService.SetProject(p);

            var folder = await fileManagementService.GetFolder(p, ProjectFolderTypes.RecPerson);

            var directory = new DirectoryInfo(folder.FolderPath);


            Assert.IsTrue(directory.GetDirectories().Length == 0);

            var newGroupA =  await pService.AddNewIdentityPerson(p, "PersonA");

            foreach (var f in faceDataA)
            {
                await pService.AddImageToPerson(newGroupA, f);
            }

            var newGroupB = await pService.AddNewIdentityPerson(p, "PersonB");

            foreach (var f in faceDataB)
            {
                await pService.AddImageToPerson(newGroupB, f);
            }

            await faceService.TrainPersonGroups(p);

            Assert.IsTrue(directory.GetDirectories().Length == 2);
        }

        [TestMethod]
        public async Task ParseFaces()
        {
            var pService = Resolve<IProjectService>();
            var fileManagementService = Resolve<IFileManagementService>();
            var faceDataService = Resolve<IFaceDataService>();
            var faceService = Resolve<IFaceService>();
            var fileRepo = Resolve<IFileRepo>();

           

            var p = await pService.OpenProject(@"C:\Users\jak\Documents\FaceProjects\OldApple");

      

            var files = await fileManagementService.GetFiles(p, ProjectFolderTypes.Staging);
           
            var dataAll = new List<FaceData>();

            foreach (var f in files)
            {
                dataAll.Add(await faceDataService.GetFaceData(p, f));
            }

            foreach (var faceData in dataAll)
            {
                await faceService.CognitiveDetectFace(p, faceData);

            }
        }

    }
}
