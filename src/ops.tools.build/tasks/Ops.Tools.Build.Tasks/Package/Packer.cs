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
    /// Defines a <see cref="ITask"/> that reads an 'isospec' file and creates the associated ISO file.
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
                Log.LogError("Output path for the ISO file is not defined. Unable to create an ISO file.");
                return false;
            }

            if ((ToolPath == null) || string.IsNullOrWhiteSpace(ToolPath.ItemSpec))
            {
                Log.LogError("The file path to the 'packer' executable is not defined. Unable to create an image.");
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

            InvokeCommandLineTool(
                ToolPath,
                arguments,
                WorkingDirectory,
                DefaultDataHandler,
                standardErrorHandler);

            return !Log.HasLoggedErrors;
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
            if (environmentVariables != null)
            {
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
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
