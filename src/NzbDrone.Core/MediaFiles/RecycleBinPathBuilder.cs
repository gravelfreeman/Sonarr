using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public static class RecycleBinPathBuilder
    {
        public const string RecycleBinFolder = ".bin";

        public static string GetRecycleBinPath(string mountPath)
        {
            if (mountPath.IsNullOrWhiteSpace())
            {
                return null;
            }

            return Path.Combine(Path.GetFullPath(mountPath), RecycleBinFolder);
        }

        public static string GetRecycleBinDestination(string path, string mountPath)
        {
            if (path.IsNullOrWhiteSpace() || mountPath.IsNullOrWhiteSpace())
            {
                return null;
            }

            var fullMountPath = Path.GetFullPath(mountPath);
            var relativePath = Path.GetRelativePath(fullMountPath, Path.GetFullPath(path));

            if (relativePath.IsNullOrWhiteSpace())
            {
                return null;
            }

            var recyclingBin = GetRecycleBinPath(fullMountPath);

            if (relativePath == ".")
            {
                return recyclingBin;
            }

            return Path.Combine(recyclingBin, relativePath);
        }
    }
}
