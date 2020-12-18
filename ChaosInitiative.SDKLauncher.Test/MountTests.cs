using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SDKLauncher.Models;
using SDKLauncher.Util;
using Steamworks;

namespace ChaosInitiative.SDKLauncher.Test
{
    [TestFixture]
    public class MountTests
    {
        private Mount _fakeP2CeMount;
        
        
        [SetUp]
        public void Setup()
        {
            _fakeP2CeMount = new Mount
            {
                MountPath = "Portal 2 Community Edition",
                PrimarySearchPath = "p2ce",
                IsRequired = true,
                SelectedSearchPaths = { "p2ce" }
            };
        }

        [Test]
        // This exists mainly to satisfy code coverage
        public void TestProperties()
        {
            Assert.That(_fakeP2CeMount.IsRequired, Is.True);
            Assert.That(_fakeP2CeMount.PrimarySearchPath, Is.EqualTo("p2ce"));
            Assert.That(_fakeP2CeMount.MountPath, Is.EqualTo("Portal 2 Community Edition"));
            Assert.That(_fakeP2CeMount.SelectedSearchPaths.Count, Is.EqualTo(1));
            Assert.That(_fakeP2CeMount.SelectedSearchPaths, Has.One.Items.EqualTo("p2ce"));
        }

        [Test]
        public void TestPrimarySearchPathDirectory()
        {
            Assert.That(_fakeP2CeMount.PrimarySearchPathDirectory, Is.EqualTo("Portal 2 Community Edition/p2ce"));
        }

        [Test]
        public void TestIsValidSearchPath()
        {
            Directory.CreateDirectory(_fakeP2CeMount.PrimarySearchPathDirectory);
            File.WriteAllText($"{_fakeP2CeMount.PrimarySearchPathDirectory}/gameinfo.txt", "This is a fake gameinfo used for testing!");

            bool isValid = MountUtil.IsValidSearchPath(_fakeP2CeMount.PrimarySearchPathDirectory);
            Assert.That(isValid, Is.True);
            
            // Clean up
            Directory.Delete(_fakeP2CeMount.PrimarySearchPathDirectory, true);
            
            isValid = MountUtil.IsValidSearchPath(_fakeP2CeMount.PrimarySearchPathDirectory);
            Assert.That(isValid, Is.False);
        }

        [Test]
        //[Platform(Include = "Win,64-Bit-OS,Net")] TODO: Uncomment this once nunit version 3.13 releases! https://github.com/nunit/nunit/issues/3565
        public void TestPlatformStringWin64()
        {
            string platformString = MountUtil.GetPlatformString();
            Assert.That(platformString, Is.EqualTo("win64"));
        }

        [Test]
        public void TestBinDirectoryNotPlatformSpecific()
        {
            Mount csgo = new Mount
            {
                MountPath = "Counter Strike Global Offensive",
                IsRequired = true,
                PrimarySearchPath = "csgo",
                SelectedSearchPaths = {"csgo"}
            };

            Directory.CreateDirectory($"{csgo.MountPath}/bin");
            
            Assert.That(csgo.BinDirectory, Is.EqualTo("Counter Strike Global Offensive/bin"));
            
            Directory.Delete(csgo.MountPath, true);
        }
        
        [Test]
        //[Platform(Include = "Win,64-Bit-OS,Net")] TODO: Uncomment this once nunit version 3.13 releases! https://github.com/nunit/nunit/issues/3565
        public void TestBinDirectoryPlatformSpecific()
        {
            Mount momentum = new Mount
            {
                MountPath = "Momentum Mod",
                IsRequired = true,
                PrimarySearchPath = "momentum",
                SelectedSearchPaths = {"momentum"}
            };

            Directory.CreateDirectory($"{momentum.MountPath}/bin/win64");
            
            Assert.That(momentum.BinDirectory, Is.EqualTo("Momentum Mod/bin/win64"));
            
            // Clean up
            Directory.Delete(momentum.MountPath, true);
        }
    }
    
    [TestFixture]
    public class MountTestsWithSteam
    {
        
        [OneTimeSetUp]
        public void Setup()
        {
            Assume.That(() =>
            {
                SteamClient.Init(440000);
            }, Throws.Nothing);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Assume.That(SteamClient.Shutdown, Throws.Nothing);
        }
        
        [Test]
        public void TestValidMountPathDetection()
        {
            Mount mount = new Mount
            {
                AppId = 440000,
                IsRequired = true,
                PrimarySearchPath = "p2ce",
                SelectedSearchPaths = { "p2ce" }
            };
            var appId = mount.AppId;
            Assert.That(appId, Is.EqualTo(440000));

            string binDir = mount.MountPath;
            Assert.That(binDir, Is.Not.Empty);
            Assert.That(Directory.Exists(mount.BinDirectory), Is.True);
        }
        
        [Test]
        public void TestInvalidMountPathDetection()
        {
            Mount mount = new Mount
            {
                AppId = 69, // <== This is an invalid appid
                IsRequired = true,
                PrimarySearchPath = "p2ce",
                SelectedSearchPaths = { "p2ce" }
            };

            string binDir = mount.MountPath;
            Assert.That(binDir, Is.Null.Or.Empty);
        }

        [Test]
        public void TestAvailableSearchPaths()
        {
            Mount mount = new Mount
            {
                AppId = 620, // Portal 2
                IsRequired = true,
                PrimarySearchPath = "portal2"
            };
            
            Assume.That(() =>
            {
                bool isAppInstalled = SteamApps.IsAppInstalled(mount.AppId);
                Assume.That(isAppInstalled, Is.True);
            }, Throws.Nothing);
            
            List<string> availableSearchPaths = mount.AvailableSearchPaths;
            
            Assert.That(availableSearchPaths, Has.One.Items.EqualTo("portal2"));
            Assert.That(availableSearchPaths, Has.One.Items.EqualTo("portal2_dlc1"));
            Assert.That(availableSearchPaths, Has.One.Items.EqualTo("portal2_dlc2"));
        }
    }
}