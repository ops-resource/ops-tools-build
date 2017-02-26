//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;
using NBuildKit.MsBuild.Tasks.Core.FileSystem;

namespace Ops.Tools.Build.Tasks.Package
{
    /// <summary>
    /// Defines a <see cref="ITask"/> that reads an 'isospec' file and creates the associated ISO file.
    /// </summary>
    public sealed class Iso : CommandLineToolTask
    {
        private static IEnumerable<FileInfo> GetFilteredFilePaths(string baseDirectory, string fileFilter, bool recurse)
        {
            var dirInfo = new DirectoryInfo(baseDirectory);
            return dirInfo.EnumerateFiles(fileFilter, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iso"/> class.
        /// </summary>
        public Iso()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iso"/> class.
        /// </summary>
        /// <param name="invoker">The object which handles the invocation of the command line applications.</param>
        public Iso(IApplicationInvoker invoker)
            : base(invoker)
        {
        }

        private void CreateIsoLayout(Dictionary<string, List<string>> files)
        {
            var destinationDirectory = GetAbsolutePath(TemporaryDirectory);
            foreach (var pair in files)
            {
                var filePath = pair.Key;
                var list = pair.Value;

                foreach (var relativePath in list)
                {
                    var absolutePath = Path.GetFullPath(Path.Combine(destinationDirectory, relativePath));
                    Log.LogMessage(
                        MessageImportance.Low,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Copying: {0}. To: {1}",
                            filePath,
                            absolutePath));
                    var directory = Path.GetDirectoryName(absolutePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    System.IO.File.Copy(filePath, absolutePath, OverwriteExistingFiles);
                }
            }
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(File.ItemSpec))
            {
                Log.LogError("Output path for the ISO file is not defined. Unable to create an ISO file.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TemporaryDirectory.ItemSpec))
            {
                Log.LogError("The temporary directory is not defined. Unable to create an ISO file.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ToolPath.ItemSpec))
            {
                Log.LogError("The file path to the 'mkisofs' executable is not defined. Unable to create an ISO file.");
                return false;
            }

            var workingDirectory = GetAbsolutePath(WorkingDirectory);
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            var inputPath = GetAbsolutePath(File);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(inputPath);
            var name = xmlDoc.SelectSingleNode("//iso/name/text()").InnerText;
            var outputFilePath = Path.Combine(GetAbsolutePath(OutputDirectory), string.Format(CultureInfo.InvariantCulture, "{0}.iso", name));

            var files = new Dictionary<string, List<string>>();
            var filesNode = xmlDoc.SelectSingleNode("//iso/files");
            foreach (XmlNode child in filesNode.ChildNodes)
            {
                var excludedFiles = new List<string>();
                var excludedAttribute = child.Attributes["exclude"];
                var excluded = (excludedAttribute != null ? excludedAttribute.Value : string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excluded)
                {
                    var pathSections = exclude.Split(new[] { "**" }, StringSplitOptions.RemoveEmptyEntries);

                    var directory = pathSections.Length == 1 ? Path.GetDirectoryName(pathSections[0]) : pathSections[0].Trim('\\');
                    if (!Path.IsPathRooted(directory))
                    {
                        directory = Path.Combine(workingDirectory, directory);
                    }

                    var fileFilter = pathSections.Length == 1 ? Path.GetFileName(pathSections[0]) : pathSections[pathSections.Length - 1].Trim('\\');
                    var recurse = pathSections.Length > 1;
                    var filesToExclude = GetFilteredFilePaths(directory, fileFilter, recurse).Select(f => f.FullName);
                    excludedFiles.AddRange(filesToExclude);
                }

                var targetAttribute = child.Attributes["target"];
                var target = targetAttribute != null ? targetAttribute.Value : string.Empty;

                var sourceAttribute = child.Attributes["src"];
                var sources = sourceAttribute.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var source in sources)
                {
                    var pathSections = source.Split(new[] { "**" }, StringSplitOptions.RemoveEmptyEntries);

                    var directory = pathSections.Length == 1 ? Path.GetDirectoryName(pathSections[0]) : pathSections[0].Trim('\\');
                    if (!Path.IsPathRooted(directory))
                    {
                        directory = Path.Combine(workingDirectory, directory);
                    }

                    var fileFilter = pathSections.Length == 1 ? Path.GetFileName(pathSections[0]) : pathSections[pathSections.Length - 1].Trim('\\');
                    var recurse = pathSections.Length > 1;
                    var filesToInclude = GetFilteredFilePaths(directory, fileFilter, recurse)
                        .Where(f => !excludedFiles.Contains(f.FullName))
                        .Select(f => f.FullName);

                    foreach (var file in filesToInclude)
                    {
                        var relativefilePath = PathUtilities.GetFilePathRelativeToDirectory(file, directory);
                        var relativePath = Path.Combine(
                            target,
                            relativefilePath);
                        if (!files.ContainsKey(file))
                        {
                            files.Add(file, new List<string>());
                        }

                        var list = files[file];
                        list.Add(relativePath);
                    }
                }
            }

            if (!files.Any())
            {
                Log.LogError(
                    "The configuration file at: '{0}' does not contain any files to add to the ISO. Cannot make an ISO without files.",
                    inputPath);
                return false;
            }

            Log.LogMessage(MessageImportance.Normal, string.Format(CultureInfo.InvariantCulture, "Creating archive at: {0}", outputFilePath));
            CreateIsoLayout(files);

            var arguments = new List<string>();
            {
                // This is like the -R option, but file ownership and modes are set to more useful values. The uid and gid are set to zero, because
                // they are usually only useful on the author’s system, and not useful to the client. All the file read bits are set true, so
                // that files and directories are globally readable on the client. If any execute bit is set for a file, set all of the execute
                // bits, so that executables are globally executable on the client. If any search bit is set for a directory, set all of the search
                // bits, so that directories are globally searchable on the client. All write bits are cleared, because the CD-Rom will be mounted
                // read-only in any case. If any of the special mode bits are set, clear them, because file locks are not useful on a read-only
                // file system, and set-id bits are not desirable for uid 0 or gid 0. When used on Win32, the execute bit is set on all files.
                // This is a result of the lack of file permissions on Win32 and the Cygwin POSIX emulation layer.  See also  -uid  -gid,
                // -dir-mode, -file-mode and -new-dir-mode.
                arguments.Add("-r");

                // Level 4 officially does not exists but mkisofs maps it to ISO - 9660:1999 which is ISO - 9660 version 2.
                //
                // With level 4, an enhanced volume descriptor with version number and file structure version number set to 2 is
                // emitted.There may be more than 8 levels of directory nesting, there is no need for a file to contain a dot
                // and the dot has no more special meaning, file names do not have version numbers, the maximum length for
                // files and directory is raised to 207.If Rock Ridge is used, the maximum ISO - 9660 name length is reduced to 197.
                //
                // When creating Version 2 images, mkisofs emits an enhanced volume descriptor which looks similar to a primary
                // volume descriptor but is slightly different.Be careful not to use broken software to make ISO - 9660 images
                // bootable by assuming a second PVD copy and patching this putative PVD copy into an El Torito VD.
                arguments.Add("-iso-level 4");

                // Include UDF support in the generated filesystem image. UDF sup-port is currently in alpha status and for this reason, it is not
                // possible to create UDF only images.  UDF data structures are currently coupled to the Joliet structures, so there are many
                // pitfalls with the current implementation. There is no UID/GID support, there is no POSIX permission support, there is no sup-
                // port for symlinks. Note that UDF wastes the space from sector ~20 to sector 256 at the beginning of the disk in addition to
                // the spcae needed for real UDF data structures.
                arguments.Add("-UDF");

                // Define the output file path location
                arguments.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "-o \"{0}\"",
                        outputFilePath));

                // Define the directory from which the files should be taken
                arguments.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "\"{0}\"",
                        GetAbsolutePath(TemporaryDirectory)));
            }

            DataReceivedEventHandler standardErrorHandler = (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Log.LogWarning(e.Data);
                }
            };

            InvokeCommandLineTool(
                ToolPath,
                arguments,
                WorkingDirectory,
                DefaultDataHandler,
                standardErrorHandler);

            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Gets or sets the path to the ISO specification file.
        /// </summary>
        [Required]
        public ITaskItem File
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the output directory.
        /// </summary>
        [Required]
        public ITaskItem OutputDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether existing files should be overwritten. Defaults to false.
        /// </summary>
        public bool OverwriteExistingFiles
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full path to a directory into which temporary files may be created.
        /// </summary>
        [Required]
        public ITaskItem TemporaryDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the 'mkisofs' command line tool.
        /// </summary>
        [Required]
        public ITaskItem ToolPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the working directory.
        /// </summary>
        [Required]
        public ITaskItem WorkingDirectory
        {
            get;
            set;
        }
    }
}
