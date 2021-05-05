using System;
using System.IO;

namespace ScanHelperMergePDF
{
    public static class Globals
    {
        public static bool DirectoriesReady(string inputDir, string outputDir)
        {
            bool inputReady = false;
            bool outputReady = false;

            try
            {
                DirectoryInfo inputDirectoryInfo = new DirectoryInfo(inputDir);

                DirectoryInfo[] inputDirectories = inputDirectoryInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                if (inputDirectoryInfo.Exists && inputDirectories.Length > 0)
                {
                    inputReady = true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                DirectoryInfo outputDirectoryInfo = new DirectoryInfo(outputDir);

                DirectoryInfo[] outputDirectories = outputDirectoryInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                if (outputDirectoryInfo.Exists && outputDirectories.Length == 0)
                {
                    outputReady = true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return inputReady && outputReady;
        }
    }
}
