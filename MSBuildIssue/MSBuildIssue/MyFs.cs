// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Shared.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Build.FileSystem
{
    /// <summary>
    /// Abstracts away some file system operations.
    ///
    /// Implementations:
    /// - must be thread safe
    /// - may cache some or all the calls.
    /// </summary>
    class MyFs : MSBuildFileSystemBase
    {
       
        /// <summary>
        /// Use this for var sr = new StreamReader(path)
        /// </summary>
        public override TextReader ReadFile(string path)
        {
            return new StreamReader(path);
        }

        public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public override string ReadFileAllText(string path)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadFileAllBytes(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateDirectories(path, searchPattern, searchOption);
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
        }

        public override FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        public override bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public override bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public override bool FileOrDirectoryExists(string path)
        {
            return FileExists(path) || DirectoryExists(path);
        }
    }
}
