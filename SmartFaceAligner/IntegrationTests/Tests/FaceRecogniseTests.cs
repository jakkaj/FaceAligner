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

            var p = await pService.OpenProject(@"C:\Users\jakka\Documents\FaceSystem\Apple");

            var faceFiles = new string[]
            {
                "IMG_0086.JPG",
                "IMG_0143.JPG",
                "IMG_0971.JPG",
                "IMG_0134.JPG",
                "IMG_0128.JPG"
            };

            var files = await fileManagementService.GetFiles(p, ProjectFolderTypes.Staging);

            var data = new List<FaceData>();
            var dataAll = new List<FaceData>();

            foreach (var f in files)
            {
                foreach (var ff in faceFiles)
                {
                    if (f.IndexOf(ff) != -1)
                    {
                        var d = await faceDataService.GetFaceData(f);
                        data.Add(d);
                    }
                    else
                    {
                        dataAll.Add(await faceDataService.GetFaceData(f));
                    }
                }
            }


            await faceService.SetPersonGroupPhotos(p, data);
            var folder = await pService.GetFolder(p, ProjectFolderTypes.RecPerson);

            var directory = new DirectoryInfo(folder.FolderPath);


            Assert.IsTrue(directory.GetFiles().Length == 5);


            foreach (var faceData in dataAll)
            {
                await faceService.CognitiveDetectFace(p, faceData);

            }
        }

    }
}
