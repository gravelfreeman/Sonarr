using System;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public static class RecycleBinPathBuilder
    {
        public const string RecycleBinFolder = ".bin";

        public static string GetRecycleBinPath(string path)
        {
            var topLevelFolder = GetTopLevelFolder(path);

            if (topLevelFolder.IsNullOrWhiteSpace())
            {
                return null;
            }

            return Path.Combine(topLevelFolder, RecycleBinFolder);
        }

        public static string GetPathRelativeToTopLevelFolder(string path)
        {
            var topLevelFolder = GetTopLevelFolder(path);

            if (topLevelFolder.IsNullOrWhiteSpace())
            {
                return null;
            }

            return Path.GetRelativePath(topLevelFolder, Path.GetFullPath(path));
        }

        public static string GetRecycleBinDestination(string path)
        {
            return BuildRecycleBinDestination(path, GetTopLevelFolder(path));
        }

        public static string GetRecycleBinDestination(string path, string rootFolderPath)
        {
            return BuildRecycleBinDestination(path, GetTopLevelFolder(rootFolderPath));
        }

        private static string BuildRecycleBinDestination(string path, string topLevelFolder)
        {
            if (topLevelFolder.IsNullOrWhiteSpace())
            {
                return null;
            }

            var recyclingBin = Path.Combine(topLevelFolder, RecycleBinFolder);
            var relativePath = Path.GetRelativePath(topLevelFolder, Path.GetFullPath(path));

            if (relativePath.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (relativePath == ".")
            {
                return recyclingBin;
            }

            return Path.Combine(recyclingBin, relativePath);
        }

        public static string GetTopLevelFolder(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return null;
            }

            var fullPath = Path.GetFullPath(path);
            var pathRoot = Path.GetPathRoot(fullPath);

            if (pathRoot.IsNullOrWhiteSpace())
            {
                return null;
            }

            var relativePath = Path.GetRelativePath(pathRoot, fullPath);

            if (relativePath == ".")
            {
                return pathRoot;
            }

            var topLevelFolderName = relativePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                                                 .FirstOrDefault();

            if (topLevelFolderName.IsNullOrWhiteSpace() || topLevelFolderName == "." || topLevelFolderName == "..")
            {
                return null;
            }

            return Path.Combine(pathRoot, topLevelFolderName);
        }
    }
}
