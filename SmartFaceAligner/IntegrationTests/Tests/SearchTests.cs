using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Contracts.Interfaces;
using IntegrationTests.Glue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Tests
{
    [TestClass]
    public class SearchTests : TestBase
    {
        [TestMethod]
        public async Task Search()
        {
            var search = Resolve<ISearchService>();

            var result = await search.SearchImages("Obama Portrait", 10, 0);

            Assert.AreEqual(result.value.Count, 10);
        }

        [TestMethod]
        public async Task SearchAndDownload()
        {
            var search = Resolve<ISearchService>();

            var searchString = "Obama";

            var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"FaceSystem\\{searchString}");

            if (!Directory.Exists(d))
            {
                Directory.CreateDirectory(d);
            }


            if (Debugger.IsAttached)
            {
                System.Diagnostics.Process.Start(d);
            }

            var result = await search.SearchAndDownload("Obama", d, 5000, 0);
            
            Assert.IsTrue(result > 0 );
            Debug.WriteLine(result);
        }
    }
}
