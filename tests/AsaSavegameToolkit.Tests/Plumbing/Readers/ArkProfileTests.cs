using AsaSavegameToolkit.Plumbing.Readers;
using AsaSavegameToolkit.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsaSavegameToolkit.Tests.Plumbing.Readers
{
    [TestClass]
    public class ArkProfileTests : SaveTests
    {
        [TestMethod]
        public void ReadFrom_Version7()
        {
            ArkProfileReader reader = new ArkProfileReader(Path.Combine(TestSettings.AssetsDirectory, "version_14/"));
            var profiles = reader.Read();
            Assert.IsNotNull(profiles);
            Assert.AreNotEqual(0, profiles.Count);
        }
    }
}
