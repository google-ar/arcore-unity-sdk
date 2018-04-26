//-----------------------------------------------------------------------
// <copyright file="ShellHelper.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCoreInternal
{
    using System.Text;

    /// <summary>
    /// Misc helper methods for running shell commands.
    /// </summary>
    public static class ShellHelper
    {
        /// <summary>
        /// Run a shell command.
        /// </summary>
        /// <param name="fileName">File name for the executable.</param>
        /// <param name="arguments">Command line arguments, space delimited.</param>
        /// <param name="output">Filled out with the result as printed to stdout.</param>
        /// <param name="error">Filled out with the result as printed to stderr.</param>
        public static void RunCommand(string fileName, string arguments, out string output, out string error)
        {
            using (var process = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo(fileName, arguments);
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                process.OutputDataReceived += (sender, ef) => outputBuilder.Append(ef.Data);
                process.ErrorDataReceived += (sender, ef) => errorBuilder.Append(ef.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();

                // Trims the output strings to make comparison easier.
                output = outputBuilder.ToString().Trim();
                error = errorBuilder.ToString().Trim();
            }
        }
    }
}
