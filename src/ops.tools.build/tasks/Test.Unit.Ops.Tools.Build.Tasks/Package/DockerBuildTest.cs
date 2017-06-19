//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Build.Utilities;
using Moq;
using NBuildKit.MsBuild.Tasks.Core;
using NBuildKit.MsBuild.Tasks.Tests;
using Nuclei;
using NUnit.Framework;

namespace Ops.Tools.Build.Tasks.Package
{
    [TestFixture]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Unit tests do not need documentation.")]
    public sealed class DockerBuildTest : TaskTest
    {
        [Test]
        public void Execute()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(2, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual(directory, invokedArgs[1]);
        }

        [Test]
        public void ExecuteWithAlwaysRemoveIntermediateLayers()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.AlwaysRemoveIntermediateLayers = true;
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("--force-rm", invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithDockerFile()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var dockerFile = Path.Combine(directory, "dockerfile");

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.DockerFile = new TaskItem(dockerFile);

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "--file \"{0}\"",
                    dockerFile),
                invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithIsolationLevel()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var isolationLevel = "a";

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.Isolation = isolationLevel;

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "--isolation {0}",
                    isolationLevel),
                invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithNoCache()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.NoCache = true;

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("--no-cache", invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithoutBuildContext()
        {
            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");
        }

        [Test]
        public void ExecuteWithoutDockerExecutable()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");
        }

        [Test]
        public void ExecuteWithPull()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.Pull = true;

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("--pull", invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithSquash()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.Squash = true;

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("--squash", invokedArgs[1]);
            Assert.AreEqual(directory, invokedArgs[2]);
        }

        [Test]
        public void ExecuteWithTags()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
            Action<StringDictionary> environmentVariableBuilder = null;
            var invoker = new Mock<IApplicationInvoker>();
            {
                invoker.Setup(
                    i => i.Invoke(
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<Action<StringDictionary>>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<DataReceivedEventHandler>(),
                        It.IsAny<bool>()))
                    .Callback<string, IEnumerable<string>, string, Action<StringDictionary>, DataReceivedEventHandler, DataReceivedEventHandler, bool>(
                        (path, args, dir, e, o, err, f) =>
                        {
                            invokedPath = path;
                            invokedArgs.AddRange(args);
                            invokedWorkingDirectory = dir;
                            environmentVariableBuilder = e;
                        });
            }

            var task = new DockerBuild(invoker.Object);
            task.BuildContext = new TaskItem(directory);
            task.BuildEngine = BuildEngine.Object;
            task.DockerExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.Tags = new[]
            {
                "a",
                "b",
            };

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(4, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "--tag \"{0}\"",
                    task.Tags[0]),
                invokedArgs[1]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "--tag \"{0}\"",
                    task.Tags[1]),
                invokedArgs[2]);
            Assert.AreEqual(directory, invokedArgs[3]);
        }
    }
}
