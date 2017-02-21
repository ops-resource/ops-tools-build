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
using System.Reflection;
using System.Text;
using Microsoft.Build.Utilities;
using Moq;
using NBuildKit.MsBuild.Tasks.Core;
using NBuildKit.MsBuild.Tasks.Tests;
using Nuclei;
using NUnit.Framework;

namespace Ops.Tools.Build.Tasks.Package
{
    [TestFixture]
    public class IsoTest : TaskTest
    {
        [Test]
        public void Execute()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var tempDirectory = Path.Combine(directory, Guid.NewGuid().ToString());
            var configPath = Path.Combine(
                directory,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.xml",
                    Guid.NewGuid().ToString()));
            var text =
@"<?xml version='1.0'?>
<iso>
    <name>Execute</name>
    <files>
        <file src='{0}' target='' />
    </files>
</iso>
";
            using (var writer = new StreamWriter(configPath, false, Encoding.UTF8))
            {
                writer.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        text,
                        Assembly.GetExecutingAssembly().LocalFilePath()));
            }

            InitializeBuildEngine();

            var invokedPath = string.Empty;
            var invokedArgs = new List<string>();
            var invokedWorkingDirectory = string.Empty;
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
                        });
            }

            var task = new Iso(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.File = new TaskItem(configPath);
            task.OutputDirectory = new TaskItem(directory);
            task.TemporaryDirectory = new TaskItem(string.Empty);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully because there are no files in the ISO.");

            Assert.AreEqual(Assembly.GetExecutingAssembly().LocalFilePath(), invokedPath);
            Assert.AreEqual(directory, invokedWorkingDirectory);

            Assert.AreEqual(5, invokedArgs.Count);
            Assert.AreEqual("-r", invokedArgs[0]);
            Assert.AreEqual("-iso-level 4", invokedArgs[1]);
            Assert.AreEqual("-UDF", invokedArgs[2]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "-o \"{0}\"",
                    Path.Combine(directory, "Execute.iso")),
                invokedArgs[3]);
            Assert.AreEqual(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\"",
                    tempDirectory),
                invokedArgs[4]);
        }

        [Test]
        public void ExecuteWithEmptyInputFile()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var tempDirectory = Path.Combine(directory, Guid.NewGuid().ToString());
            var configPath = Path.Combine(
                directory,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.xml",
                    Guid.NewGuid().ToString()));
            var text =
@"<?xml version='1.0'?>
<iso>
    <name>ExecuteWithEmptyInputFile</name>
    <files>
    </files>
</iso>
";
            using (var writer = new StreamWriter(configPath, false, Encoding.UTF8))
            {
                writer.WriteLine(text);
            }

            InitializeBuildEngine();

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
                    .Verifiable();
            }

            var task = new Iso(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.File = new TaskItem(configPath);
            task.OutputDirectory = new TaskItem(directory);
            task.TemporaryDirectory = new TaskItem(tempDirectory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully because there are no files in the ISO.");
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
        public void ExecuteWithoutInputFile()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var tempDirectory = Path.Combine(directory, Guid.NewGuid().ToString());

            InitializeBuildEngine();

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
                    .Verifiable();
            }

            var task = new Iso(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.File = new TaskItem(string.Empty);
            task.OutputDirectory = new TaskItem(directory);
            task.TemporaryDirectory = new TaskItem(tempDirectory);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully because there are no files in the ISO.");
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
        public void ExecuteWithoutTemporaryDirectory()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var configPath = Path.Combine(
                directory,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.xml",
                    Guid.NewGuid().ToString()));
            var text =
@"<?xml version='1.0'?>
<iso>
    <name>ExecuteWithEmptyInputFile</name>
    <files>
        <file src='{0}' target='' />
    </files>
</iso>
";
            using (var writer = new StreamWriter(configPath, false, Encoding.UTF8))
            {
                writer.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        text,
                        Assembly.GetExecutingAssembly().LocalFilePath()));
            }

            InitializeBuildEngine();

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
                    .Verifiable();
            }

            var task = new Iso(invoker.Object);
            task.BuildEngine = BuildEngine.Object;
            task.File = new TaskItem(configPath);
            task.OutputDirectory = new TaskItem(directory);
            task.TemporaryDirectory = new TaskItem(string.Empty);
            task.ToolPath = new TaskItem(Assembly.GetExecutingAssembly().LocalFilePath());

            var result = task.Execute();
            Assert.IsFalse(result, "The task should not have executed successfully because there are no files in the ISO.");
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
