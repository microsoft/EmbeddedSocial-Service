// <copyright file="RunCommand.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Diagnostics;

    /// <summary>
    /// Helper class to execute a command inside the command prompt and capture its output
    /// </summary>
    public class RunCommand
    {
        /// <summary>
        /// Execute the given command inside a command window.  Do not show the command window, and capture the output.
        /// </summary>
        /// <param name="command">command to execute</param>
        /// <returns>output of the command</returns>
        public static string Execute(string command)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            return cmd.StandardOutput.ReadToEnd();
        }
    }
}
