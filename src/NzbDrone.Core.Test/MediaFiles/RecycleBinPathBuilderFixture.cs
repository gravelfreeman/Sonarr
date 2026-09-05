using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class RecycleBinPathBuilderFixture
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
    }
}
