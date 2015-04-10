// Copyright � 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.Infrastructure.Services
{
    using System.IO;

    /// <summary>
    ///   File System interaction
    /// </summary>
    public interface IFileSystemService
    {
        //void Save(string fileName, string fileText, bool overwrite);

        /// <summary>
        ///   Gets the name of the file from the file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        string GetFileName(string filePath);

        /// <summary>
        ///   Gets the file extension.
        /// </summary>
        /// <param name="path">The path to the file, could be just a name of a file.</param>
        /// <returns>Extension of the file</returns>
        string GetFileExtension(string path);

        /// <summary>
        ///   Does a file exist?
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        bool FileExists(string filePath);

        /// <summary>
        ///   Combines all items together as one path with \ between them
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns></returns>
        string PathCombine(params string[] paths);

        /// <summary>
        ///   Gets the full path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string GetFullPath(string path);

        /// <summary>
        ///   Gets the full directory path of a file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        string GetDirectoryName(string filePath);

        /// <summary>
        ///   Creates the directory if it doesn't exist.
        /// </summary>
        /// <param name="directory">The directory.</param>
        void CreateDirectoryIfNotExists(string directory);

        /// <summary>
        ///   Saves the specified file name.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="overwrite">
        ///   if set to <c>true</c> [overwrite].
        /// </param>
        void Save(string filePath, Stream fileStream);

        /// <summary>
        ///   Gets the stream for a file
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        Stream GetStream(string filePath);

        /// <summary>
        ///   Gets the file text.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        string GetFileText(string filePath);
    }
}