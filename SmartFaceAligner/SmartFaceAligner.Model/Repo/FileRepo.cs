using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;

namespace SmartFaceAligner.Processor.Repo
{
    public class FileRepo : IFileRepo
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        
        public async Task<bool> Write(string file, byte[] data)
        {
            var f = _getFile(file);
            File.WriteAllBytes(f.FullName, data);
            return true;
        }

        public async Task<bool> Write(string file, string text)
        {
            var f = _getFile(file);
            File.WriteAllText(f.FullName, text);
            return true;
        }


        public async Task<byte[]> ReadBytes(string file)
        {
            var f = _getFile(file, false);
            if (!f.Exists)
            {
                return null;
            }

            return File.ReadAllBytes(f.FullName);
        }


        public async Task<string> ReadText(string file)

        {
            var f = _getFile(file, false);

            if (!f.Exists)
            {
                return null;
            }

            return File.ReadAllText(f.FullName);
        }

        FileInfo _getFile(string file, bool createPath = true)
        {
            var f = new FileInfo(file);
            if (!f.Directory.Exists && createPath)
            {
                f.Directory.Create();
            }
            return f;
        }

        public async Task<bool> FileExists(string filePath)
        {
            var f = new FileInfo(filePath);
            return f.Exists;
        }

        public string GetPathSeparator()
        {
            return Path.PathSeparator.ToString();
        }

        public async Task<bool> DirectoryExists(string directory)
        {
            var d = new DirectoryInfo(directory);
            return d.Exists;
        }

        public async Task<int> CopyFolder(string source, string target)
        {
            var dSource = new DirectoryInfo(source);
            var dTarget = new DirectoryInfo(target);

            if (!dSource.Exists)
            {
                dSource.Create();
            }
            if (!dTarget.Exists)
            {
                dTarget.Create();
            }

            var sourceFiles = dSource.GetFiles();

            foreach (var f in sourceFiles)
            {
                f.CopyTo(Path.Combine(dTarget.FullName, f.Name));
            }
            return sourceFiles.Length;
        }

        public async Task<string> GetOffsetFile(params string[] filePath)
        {
            var fOffset = string.Join(GetPathSeparator(), filePath);

            return _getFile(fOffset).FullName;
        }

        public async Task<string> GetOffsetFolder(params string[] filePath)
        {
            var fOffset = string.Join(GetPathSeparator(), filePath);
            var d = new DirectoryInfo(fOffset);
            if (!d.Exists)
            {
                d.Create();
            }
            return d.FullName;
        }

        public async Task<string> GetOffsetFile(string filePath)
        {
            var f = _getFile(
                Path.Combine((await GetBaseFolder()).FullName, filePath));
            return f.FullName;
        }

       
        public async Task<DirectoryInfo> GetBaseFolder()
        {
            var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"SmartFaceAligner");
            var directory = new DirectoryInfo(d);
            if (!directory.Exists)
            {
                directory.Create();
            }

            return directory;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
