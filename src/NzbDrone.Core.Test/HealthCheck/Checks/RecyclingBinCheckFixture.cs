using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RecyclingBinCheckFixture : CoreTest<RecyclingBinCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinEnabled).Returns(true);
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                  .Returns("Some Error Message");
        }

        private void GivenMount(string path)
        {
            var mount = new Mock<IMount>();
            mount.SetupGet(s => s.RootDirectory).Returns(path);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetMount(It.IsAny<string>())).Returns(mount.Object);
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void should_not_check_paths_when_recycle_bin_is_disabled(bool recycleBinEnabled, bool rootFolderRecycleBinEnabled)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinEnabled).Returns(recycleBinEnabled);
            Mocker.GetMock<IRootFolderService>().Setup(s => s.All()).Returns(new List<RootFolder>
            {
                new RootFolder { Path = "/media/library/tv", RecycleBinEnabled = rootFolderRecycleBinEnabled }
            });

            Subject.Check().ShouldBeOk();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderWritable(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_check_top_level_folder_when_shared_bin_does_not_exist()
        {
            PosixOnly();

            Mocker.GetMock<IRootFolderService>().Setup(s => s.All()).Returns(new List<RootFolder>
            {
                new RootFolder { Path = "/media/library/tv/hd", RecycleBinEnabled = true },
                new RootFolder { Path = "/media/library/tv/sd", RecycleBinEnabled = true }
            });
            GivenMount("/media");

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists("/media/.bin")).Returns(false);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderWritable("/media")).Returns(true);

            Subject.Check().ShouldBeOk();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderWritable("/media"), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderWritable("/media/.bin"), Times.Never());
        }

        [Test]
        public void should_check_volume_root_when_shared_bin_does_not_exist()
        {
            PosixOnly();

            Mocker.GetMock<IRootFolderService>().Setup(s => s.All()).Returns(new List<RootFolder>
            {
                new RootFolder { Path = "/", RecycleBinEnabled = true }
            });
            GivenMount("/");

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists("/.bin")).Returns(false);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderWritable("/")).Returns(true);

            Subject.Check().ShouldBeOk();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderWritable("/"), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.FolderWritable("/.bin"), Times.Never());
        }
    }
}
