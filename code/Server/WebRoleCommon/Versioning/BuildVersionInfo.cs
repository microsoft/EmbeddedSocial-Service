// <copyright file="BuildVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Information about the version of the build
    /// </summary>
    public class BuildVersionInfo : IBuildVersionInfo
    {
        /// <summary>
        /// The Git commit hash that represents the current git head
        /// </summary>
        private readonly string commitHash = null;

        /// <summary>
        /// the hostname that this code was built on
        /// </summary>
        private readonly string hostname = null;

        /// <summary>
        /// which files were not committed at build time
        /// </summary>
        private readonly List<string> dirtyFiles;

        /// <summary>
        /// The date and time when the current build was created
        /// </summary>
        private readonly string buildDateAndTime = null;

        /// <summary>
        /// The name of the branch the current build was created from
        /// </summary>
        private readonly string branchName = null;

        /// <summary>
        /// The name of the tag for the commit hash
        /// </summary>
        private readonly string tag = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildVersionInfo"/> class.
        /// </summary>
        public BuildVersionInfo()
        {
            this.dirtyFiles = new List<string>();

            // get the commitHash if it exists
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoCommitHash.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.commitHash = reader.ReadToEnd().Trim();
            }

            // get the hostname if it exists
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoHostname.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.hostname = reader.ReadToEnd().Trim();
            }

            // get the build date and time if it exists
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoDate.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.buildDateAndTime = reader.ReadToEnd().Trim();
            }

            // get the list of dirty files
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoDirtyFiles.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string dirtyString = reader.ReadToEnd().Trim();
                var elements = dirtyString.Split('\u0000');
                foreach (var dirtyFile in elements)
                {
                    this.dirtyFiles.Add(dirtyFile.Trim());
                }

                if (this.dirtyFiles.Count > 0 && string.IsNullOrWhiteSpace(this.dirtyFiles[this.dirtyFiles.Count - 1]))
                {
                    this.dirtyFiles.RemoveAt(this.dirtyFiles.Count - 1);
                }
            }

            // get the branch name if it exists
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoBranchName.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.branchName = reader.ReadToEnd().Trim();
            }

            // get the tag name if it exists
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialPlus.Server.WebRoleCommon.Resources.BuildInfoTag.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.tag = reader.ReadToEnd().Trim();
            }

            // If tag name does not exist, replace it with "N/A"
            if (string.IsNullOrEmpty(this.tag))
            {
                this.tag = "N/A";
            }
        }

        /// <summary>
        /// Gets commit hash
        /// </summary>
        public string CommitHash
        {
            get
            {
                return this.commitHash;
            }
        }

        /// <summary>
        /// Gets hostname
        /// </summary>
        public string Hostname
        {
            get
            {
                return this.hostname;
            }
        }

        /// <summary>
        /// Gets dirty files
        /// </summary>
        public List<string> DirtyFiles
        {
            get
            {
                return this.dirtyFiles;
            }
        }

        /// <summary>
        /// Gets build data and time
        /// </summary>
        public string BuildDateAndTime
        {
            get
            {
                return this.buildDateAndTime;
            }
        }

        /// <summary>
        /// Gets name of branch
        /// </summary>
        public string BranchName
        {
            get
            {
                return this.branchName;
            }
        }

        /// <summary>
        /// Gets tag
        /// </summary>
        public string Tag
        {
            get
            {
                return this.tag;
            }
        }
    }
}
