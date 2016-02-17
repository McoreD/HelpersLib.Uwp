using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HelpersLib
{
    public static class Helpers
    {
        public const string Numbers = "0123456789"; // 48 ... 57
        public const string AlphabetCapital = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 65 ... 90
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyz"; // 97 ... 122
        public const string Alphanumeric = Numbers + AlphabetCapital + Alphabet;
        public const string AlphanumericInverse = Numbers + Alphabet + AlphabetCapital;
        public const string Hexadecimal = Numbers + "ABCDEF";
        public const string URLCharacters = Alphanumeric + "-._~"; // 45 46 95 126
        public const string URLPathCharacters = URLCharacters + "/"; // 47
        public const string ValidURLCharacters = URLPathCharacters + ":?#[]@!$&'()*+,;= ";

        public static void CreateDirectoryIfNotExist(string path, bool isFilePath = true)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (isFilePath)
                {
                    path = Path.GetDirectoryName(path);
                }

                if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception e)
                    {
                        DebugHelper.WriteException(e);
                        //MessageBox.Show(Resources.Helpers_CreateDirectoryIfNotExist_Create_failed_ + "\r\n\r\n" + e, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public static string GetValidFileName(string fileName, string separator = "")
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            if (string.IsNullOrEmpty(separator))
            {
                return new string(fileName.Where(c => !invalidFileNameChars.Contains(c)).ToArray());
            }
            else
            {
                invalidFileNameChars.ForEach(x => fileName = fileName.Replace(x.ToString(), separator));
                return fileName.Trim().Replace(separator + separator, separator);
            }
        }
    }
}