﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
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
        public async Task CreateProject()
        {
            var pService = Resolve<IProjectService>();
            var fileRepo = Resolve<IFileRepo>();

            var testProject = await pService.GetProject("TestProject");

            Assert.IsNotNull(testProject);

            var pFile = await fileRepo.GetOffsetFile("TestProject", ProcessorContstants.FileNames.ProjectRoot);

            var fileInfo = new FileInfo(pFile);

            Assert.IsTrue(fileInfo.Exists);
        }

        [TestMethod]
        public async Task CreateFolderTypes()
        {
            var pService = Resolve<IProjectService>();
            var fileRepo = Resolve<IFileRepo>();

            var testProject = await pService.GetProject("TestProject");

            Assert.IsNotNull(testProject);

            var pFile = await fileRepo.GetOffsetFile("TestProject", ProcessorContstants.FileNames.ProjectRoot);

            var fileInfo = new FileInfo(pFile);

            Assert.IsTrue(fileInfo.Exists);

            var folderStaging = await pService.GetFolder(testProject, ProjectFolderTypes.Staging);
            Assert.IsTrue(Directory.Exists(folderStaging.FolderPath));

            var folderFildtered = await pService.GetFolder(testProject, ProjectFolderTypes.Filtered);
            Assert.IsTrue(Directory.Exists(folderFildtered.FolderPath));

            var folderAligned = await pService.GetFolder(testProject, ProjectFolderTypes.Aligned);
            Assert.IsTrue(Directory.Exists(folderAligned.FolderPath));

        }
    }
}