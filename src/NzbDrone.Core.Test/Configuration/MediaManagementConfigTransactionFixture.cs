using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Configuration
{
    [TestFixture]
    public class MediaManagementConfigTransactionFixture : DbTest
    {
        private ConfigService _configService;
        private IRootFolderRepository _rootFolderRepository;

        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<IConfigRepository>(new ConfigRepository(Mocker.Resolve<IMainDatabase>(), Mocker.Resolve<IEventAggregator>()));
            Mocker.SetConstant<IRootFolderRepository>(new RootFolderRepository(Mocker.Resolve<IMainDatabase>(), Mocker.Resolve<IEventAggregator>()));

            _configService = Mocker.Resolve<ConfigService>();
            _rootFolderRepository = Mocker.Resolve<IRootFolderRepository>();
        }

        [Test]
        public void should_save_global_settings_and_multiple_root_folder_updates()
        {
            var first = Db.Insert(new RootFolder { Path = "/media/one", RecycleBinEnabled = true });
            var second = Db.Insert(new RootFolder { Path = "/media/two", RecycleBinEnabled = true });

            first.RecycleBinEnabled = false;
            second.RecycleBinEnabled = false;

            _configService.SaveConfigDictionary(new Dictionary<string, object> { { "RecycleBinEnabled", true } },
                (connection, transaction) => _rootFolderRepository.UpdateRecycleBinEnabled(new List<RootFolder> { first, second }, connection, transaction));

            Db.All<Config>().Single(x => x.Key == "recyclebinenabled").Value.Should().Be("True");
            Db.All<RootFolder>().Should().OnlyContain(x => !x.RecycleBinEnabled);
        }

        [Test]
        public void should_rollback_global_settings_and_root_folder_updates_when_an_update_fails()
        {
            var rootFolder = Db.Insert(new RootFolder { Path = "/media/one", RecycleBinEnabled = true });
            rootFolder.RecycleBinEnabled = false;

            Assert.Throws<InvalidOperationException>(() => _configService.SaveConfigDictionary(new Dictionary<string, object> { { "RecycleBinEnabled", true } },
                (connection, transaction) =>
                {
                    _rootFolderRepository.UpdateRecycleBinEnabled(new List<RootFolder> { rootFolder }, connection, transaction);
                    throw new InvalidOperationException();
                }));

            Db.All<Config>().Should().NotContain(x => x.Key == "recyclebinenabled");
            Db.All<RootFolder>().Single().RecycleBinEnabled.Should().BeTrue();
        }

        [Test]
        public void should_not_save_global_settings_when_a_root_folder_does_not_exist()
        {
            var missingRootFolder = new RootFolder { Id = 999, RecycleBinEnabled = false };

            Assert.Throws<ModelNotFoundException>(() => _configService.SaveConfigDictionary(new Dictionary<string, object> { { "RecycleBinEnabled", true } },
                (connection, transaction) => _rootFolderRepository.UpdateRecycleBinEnabled(new List<RootFolder> { missingRootFolder }, connection, transaction)));

            Db.All<Config>().Should().NotContain(x => x.Key == "recyclebinenabled");
        }
    }
}
