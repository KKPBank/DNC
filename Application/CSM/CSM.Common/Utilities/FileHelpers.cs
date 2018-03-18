using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Common.Utilities
{
    public static class FileHelpers
    {
        /// <summary>
        /// Create folder if not exist and return full file path (created folder path + file name)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileNameWithExtension"></param>
        /// <returns></returns>
        public static string GenerateFilePath(string folderPath, string fileNameWithExtension)
        {
            // Check Exists Folder           
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = $@"{folderPath}\{fileNameWithExtension}";
            return fullFilePath;
        }
    }
}
