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
using NUnit.Framework;

namespace Ops.Tools.Build.Tasks.Package
{
    [TestFixture]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Unit tests do not need documentation.")]
    public sealed class TerraformPlanTest : TaskTest
    {
        [Test]
        public void ExecuteWithoutSettings()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var deployDirectory = Path.Combine(directory, "deploy");
            var logFile = Path.Combine(directory, "a.log");
            var planFile = Path.Combine(deployDirectory, "plan");

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

            var task = new TerraformPlan(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.TerraformDataDirectory = new TaskItem(deployDirectory);
            task.LogFile = new TaskItem(logFile);
            task.TerraformExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
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
        public void ExecuteWithoutTerraformExecutable()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var deployDirectory = Path.Combine(directory, "deploy");
            var logFile = Path.Combine(directory, "a.log");
            var planFile = Path.Combine(deployDirectory, "plan");

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

            var task = new TerraformPlan(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.Settings = new TaskItem(planFile);

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
        public void ExecuteWithoutVariables()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var deployDirectory = Path.Combine(directory, "deploy");
            var logFile = Path.Combine(directory, "a.log");
            var planFile = Path.Combine(deployDirectory, "plan");

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

            var task = new TerraformPlan(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.TerraformDataDirectory = new TaskItem(deployDirectory);
            task.LogFile = new TaskItem(logFile);
            task.Settings = new TaskItem(planFile);
            task.TerraformExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.WorkingDirectory = new TaskItem(directory);

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(2, invokedArgs.Count);
            Assert.AreEqual("plan", invokedArgs[0]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-out={0}",
                    planFile),
                invokedArgs[1]);

            var environmentVariables = new StringDictionary();
            environmentVariableBuilder(environmentVariables);

            Assert.AreEqual(6, environmentVariables.Count);
            Assert.AreEqual("true", environmentVariables["TF_IN_AUTOMATION"]);
            Assert.AreEqual("false", environmentVariables["TF_INPUT"]);
            Assert.AreEqual("-no-color", environmentVariables["TF_CLI_ARGS"]);
            Assert.AreEqual("DEBUG", environmentVariables["TF_LOG"]);
            Assert.AreEqual(logFile, environmentVariables["TF_LOG_PATH"]);
            Assert.AreEqual(deployDirectory, environmentVariables["TF_DATA_DIR"]);
        }

        [Test]
        public void ExecuteWithVariables()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var deployDirectory = Path.Combine(directory, "deploy");
            var logFile = Path.Combine(directory, "a.log");
            var planFile = Path.Combine(deployDirectory, "plan");

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

            var task = new TerraformPlan(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.TerraformDataDirectory = new TaskItem(deployDirectory);
            task.LogFile = new TaskItem(logFile);
            task.TerraformExecutablePath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());
            task.WorkingDirectory = new TaskItem(directory);

            task.Settings = new TaskItem(planFile);

            var variableName = "name";
            var variableValue = "value";
            task.Settings.SetMetadata(variableName, variableValue);

            var result = task.Execute();
            Assert.IsTrue(result, "The task should have executed successfully.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(3, invokedArgs.Count);
            Assert.AreEqual("plan", invokedArgs[0]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-var {0}={1}",
                    variableName,
                    variableValue),
                invokedArgs[1]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-out={0}",
                    planFile),
                invokedArgs[2]);

            var environmentVariables = new StringDictionary();
            environmentVariableBuilder(environmentVariables);

            Assert.AreEqual(6, environmentVariables.Count);
            Assert.AreEqual("true", environmentVariables["TF_IN_AUTOMATION"]);
            Assert.AreEqual("false", environmentVariables["TF_INPUT"]);
            Assert.AreEqual("-no-color", environmentVariables["TF_CLI_ARGS"]);
            Assert.AreEqual("DEBUG", environmentVariables["TF_LOG"]);
            Assert.AreEqual(logFile, environmentVariables["TF_LOG_PATH"]);
            Assert.AreEqual(deployDirectory, environmentVariables["TF_DATA_DIR"]);
        }
    }
}
