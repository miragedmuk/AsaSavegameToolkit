using AsaSavegameToolkit.Plumbing.Readers;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Porcelain;
using AsaSavegameToolkit.Tests.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AsaSavegameToolkit.Tests.Plumbing.Readers
{
    [TestClass]
    public class ArkTribeTests: SaveTests
    {
        [TestMethod]
        public void ReadFrom_Version7()
        {
            ArkTribeReader reader = new ArkTribeReader(Path.Combine(TestSettings.AssetsDirectory, "version_14/"));
            var tribes = reader.Read();
            Assert.IsNotNull(tribes);
            Assert.AreNotEqual(0, tribes.Count);
        }
    }
}
