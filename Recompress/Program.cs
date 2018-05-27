using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recompress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("Recompress [PATH]");
                return;
            }

            var compressors = new ICompressor[] { new FlacCompressor() };

            var files = GetFiles(args[0]);
            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 1;//Environment.ProcessorCount;
            Parallel.ForEach(files, options, file =>
            {
                Console.WriteLine("Processing " + file);
                foreach (var compressor in compressors)
                {
                    if (compressor.CanProcess(file))
                    {
                        compressor.Process(file);
                    }
                }
            });
        }

        private static IEnumerable<string> GetFiles(string path)
        {
            if (File.Exists(path))
            {
                yield return path;
                yield break;
            }

            IEnumerable<string> files = null;
            try
            {
                files = Directory.GetFiles(path).OrderBy(_ => _);
            }
            catch
            {
                Console.Error.WriteLine("Error while reading directory " + path);
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    yield return file;
                }
            }

            IEnumerable<string> subFolders = null;
            try
            {
                subFolders = Directory.GetDirectories(path).OrderBy(_ => _);
            }
            catch
            {
                Console.Error.WriteLine("Error while reading subdirectories " + path);
            }

            if (subFolders != null)
            {
                foreach (var subFolder in subFolders)
                {
                    foreach (var file in GetFiles(subFolder))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
