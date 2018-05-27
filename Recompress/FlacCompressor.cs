using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace Recompress
{
    public class FlacCompressor : ICompressor
    {
        private static readonly object _lock = new object();

        public bool CanProcess(string path)
        {
            return File.Exists(path) && string.Equals(Path.GetExtension(path), ".flac", StringComparison.OrdinalIgnoreCase);
        }

        public void Process(string path)
        {
            var exe = GetFlacPath();
            if (exe == null)
                throw new Exception("Cannot find flac.exe");

            var outputFile = Path.GetTempFileName();
            using (var process = System.Diagnostics.Process.Start(exe, "--totally-silent --force --warnings-as-errors --exhaustive-model-search --best -o \"" + outputFile + "\" \"" + path + "\""))
            {
                process.WaitForExit();

                var sfi = new FileInfo(path);
                var tfi = new FileInfo(outputFile);

                if (tfi.Length > 0 && sfi.Length > tfi.Length)
                {
                    var fullName = sfi.FullName;
                    var temp = fullName + ".tmp";
                    sfi.MoveTo(temp);
                    try
                    {
                        tfi.MoveTo(fullName);
                        File.Delete(temp);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    tfi.Delete();
                }
            }
        }

        private static string GetFlacPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Recompress", "Flac", "flac.exe");
            if (File.Exists(path))
                return path;

            lock (_lock)
            {
                if (File.Exists(path))
                    return path;

                using (var httpClient = new HttpClient())
                {
                    using (var stream = httpClient.GetStreamAsync("https://ftp.osuosl.org/pub/xiph/releases/flac/flac-1.3.2-win.zip").Result)
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        var entry = archive.Entries.FirstOrDefault(e => string.Equals(e.Name, "flac.exe", StringComparison.OrdinalIgnoreCase));
                        if (entry != null)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(path));
                            entry.ExtractToFile(path, true);
                            return path;
                        }
                    }
                }
            }

            return null;
        }
    }
}
