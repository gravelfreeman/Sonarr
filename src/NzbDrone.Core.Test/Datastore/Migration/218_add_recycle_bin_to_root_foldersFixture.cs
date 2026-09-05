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
        public void should_not_enable_recycle_bin_when_legacy_path_is_missing()
        {
            var db = WithMigrationTestDb();

            db.QueryScalar<string>("SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'recyclebinenabled'")
              .Should().BeNull();
        }

        [Test]
        public void should_not_enable_recycle_bin_when_legacy_path_is_empty()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "recyclebin",
                    Value = ""
                });
            });

            db.QueryScalar<string>("SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'recyclebinenabled'")
              .Should().BeNull();
        }
    }
}
