using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;
using IntegrationTests.Glue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartFaceAligner.Processor.Entity;

namespace IntegrationTests.Tests
{
    [TestClass]
    public class ProjectTests : TestBase
    {
        [TestMethod]
        public async Task TestCreate()
        {
            var pService = Resolve<IProjectService>();
            var fileRepo = Resolve<IFileRepo>();

            var testProject = await pService.GetProject("TestProject");

            Assert.IsNotNull(testProject);

            var pFile = await fileRepo.GetOffsetFile("TestProject", ProcessorContstants.FileNames.ProjectRoot);

            var fileInfo = new FileInfo(pFile);

            Assert.IsTrue(fileInfo.Exists);
        }
    }
}
