using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraFileServiceFixture : CoreTest<OtherExtraFileService>
    {
        private Series _series;
        private EpisodeFile _episodeFile;
        private OtherExtraFile _extraFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Id = 1)
                                     .With(s => s.Path = @"/series")
                                     .Build();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Id = 2)
                                               .With(f => f.SeriesId = _series.Id)
                                               .Build();

            _extraFile = Builder<OtherExtraFile>.CreateNew()
                                                 .With(f => f.EpisodeFileId = _episodeFile.Id)
                                                 .With(f => f.RelativePath = Path.Combine("Season 1", "episode.nfo"))
                                                 .Build();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);

            Mocker.GetMock<IExtraFileRepository<OtherExtraFile>>()
                  .Setup(r => r.GetFilesByEpisodeFile(_episodeFile.Id))
                  .Returns(new[] { _extraFile }.ToList());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(d => d.FileExists(Path.Combine(_series.Path, _extraFile.RelativePath)))
                  .Returns(true);
        }

        [TestCase(DeleteMediaFileReason.Upgrade, RecycleBinOperation.Upgrade)]
        [TestCase(DeleteMediaFileReason.Manual, RecycleBinOperation.Delete)]
        [TestCase(DeleteMediaFileReason.ManualOverride, RecycleBinOperation.Delete)]
        [TestCase(DeleteMediaFileReason.MissingFromDisk, RecycleBinOperation.Delete)]
        public void should_use_the_same_recycle_bin_operation_as_the_episode_file(DeleteMediaFileReason reason,
                                                                                    RecycleBinOperation operation)
        {
            var path = Path.Combine(_series.Path, _extraFile.RelativePath);

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, reason));

            Mocker.GetMock<IRecycleBinProvider>()
                  .Verify(r => r.DeleteFile(path, operation), Times.Once());
        }
    }
}
