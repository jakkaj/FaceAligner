﻿using System.IO;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IFileRepo
    {
        Task<DirectoryInfo> GetBaseFolder();
        Task<bool> Write(string file, byte[] data);
        Task<bool> Write(string file, string text);
        Task<byte[]> ReadBytes(string file);
        Task<string> ReadText(string file);
        Task<string> GetOffsetFile(string filePath);
        Task<bool> FileExists(string filePath);
        string GetPathSeparator();
        Task<string> GetOffsetFile(params string[] filePath);
        Task<string> GetOffsetFolder(params string[] filePath);
        Task<int> CopyFolder(string source, string target);
        Task<bool> DirectoryExists(string directory);
    }
}