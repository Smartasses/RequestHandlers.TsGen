using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RequestHandlers.TsGen.Helpers;

namespace RequestHandlers.TsGen
{
    class SyncFiles
    {

        public void DoSync(string path, IEnumerable<KeyValuePair<string, string>> files)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            var dict = files.ToDictionary(x => Path.Combine(path, x.Key), x => x.Value);
            RemoveUnused(path, dict);
            CreateDirectories(dict);

            Parallel.ForEach(dict.Select(x => new {FilePath = x.Key, Content = x.Value, Hash = GetHash(x.Value)}),
                file =>
                {
                    var hashComment = $"// hash: {file.Hash}";
                    bool needsWrite = false;
                    using (var strm = new FileStream(file.FilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                    using (var fileReader = new StreamReader(strm))
                    {
                        var hash = fileReader.ReadLine()?.Trim();
                        needsWrite = hash != hashComment;
                    }
                    if (needsWrite)
                    {
                        Console.WriteLine("Writing file: " + file.FilePath);
                        RetryTimes(() => {
                            using (var strm = new FileStream(file.FilePath, FileMode.Create, FileAccess.Write,
                                FileShare.Write))
                            using (var fileWriter = new StreamWriter(strm))
                            {
                                fileWriter.WriteLine(hashComment);
                                fileWriter.Write(file.Content);
                            }
                        }, 3);
                    }
                    else
                    {
                        Console.WriteLine("Skip file: " + file.FilePath);
                    }
                });
        }

        private void RetryTimes(Action action, int times)
        {
            var tryCount = 0;
            bool succeeded = false;
            do
            {
                try
                {
                    action();
                    succeeded = true;
                }
                catch
                {
                    tryCount++;
                    Thread.Sleep(tryCount * 50 + 50);
                }
            } while (!succeeded && times > tryCount);
        }

        private string GetHash(string content)
        {
            var messageBytes = Encoding.UTF8.GetBytes(content);
#if NET45
            var sha1Managed = new SHA1Managed();
            var hash = sha1Managed.ComputeHash(messageBytes);
#else
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(messageBytes);
#endif
            return Convert.ToBase64String(hash);
        }

        private static void CreateDirectories(Dictionary<string, string> dict)
        {
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
            }
        }

        private static void RemoveUnused(string path, Dictionary<string, string> dict)
        {
            var typescriptFiles = Directory.GetFileSystemEntries(path, "*.ts", SearchOption.AllDirectories);
            typescriptFiles.Where(x => !dict.ContainsKey(x)).ForEach(File.Delete);
            Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Where(x => Directory.GetFiles(x).Length == 0)
                .ForEach(
                    x =>
                    {
                        if (Directory.Exists(x))
                            Directory.Delete(x, true);
                    });
        }
    }
}