//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that performs a Docker build.
    /// </summary>
    public sealed class DockerBuild : DockerCommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockerBuild"/> class.
        /// </summary>
        public DockerBuild()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerBuild"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public DockerBuild(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether intermediate layers should always be removed.
        /// </summary>
        public bool AlwaysRemoveIntermediateLayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the build context. The build context is either a directory or a URL
        /// </summary>
        [Required]
        public ITaskItem BuildContext
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the docker file
        /// </summary>
        public ITaskItem DockerFile
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if ((BuildContext == null) || string.IsNullOrWhiteSpace(BuildContext.ItemSpec))
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
                    "The build context was not provided. Cannot invoke docker build.");
                return false;
            }

            var arguments = new List<string>();
            {
                arguments.Add("build");

                if (AlwaysRemoveIntermediateLayers)
                {
                    arguments.Add("--force-rm");
                }

                if (NoCache)
                {
                    arguments.Add("--no-cache");
                }

                if (Pull)
                {
                    arguments.Add("--pull");
                }

                if (Squash)
                {
                    arguments.Add("--squash");
                }

                if (!string.IsNullOrWhiteSpace(Isolation))
                {
                    arguments.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "--isolation {0}",
                            Isolation));
                }

                if (Tags != null)
                {
                    foreach (var tag in Tags)
                    {
                        if (!string.IsNullOrWhiteSpace(tag))
                        {
                            arguments.Add(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "--tag \"{0}\"",
                                    tag));
                        }
                    }
                }

                if ((DockerFile != null) && !string.IsNullOrWhiteSpace(DockerFile.ItemSpec))
                {
                    arguments.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "--file \"{0}\"",
                            GetAbsolutePath(DockerFile).TrimEnd('\\')));
                }

                arguments.Add(GetAbsolutePath(BuildContext).TrimEnd('\\'));
            }

            var exitCode = InvokeDocker(BuildContext, arguments);
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
        /// Gets or sets the container isolation level.
        /// </summary>
        public string Isolation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container cache should be used while building.
        /// </summary>
        public bool NoCache
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether newer versions of the base image should always be pulled.
        /// </summary>
        public bool Pull
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not intermediate layers should be squashed into a single new layer.
        /// </summary>
        public bool Squash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tags for the container.
        /// </summary>
        public string[] Tags
        {
            get;
            set;
        }
    }
}
