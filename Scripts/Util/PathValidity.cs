using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor
{
    internal static class PathValidity
    {
        public static bool IsPathValid(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            var invalidChars = Path.GetInvalidFileNameChars();

            return !path.Any(x => invalidChars.Contains(x));
        }

        public static bool IsDirValid(string dirPath)
        {
            if (!IsPathValid(dirPath))
                return false;

            var additionalInvalidChars = new char[] { '.' };
            var invalidChars = Path.GetInvalidFileNameChars().Concat(additionalInvalidChars);

            return !Path.GetDirectoryName(dirPath).Any(x => invalidChars.Contains(x));
        }

        public static bool IsFileValid(string filePath)
        {
            if (!IsPathValid(filePath))
                return false;
            var invalidChars = Path.GetInvalidFileNameChars();

            return !Path.GetFileName(filePath).Any(x => invalidChars.Contains(x));
        }
    }
}
