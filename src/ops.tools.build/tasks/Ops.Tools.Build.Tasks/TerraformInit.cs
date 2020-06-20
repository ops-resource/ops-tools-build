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

namespace Ops.Tools.Build.Tasks
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that invokes the <a href="https://terraform.io">Terraform</a> command line executable
    /// to initialize a workspace.
    /// </summary>
    public sealed class TerraformInit : TerraformCommandLineToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerraformInit"/> class.
        /// </summary>
        public TerraformInit()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerraformInit"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public TerraformInit(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            var arguments = new List<string>();
            {
                arguments.Add("init");
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
    }
}
