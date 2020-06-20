//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that invokes the <a href="https://terraform.io">Terraform</a> command line executable
    /// to generate a plan.
    /// </summary>
    public sealed class TerraformPlan : TerraformCommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerraformPlan"/> class.
        /// </summary>
        public TerraformPlan()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerraformPlan"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public TerraformPlan(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if ((Settings is null) || string.IsNullOrWhiteSpace(Settings.ItemSpec))
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
                arguments.Add("plan");
            }

            if (Settings != null)
            {
                var dict = Settings.CloneCustomMetadata();
                foreach (DictionaryEntry pair in dict)
                {
                    arguments.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "-var '{0}={1}'",
                            pair.Key,
                            pair.Value));
                }

                arguments.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "-out={0}",
                        Settings.ItemSpec));
            }

            var exitCode = InvokeTerraform(WorkingDirectory, arguments);
            if (exitCode != 0)
            {
                if (TerraformExecutablePath != null)
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
                        Path.GetFileName(TerraformExecutablePath.ItemSpec),
                        exitCode);
                }

                return false;
            }

            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Gets or sets the settings for the Plan command.
        /// </summary>
        [SuppressMessage(
           "Microsoft.Performance",
           "CA1819:PropertiesShouldNotReturnArrays",
           Justification = "MsBuild does not understand collections")]
        public ITaskItem Settings
        {
            get;
            set;
        }
    }
}
