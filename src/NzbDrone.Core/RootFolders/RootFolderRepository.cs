using System.Collections.Generic;
using System.Data;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
        void UpdateRecycleBinEnabled(IList<RootFolder> rootFolders, IDbConnection connection, IDbTransaction transaction);
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;

        public void UpdateRecycleBinEnabled(IList<RootFolder> rootFolders, IDbConnection connection, IDbTransaction transaction)
        {
            var rowsUpdated = connection.Execute("UPDATE \"RootFolders\" SET \"RecycleBinEnabled\" = @RecycleBinEnabled WHERE \"Id\" = @Id", rootFolders, transaction);

            if (rowsUpdated != rootFolders.Count)
            {
                throw new ModelNotFoundException(typeof(RootFolder), 0);
            }
        }
    }
}
