//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that performs a Docker push.
    /// </summary>
    public sealed class DockerPush : DockerCommandLineToolTask
    {
    }
}
