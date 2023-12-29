using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
//using System.Security.Permissions;
using System.Text;
using Windows.Storage;

namespace GameManager
{
    public class ExternalStorage : IStorageProvider
    {
        private readonly string BASE_DIR;

        public ExternalStorage()
        {
#if WINDOWS
            BASE_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "TWP");
#else
            //BASE_DIR = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "TWP");
            //BASE_DIR = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
            // BASE_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            //"My Games", "TWP");
            BASE_DIR = ApplicationData.Current.LocalFolder.Path;
#endif
        }

        public bool FileExists(string path)
        {
            return File.Exists(Path.Combine(BASE_DIR, path));
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(Path.Combine(BASE_DIR, path));
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(BASE_DIR, path));
        }

        public FileStream OpenFile(string path, FileMode mode)
        {
            return File.Open(Path.Combine(BASE_DIR, path), mode);
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            File.Move(Path.Combine(BASE_DIR, sourcePath), Path.Combine(BASE_DIR, destinationPath));
        }

        public void DeleteFile(string path)
        {
            File.Delete(Path.Combine(BASE_DIR, path));
        }

        public string[] GetFileNames(string searchPattern)
        {
            string[] files = Directory.GetFiles(BASE_DIR, searchPattern);

            Array.Sort<string>(files, (a, b) =>
            {
                return File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a));
            });

            return files.Select(x => Path.GetFileName(x)).ToArray();
        }
    }
}
