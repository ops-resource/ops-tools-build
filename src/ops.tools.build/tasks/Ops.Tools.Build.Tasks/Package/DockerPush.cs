//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that performs a Docker push.
    /// </summary>
    public sealed class DockerPush : DockerCommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockerPush"/> class.
        /// </summary>
        public DockerPush()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerPush"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public DockerPush(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Log.LogError(
                    string.Empty,
                    ErrorCodeById(NBuildKit.MsBuild.Tasks.Core.ErrorInformation.ErrorIdApplicationMissingArgument),
                    NBuildKit.MsBuild.Tasks.Core.ErrorInformation.ErrorIdApplicationMissingArgument,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    "The name of the container to push was not provided. Cannot push container to the repository.");
                return false;
            }

            var arguments = new List<string>();
            {
                arguments.Add("push");
                arguments.Add(Name);
            }

            var exitCode = InvokeDocker(WorkingDirectory, arguments);
            if (exitCode != 0)
            {
                if (DockerExecutablePath != null)
                {
                    Log.LogError(
                        string.Empty,
                        ErrorCodeById(NBuildKit.MsBuild.Tasks.Core.ErrorInformation.ErrorIdApplicationNonzeroExitCode),
                        NBuildKit.MsBuild.Tasks.Core.ErrorInformation.ErrorIdApplicationNonzeroExitCode,
                        string.Empty,
                        0,
                        0,
                        0,
                        0,
                        "{0} exited with a non-zero exit code. Exit code was: {1}",
                        Path.GetFileName(DockerExecutablePath.ItemSpec),
                        exitCode);
                }

                return false;
            }

            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Gets or sets the image name or tag that should be pushed.
        /// </summary>
        [Required]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the working directory for the docker push command.
        /// </summary>
        [Required]
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
