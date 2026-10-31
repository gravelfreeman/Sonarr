using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class RecycleBinPathBuilderFixture : TestBase
    {
        [TestCase("/media", "/media/.bin")]
        [TestCase("/", "/.bin")]
        [TestCase("/mnt/media", "/mnt/media/.bin")]
        public void should_build_recycle_bin_path(string mountPath, string expected)
        {
            PosixOnly();

            RecycleBinPathBuilder.GetRecycleBinPath(mountPath).Should().Be(expected);
        }

        [TestCase("/media/library/tv/30 Rock/S01E01.avi", "/media", "/media/.bin/library/tv/30 Rock/S01E01.avi")]
        [TestCase("/", "/", "/.bin")]
        [TestCase("/episode.mkv", "/", "/.bin/episode.mkv")]
        [TestCase("/mnt/media/tv/30 Rock/S01E01.avi", "/mnt/media", "/mnt/media/.bin/tv/30 Rock/S01E01.avi")]
        public void should_build_recycle_bin_destination(string path, string mountPath, string expected)
        {
            PosixOnly();

            RecycleBinPathBuilder.GetRecycleBinDestination(path, mountPath).Should().Be(expected);
        }

        [TestCase("", "/media")]
        [TestCase("/srv/tv/episode.mkv", "/mnt/storage")]
        public void should_return_null_for_an_invalid_destination(string path, string mountPath)
        {
            PosixOnly();

            RecycleBinPathBuilder.GetRecycleBinDestination(path, mountPath).Should().BeNull();
        }

        [Test]
        public void should_preserve_windows_path_structure()
        {
            WindowsOnly();

            RecycleBinPathBuilder.GetRecycleBinDestination(@"C:\media\library\tv\Series\Episode.mkv", @"C:\media").Should().Be(@"C:\media\.bin\library\tv\Series\Episode.mkv");
            RecycleBinPathBuilder.GetRecycleBinDestination(@"D:\", @"D:\").Should().Be(@"D:\.bin");
            RecycleBinPathBuilder.GetRecycleBinDestination(@"D:\Episode.mkv", @"D:\").Should().Be(@"D:\.bin\Episode.mkv");
            RecycleBinPathBuilder.GetRecycleBinDestination(@"\\server\share\library\tv\Series\Episode.mkv", @"\\server\share\library").Should().Be(@"\\server\share\library\.bin\tv\Series\Episode.mkv");
        }
    }
}
