using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_recycle_bin_to_root_foldersFixture : MigrationTest<add_recycle_bin_to_root_folders>
    {
        [Test]
        public void should_enable_recycle_bin_when_legacy_path_is_configured()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "recyclebin",
                    Value = "/media/.recyclebin"
                });
            });

            db.QueryScalar<string>("SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'recyclebinenabled'")
              .Should().Be("True");
        }

        [Test]
        public void should_enable_recycle_bin_for_existing_root_folders()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("RootFolders").Row(new
                {
                    Path = "/media/tv"
                });
            });

            db.QueryScalar<int>("SELECT \"RecycleBinEnabled\" FROM \"RootFolders\" WHERE \"Path\" = '/media/tv'")
              .Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("")]
        public void should_not_enable_recycle_bin_when_legacy_path_is_missing_or_empty(string legacyPath)
        {
            var db = WithMigrationTestDb(c =>
            {
                if (legacyPath != null)
                {
                    c.Insert.IntoTable("Config").Row(new
                    {
                        Key = "recyclebin",
                        Value = legacyPath
                    });
                }
            });

            db.QueryScalar<string>("SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'recyclebinenabled'")
              .Should().BeNull();
        }
    }
}
