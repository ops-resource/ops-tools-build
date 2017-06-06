//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines the base class for <see cref="ITask"/> classes that work with the Docker command line tool.
    /// </summary>
    public abstract class DockerCommandLineToolTask : CommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockerCommandLineToolTask"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        protected DockerCommandLineToolTask(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <summary>
        /// Invokes the Docker command line tool with the given arguments in the provided workspace directory.
        /// </summary>
        /// <param name="arguments">The collection of arguments.</param>
        /// <param name="standardOutputHandler">
        ///     The event handler that handles the standard output stream of the command line application. If no value is provided
        ///     then all messages are logged.
        /// </param>
        /// <returns>The output of the GIT process</returns>
        protected int InvokeDocker(IEnumerable<string> arguments, DataReceivedEventHandler standardOutputHandler = null)
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
                DockerExecutablePath,
                arguments,
                WorkingDirectory,
                standardOutputHandler: standardOutputHandler,
                standardErrorHandler: DefaultErrorHandler);
            return exitCode;
        }

        /// <summary>
        /// Gets or sets the path to the Docker command line executable.
        /// </summary>
        [Required]
        public ITaskItem DockerExecutablePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full path to the working directory
        /// </summary>
        [Required]
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
