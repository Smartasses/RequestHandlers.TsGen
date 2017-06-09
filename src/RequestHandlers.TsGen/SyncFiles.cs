﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen
{
    class SyncFiles
    {

        public void DoSync(string path, IEnumerable<KeyValuePair<string, string>> files)
        {
            files.ToArray();
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var dict = files.ToDictionary(x => Path.Combine(path, x.Key), x => x.Value);
            var typescriptFiles = Directory.GetFileSystemEntries(path, "*.ts", SearchOption.AllDirectories);
            typescriptFiles.Where(x => !dict.ContainsKey(x)).ForEach(File.Delete);
            Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Where(x => Directory.GetFiles(x).Length == 0).ForEach(
                x =>
                {
                    if (Directory.Exists(x))
                        Directory.Delete(x, true);
                });
            var dirHash = new HashSet<string>();
            foreach (var file in dict)
            {
                var dir = Path.GetDirectoryName(file.Key);
                if (!dirHash.Contains(dir))
                {
                    dirHash.Add(dir);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                File.WriteAllText(file.Key, file.Value);
            }
        }
    }
}