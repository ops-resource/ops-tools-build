//-----------------------------------------------------------------------
// <copyright company="Ops-Resource">
// Copyright (c) Ops-Resource. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Build.Framework;
using NBuildKit.MsBuild.Tasks.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ops.Tools.Build.Tasks.Deploy
{
    /// <summary>
    /// Pushes one or more configuration values to the consul key - value store.
    /// </summary>
    public sealed class ConsulKV : BaseTask
    {
        private static Deserializer CreateYamlDeserializer()
        {
            var namingConvention = new CamelCaseNamingConvention();

            var builder = new DeserializerBuilder()
                .WithNamingConvention(namingConvention)
                .Build();

            return builder;
        }

        private static KeyValueEntryList GetEntryList(string path, Deserializer builder)
        {
            var input = new StringReader(File.ReadAllText(path));
            return builder.Deserialize<KeyValueEntryList>(input);
        }

        /// <summary>
        /// Gets or sets the address of a consul instance.
        /// </summary>
        [Required]
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the datacenter to which the configuration values should be send.
        /// </summary>
        [Required]
        public string Datacenter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of files that contain Consul k-v values.
        /// </summary>
        [Required]
        public ITaskItem[] Items
        {
            get;
            set;
        }

        /// <inheritdoc />
        [SuppressMessage(
            "Microsoft.Design",
            "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Logging the error so that MsBuild can handle it.")]
        public override bool Execute()
        {
            if ((Items == null) || (Items.Length == 0))
            {
                Log.LogError("No files to upload");
                return false;
            }

            using (var webClient = new WebClient())
            {
                var builder = CreateYamlDeserializer();
                foreach (var item in Items)
                {
                    var path = Path.GetFullPath(item.ItemSpec);
                    Log.LogMessage(
                        MessageImportance.Low,
                        "Processing {0} ...",
                        path);

                    var entries = GetEntryList(path, builder);
                    foreach (var entry in entries.Config)
                    {
                        try
                        {
                            var address = string.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "{0}/v1/kv/{1}?dc={2}",
                                Address,
                                entry.Key,
                                Datacenter);

                            byte[] bytes = null;
                            if (!string.IsNullOrWhiteSpace(entry.File))
                            {
                                var referencedPath = Path.Combine(Path.GetDirectoryName(path), entry.File);
                                var valueText = File.ReadAllText(referencedPath);
                                bytes = Encoding.UTF8.GetBytes(valueText);

                                Log.LogMessage(
                                    MessageImportance.Low,
                                    "Writing k-v [key: {0} - file content from: {1}] to {2}",
                                    entry.Key,
                                    referencedPath,
                                    address);
                            }
                            else
                            {
                                bytes = Encoding.UTF8.GetBytes(entry.Value.ToString());

                                Log.LogMessage(
                                    MessageImportance.Low,
                                    "Writing k-v [key: {0} - value: {1}] to {2}",
                                    entry.Key,
                                    entry.Value,
                                    address);
                            }

                            var responseBytes = webClient.UploadData(address, "PUT", bytes);
                            var response = Encoding.ASCII.GetString(responseBytes);
                            Log.LogMessage(
                                MessageImportance.Normal,
                                "Wrote to k-v at {0}. Response: {1}] to {2}",
                                entry.Key,
                                response,
                                address);
                        }
                        catch (Exception e)
                        {
                            Log.LogError(
                                "Failed to store the pair from {0}: key: {1} - value: {2} - file: {3}. The error was: {4}",
                                path,
                                entry.Key,
                                entry.Value,
                                entry.File,
                                e);
                        }
                    }
                }
            }

            // Log.HasLoggedErrors is true if the task logged any errors -- even if they were logged
            // from a task's constructor or property setter. As long as this task is written to always log an error
            // when it fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }

        [SuppressMessage(
            "Microsoft.Performance",
            "CA1812:AvoidUninstantiatedInternalClasses",
            Justification = "This class is instantiated by the Yaml deserializer.")]
        private sealed class KeyValueEntryList
        {
            public KeyValueEntry[] Config
            {
                get;
                set;
            }
        }

        [SuppressMessage(
            "Microsoft.Performance",
            "CA1812:AvoidUninstantiatedInternalClasses",
            Justification = "This class is instantiated by the Yaml deserializer.")]
        private sealed class KeyValueEntry
        {
            public string Key
            {
                get;
                set;
            }

            public string File
            {
                get;
                set;
            }

            public object Value
            {
                get;
                set;
            }
        }
    }
}
