﻿//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using NBuildKit.MsBuild.Tasks.Tests;
using Nuclei;
using NUnit.Framework;

namespace Ops.Tools.Build.Tasks.FileSystem
{
    [TestFixture]
    public sealed class CalculateFileHashTest : TaskTest
    {
        [Test]
        public void ExecuteWithEmptyPath()
        {
            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "md5";
            task.Path = new TaskItem(string.Empty);

            var result = task.Execute();
            Assert.IsFalse(result);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 1, numberOfWarningMessages: 0, numberOfNormalMessages: 0);
        }

        [Test]
        public void ExecuteWithInvalidHashAlgorithm()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var filePath = Path.Combine(directory, "FileToHash.txt");

            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "stuff";
            task.Path = new TaskItem(filePath);

            var result = task.Execute();
            Assert.IsFalse(result);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 1, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }

        [Test]
        public void ExecuteWithMD5()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var filePath = Path.Combine(directory, "FileToHash.txt");

            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "md5";
            task.Path = new TaskItem(filePath);

            var result = task.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual("B5D4379F6B3E960EA12132B34E8E65C9", task.Hash);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 0, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }

        [Test]
        public void ExecuteWithNonExistingPath()
        {
            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "md5";
            task.Path = new TaskItem(@"c:\this\path\does\not\exist.txt");

            var result = task.Execute();
            Assert.IsFalse(result);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 1, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }

        [Test]
        public void ExecuteWithNullPath()
        {
            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "md5";
            task.Path = null;

            var result = task.Execute();
            Assert.IsFalse(result);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 1, numberOfWarningMessages: 0, numberOfNormalMessages: 0);
        }

        [Test]
        public void ExecuteWithSHA1()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var filePath = Path.Combine(directory, "FileToHash.txt");

            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "sha1";
            task.Path = new TaskItem(filePath);

            var result = task.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual("FFC0F8E69E3753E3A4087E197C160261F3EF11D5", task.Hash);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 0, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }

        [Test]
        public void ExecuteWithSHA256()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var filePath = Path.Combine(directory, "FileToHash.txt");

            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "sha256";
            task.Path = new TaskItem(filePath);

            var result = task.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual("A65235E41B8072B726FDEBB63DEEF3EBDFE4FED516F510608DA6BB1497ED11BA", task.Hash);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 0, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }

        [Test]
        public void ExecuteWithSHA512()
        {
            var directory = Assembly.GetExecutingAssembly().LocalDirectoryPath();
            var filePath = Path.Combine(directory, "FileToHash.txt");

            InitializeBuildEngine();

            var task = new CalculateFileHash();
            task.BuildEngine = BuildEngine.Object;
            task.Algorithm = "sha512";
            task.Path = new TaskItem(filePath);

            var result = task.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual("9E340E6C4B70FA6E9F1F5D25859F57E1FCC7A0B3A49B94AE5B3E64090FCC4CA4E2490E10556C85E0B84F634B68EC6C7312F834A875A76EC5996ABDED70C214F1", task.Hash);

            VerifyNumberOfLogMessages(numberOfErrorMessages: 0, numberOfWarningMessages: 0, numberOfNormalMessages: 1);
        }
    }
}
