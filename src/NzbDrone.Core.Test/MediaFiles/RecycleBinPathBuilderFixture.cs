using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class RecycleBinPathBuilderFixture : TestBase
    {
        [Test]
        public void should_share_bin_at_top_level_folder()
        {
            var path = @"/media/library/tv/30 Rock/S01E01.avi";

            RecycleBinPathBuilder.GetRecycleBinPath(path).Should().Be("/media/.bin");
            RecycleBinPathBuilder.GetRecycleBinDestination(path).Should().Be("/media/.bin/library/tv/30 Rock/S01E01.avi");
        }

        [Test]
        public void should_return_null_for_empty_path()
        {
            RecycleBinPathBuilder.GetRecycleBinDestination(string.Empty).Should().BeNull();
        }

        [Test]
        public void should_preserve_windows_drive_structure()
        {
            WindowsOnly();

            var path = @"C:\media\library\tv\Series\Episode.mkv";

            RecycleBinPathBuilder.GetRecycleBinPath(path).Should().Be(@"C:\media\.bin");
            RecycleBinPathBuilder.GetRecycleBinDestination(path).Should().Be(@"C:\media\.bin\library\tv\Series\Episode.mkv");
        }

        [Test]
        public void should_preserve_windows_unc_structure()
        {
            WindowsOnly();

            var path = @"\\server\share\library\tv\Series\Episode.mkv";

            RecycleBinPathBuilder.GetRecycleBinPath(path).Should().Be(@"\\server\share\library\.bin");
            RecycleBinPathBuilder.GetRecycleBinDestination(path).Should().Be(@"\\server\share\library\.bin\tv\Series\Episode.mkv");
        }
    }
}
