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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using Moq;
using NBuildKit.MsBuild.Tasks.Core;
using NBuildKit.MsBuild.Tasks.Tests;
using Nuclei;
using NUnit.Framework;

namespace Ops.Tools.Build.Tasks.Package
{
    [TestFixture]
    public sealed class PackerTest : TaskTest
    {
        [Test]
        public void ExecuteWithEmptyConfigurationFile()
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

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.ConfigurationFile = new TaskItem();
            task.TempDirectory = new TaskItem(directory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");
            invoker.Verify(
                i => i.Invoke(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<StringDictionary>>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<bool>()),
                Times.Never());
        }

        [Test]
        public void ExecuteWithFailure()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var configurationFile = Path.Combine(directory, "a.json");
            var logFile = Path.Combine(directory, "a.log");
            var variableFile = Path.Combine(directory, "v.json");
            if (!File.Exists(variableFile))
            {
                File.Create(variableFile);
            }

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
                        })
                    .Returns(-1);
            }

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.ConfigurationFile = new TaskItem(configurationFile);
            task.LogFile = new TaskItem(logFile);
            task.TempDirectory = new TaskItem(directory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.VariableFile = new TaskItem(variableFile);
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(4, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("-color=false", invokedArgs[1]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-var-file=\"{0}\"",
                    variableFile),
                invokedArgs[2]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\"",
                    configurationFile),
                invokedArgs[3]);

            var environmentVariables = new StringDictionary();
            environmentVariableBuilder(environmentVariables);

            Assert.AreEqual(4, environmentVariables.Count);
            Assert.AreEqual(
                "packer-cache",
                environmentVariables["PACKER_CACHE_DIR"]);
            Assert.AreEqual(directory, environmentVariables["TMP"]);
            Assert.AreEqual("true", environmentVariables["PACKER_LOG"]);
            Assert.AreEqual(logFile, environmentVariables["PACKER_LOG_PATH"]);
        }

        [Test]
        public void ExecuteWithLogFile()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var configurationFile = Path.Combine(directory, "a.json");
            var logFile = Path.Combine(directory, "a.log");
            var variableFile = Path.Combine(directory, "v.json");
            if (!File.Exists(variableFile))
            {
                File.Create(variableFile);
            }

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

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.ConfigurationFile = new TaskItem(configurationFile);
            task.LogFile = new TaskItem(logFile);
            task.TempDirectory = new TaskItem(directory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.VariableFile = new TaskItem(variableFile);
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(4, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("-color=false", invokedArgs[1]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-var-file=\"{0}\"",
                    variableFile),
                invokedArgs[2]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\"",
                    configurationFile),
                invokedArgs[3]);

            var environmentVariables = new StringDictionary();
            environmentVariableBuilder(environmentVariables);

            Assert.AreEqual(4, environmentVariables.Count);
            Assert.AreEqual(
                "packer-cache",
                environmentVariables["PACKER_CACHE_DIR"]);
            Assert.AreEqual(directory, environmentVariables["TMP"]);
            Assert.AreEqual("true", environmentVariables["PACKER_LOG"]);
            Assert.AreEqual(logFile, environmentVariables["PACKER_LOG_PATH"]);
        }

        [Test]
        public void ExecuteWithoutConfigurationFile()
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

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.TempDirectory = new TaskItem(directory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");
            invoker.Verify(
                i => i.Invoke(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<StringDictionary>>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<bool>()),
                Times.Never());
        }

        [Test]
        public void ExecuteWithoutLogFile()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var configurationFile = Path.Combine(directory, "a.json");

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

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.ConfigurationFile = new TaskItem(configurationFile);
            task.TempDirectory = new TaskItem(directory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("build", invokedArgs[0]);
            Assert.AreEqual("-color=false", invokedArgs[1]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\"",
                    configurationFile),
                invokedArgs[2]);

            var environmentVariables = new StringDictionary();
            environmentVariableBuilder(environmentVariables);

            Assert.AreEqual(2, environmentVariables.Count);
            Assert.AreEqual(
                "packer-cache",
                environmentVariables["PACKER_CACHE_DIR"]);
            Assert.AreEqual(directory, environmentVariables["TMP"]);
        }

        [Test]
        public void ExecuteWithoutToolPath()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var configurationFile = Path.Combine(directory, "a.json");

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

            var task = new Packer(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.ConfigurationFile = new TaskItem(configurationFile);
            task.TempDirectory = new TaskItem(directory);
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully.");
            invoker.Verify(
                i => i.Invoke(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<Action<StringDictionary>>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<DataReceivedEventHandler>(),
                    It.IsAny<bool>()),
                Times.Never());
        }
    }
}
