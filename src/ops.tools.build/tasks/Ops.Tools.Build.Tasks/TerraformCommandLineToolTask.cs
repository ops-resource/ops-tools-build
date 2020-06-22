//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks
{
    /// <summary>
    /// Defines the base class for <see cref="ITask"/> classes that work with the Terraform command line tool.
    /// </summary>
    public abstract class TerraformCommandLineToolTask : CommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerraformCommandLineToolTask"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        protected TerraformCommandLineToolTask(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <summary>
        /// Gets or sets the path to the terraform data directory.
        /// </summary>
        [Required]
        public ITaskItem TerraformDataDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full path to the location to the log file.
        /// </summary>
        public ITaskItem LogFile
        {
            get;
            set;
        }

        /// <summary>
        /// Invokes the Terraform command line tool with the given arguments in the provided workspace directory.
        /// </summary>
        /// <param name="workingDirectory">The directory from which the docker command should be invoked.</param>
        /// <param name="arguments">The collection of arguments.</param>
        /// <param name="standardOutputHandler">
        ///     The event handler that handles the standard output stream of the command line application. If no value is provided
        ///     then all messages are logged.
        /// </param>
        /// <returns>The output of the GIT process.</returns>
        protected int InvokeTerraform(ITaskItem workingDirectory, IEnumerable<string> arguments, DataReceivedEventHandler standardOutputHandler = null)
        {
            if (standardOutputHandler == null)
            {
                standardOutputHandler = (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Log.LogMessage(MessageImportance.Normal, e.Data);
                    }
                };
            }

            var exitCode = InvokeCommandLineTool(
                TerraformExecutablePath,
                arguments,
                workingDirectory,
                standardOutputHandler: standardOutputHandler,
                standardErrorHandler: DefaultErrorHandler);
            return exitCode;
        }

        /// <summary>
        /// Gets or sets the path to the Terraform command line executable.
        /// </summary>
        [Required]
        public ITaskItem TerraformExecutablePath
        {
            get;
            set;
        }

        /// <inheritdoc/>
        protected override void UpdateEnvironmentVariables(StringDictionary environmentVariables)
        {
            if (environmentVariables != null)
            {
                const string TerraformInAutomation = "TF_IN_AUTOMATION";
                if (!environmentVariables.ContainsKey(TerraformInAutomation))
                {
                    environmentVariables.Add(TerraformInAutomation, "true");
                }

                const string TerraformInput = "TF_INPUT";
                if (!environmentVariables.ContainsKey(TerraformInput))
                {
                    environmentVariables.Add(TerraformInput, "false");
                }

                const string TerraformCliArgs = "TF_CLI_ARGS";
                if (!environmentVariables.ContainsKey(TerraformCliArgs))
                {
                    environmentVariables.Add(TerraformCliArgs, "-no-color");
                }

                const string TerraformLog = "TF_LOG";
                if (!environmentVariables.ContainsKey(TerraformLog))
                {
                    environmentVariables.Add(TerraformLog, "TRACE");
                }

                const string TerraformLogPath = "TF_LOG_PATH";
                if (!environmentVariables.ContainsKey(TerraformLogPath) && (LogFile != null))
                {
                    environmentVariables.Add(TerraformLogPath, LogFile.ItemSpec);
                }

                const string TerraformDataDir = "TF_DATA_DIR";
                if (!environmentVariables.ContainsKey(TerraformDataDir) && (TerraformDataDirectory != null))
                {
                    environmentVariables.Add(TerraformDataDir, TerraformDataDirectory.ItemSpec);
                }
            }

            base.UpdateEnvironmentVariables(environmentVariables);
        }

        /// <summary>
        /// Gets or sets the working directory for the Terraform commands.
        /// </summary>
        [Required]
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
