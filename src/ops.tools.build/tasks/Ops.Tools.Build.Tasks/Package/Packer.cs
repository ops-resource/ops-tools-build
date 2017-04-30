//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that invokes the <a href="https://packer.io">Packer</a> command line executable
    /// to generate an VM or container image.
    /// </summary>
    public sealed class Packer : CommandLineToolTask
    {
        private const string EnvironmentVariablePackerLogEnabled = "PACKER_LOG";

        private const string EnvironmentVariablePackerLogFile = "PACKER_LOG_PATH";

        /// <summary>
        /// Initializes a new instance of the <see cref="Packer"/> class.
        /// </summary>
        public Packer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Packer"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public Packer(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <summary>
        /// Gets or sets the path to the Packer configuration file.
        /// </summary>
        [Required]
        public ITaskItem ConfigurationFile
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if ((ConfigurationFile == null) || string.IsNullOrWhiteSpace(ConfigurationFile.ItemSpec))
            {
                Log.LogError(
                    string.Empty,
                    ErrorCodeById(ErrorIdFileNotFound),
                    ErrorIdFileNotFound,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    "Output path for the configuration file is not defined. Unable to invoke Packer.");
                return false;
            }

            if ((ToolPath == null) || string.IsNullOrWhiteSpace(ToolPath.ItemSpec))
            {
                Log.LogError(
                    string.Empty,
                    ErrorCodeById(ErrorIdApplicationPathNotFound),
                    ErrorIdApplicationPathNotFound,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    "The file path to the 'packer' executable is not defined. Unable to create an image.");
                return false;
            }

            var workingDirectory = GetAbsolutePath(WorkingDirectory);
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            var arguments = new List<string>();
            {
                arguments.Add("build");

                var variableFile = GetAbsolutePath(VariableFile);
                if (File.Exists(variableFile))
                {
                    arguments.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "-var-file=\"{0}\"",
                            variableFile));
                }

                if (KeepImageOnError)
                {
                    arguments.Add("-on-error=abort");
                }

                arguments.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "\"{0}\"",
                        GetAbsolutePath(ConfigurationFile)));
            }

            DataReceivedEventHandler standardErrorHandler = (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Log.LogWarning(e.Data);
                }
            };

            var toolPath = GetFullToolPath(ToolPath);
            var exitCode = InvokeCommandLineTool(
                toolPath,
                arguments,
                workingDirectory,
                DefaultDataHandler,
                standardErrorHandler);
            if (exitCode != 0)
            {
                Log.LogError(
                    string.Empty,
                    ErrorCodeById(ErrorIdApplicationNonzeroExitCode),
                    ErrorIdApplicationNonzeroExitCode,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    "{0} exited with a non-zero exit code. Exit code was: {1}",
                    Path.GetFileName(toolPath),
                    exitCode);
            }

            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Packer should keep the image if there is an error or not.
        /// </summary>
        public bool KeepImageOnError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full path to the location where the Packer log file should be dropped.
        /// </summary>
        public ITaskItem LogFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the 'packer' command line tool.
        /// </summary>
        [Required]
        public ITaskItem ToolPath
        {
            get;
            set;
        }

        /// <inheritdoc/>
        protected override void UpdateEnvironmentVariables(StringDictionary environmentVariables)
        {
            const string EnvironmentVariableTempDirectory = "TMP";
            if (environmentVariables != null)
            {
                var tempDir = GetAbsolutePath(TempDirectory);
                if (!environmentVariables.ContainsKey(EnvironmentVariableTempDirectory))
                {
                    environmentVariables.Add(EnvironmentVariableTempDirectory, tempDir);
                }
                else
                {
                    environmentVariables[EnvironmentVariableTempDirectory] = tempDir;
                }

                environmentVariables.Add("PACKER_CACHE_DIR", "packer-cache");

                var logFile = GetAbsolutePath(LogFile);
                if (!string.IsNullOrWhiteSpace(logFile))
                {
                    if (!environmentVariables.ContainsKey(EnvironmentVariablePackerLogEnabled))
                    {
                        environmentVariables.Add(EnvironmentVariablePackerLogEnabled, "true");
                    }

                    environmentVariables.Add(
                        EnvironmentVariablePackerLogFile,
                        logFile);
                }
            }
        }

        /// <summary>
        /// Gets or sets the path to the file containing the definition of the Packer variables.
        /// </summary>
        public ITaskItem VariableFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the working directory.
        /// </summary>
        [Required]
        public ITaskItem TempDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the working directory.
        /// </summary>
        [Required]
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
