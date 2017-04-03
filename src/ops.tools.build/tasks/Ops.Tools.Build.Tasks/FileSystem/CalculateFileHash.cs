//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.FileSystem
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that calculates the hash of a given file.
    /// </summary>
    public sealed class CalculateFileHash : BaseTask
    {
        /// <summary>
        /// Gets or sets the algorithm that should be used to calculate the file hash.
        /// </summary>
        public string Algorithm
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if ((Path == null) || string.IsNullOrWhiteSpace(Path.ItemSpec))
            {
                Log.LogError("The file path is not defined. Unable to calculate the hash.");
                return false;
            }

            var filePath = GetAbsolutePath(Path);
            if (!File.Exists(filePath))
            {
                Log.LogError(
                    "The file was expected to be found at '{0}' but that path does not exist. Unable to calculate the hash of a non-existent file.",
                    filePath);
                return false;
            }

            // Create a fileStream for the file.
            var fileStream = File.OpenRead(filePath);

            // Be sure it's positioned to the beginning of the stream.
            fileStream.Position = 0;

            byte[] hashValue = null;
            var algorithm = Algorithm.ToUpperInvariant();
            switch (algorithm)
            {
                case "MD5":
                    // Compute the MD5 hash of the fileStream.
                    hashValue = MD5.Create().ComputeHash(fileStream);
                    break;
                case "SHA1":
                    // Compute the SHA1 hash of the fileStream.
                    hashValue = SHA1.Create().ComputeHash(fileStream);
                    break;
                case "SHA256":
                    // Compute the SHA256 hash of the fileStream.
                    hashValue = SHA256.Create().ComputeHash(fileStream);
                    break;
                case "SHA512":
                    // Compute the SHA512 hash of the fileStream.
                    hashValue = SHA512.Create().ComputeHash(fileStream);
                    break;
                default:
                    Log.LogError(
                        "The specified hash algorithm of '{0}' is not valid. Please select on of: MD5, SHA1, SHA256, SHA384 or SHA512.",
                        algorithm);
                    return false;
            }

            if (hashValue == null)
            {
                Log.LogError(
                    "Failed to calculate a hash for the file at '{0}' with the specified hash algorithm of '{0}'.",
                    filePath,
                    algorithm);
                return false;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < hashValue.Length; i++)
            {
                builder.AppendFormat("{0:X2}", hashValue[i]);
            }

            Hash = builder.ToString();

            // Log.HasLoggedErrors is true if the task logged any errors -- even if they were logged
            // from a task's constructor or property setter. As long as this task is written to always log an error
            // when it fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Gets or sets the hash of the given file.
        /// </summary>
        [Output]
        public string Hash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the file from which the hash should be calculated.
        /// </summary>
        [Required]
        public ITaskItem Path
        {
            get;
            set;
        }
    }
}
