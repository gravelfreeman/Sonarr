using System;
using System.IO;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]

    public class DeleteFileFixture : CoreTest
    {
        private void WithRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinEnabled).Returns(true);
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolder(It.IsAny<string>()))
                  .Returns(new RootFolder { Path = @"/media/library/tv".AsOsAgnostic(), RecycleBinEnabled = true });
        }

        private void WithoutRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinEnabled).Returns(false);
        }

        [Test]
        public void should_use_delete_when_recycleBin_is_not_configured()
        {
            WithoutRecycleBin();

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(path), Times.Once());
        }

        [Test]
        public void should_use_move_when_recycleBin_is_configured()
        {
            WithRecycleBin();

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskTransferService>().Verify(v => v.TransferFile(path, RecycleBinPathBuilder.GetRecycleBinDestination(path), TransferMode.Move, false), Times.Once());
        }

        [Test]
        public void should_use_alternative_name_if_already_exists()
        {
            WithRecycleBin();

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();
            var recycleBinPath = RecycleBinPathBuilder.GetRecycleBinDestination(path);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(recycleBinPath))
                .Returns(true);

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskTransferService>().Verify(v => v.TransferFile(path, Path.Combine(Path.GetDirectoryName(recycleBinPath), "S01E01_2.avi"), TransferMode.Move, false), Times.Once());
        }

        [Test]
        public void should_call_fileSetLastWriteTime_for_each_file()
        {
            WindowsOnly();
            WithRecycleBin();
            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FileSetLastWriteTime(RecycleBinPathBuilder.GetRecycleBinDestination(path), It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        public void should_use_subfolder_when_passed_in()
        {
            WithRecycleBin();

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path, "30 Rock");

            Mocker.GetMock<IDiskTransferService>().Verify(v => v.TransferFile(path, RecycleBinPathBuilder.GetRecycleBinDestination(path), TransferMode.Move, false), Times.Once());
        }

        [Test]
        public void should_delete_permanently_when_root_folder_recycle_bin_is_disabled()
        {
            WithRecycleBin();
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolder(It.IsAny<string>()))
                  .Returns(new RootFolder { RecycleBinEnabled = false });

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(path), Times.Once());
            Mocker.GetMock<IDiskTransferService>().Verify(v => v.TransferFile(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move, false), Times.Never());
        }

        [Test]
        public void should_delete_permanently_when_mode_is_upgrades_only()
        {
            WithRecycleBin();
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBinMode).Returns(RecycleBinMode.UpgradesOnly);

            var path = @"/media/library/tv/30 Rock/S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path, string.Empty, RecycleBinOperation.Delete);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(path), Times.Once());
            Mocker.GetMock<IDiskTransferService>().Verify(v => v.TransferFile(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move, false), Times.Never());
        }
    }
}
