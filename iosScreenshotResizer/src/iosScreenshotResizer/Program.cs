using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iosScreenshotResizer
{
    public class Program
    {
        public static string InputDir { get; private set; }
        public static string OutputDir { get; private set; }

        public static void Main(string[] args)
        {
            var currDir = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current directory : {currDir}.");

            if (!TryParseArgs(args))
            {
                return;
            }

            if (!Directory.Exists(InputDir))
            {
                WriteMessageAndWaitForKey($"Input directory '{InputDir}' not found.");

                return;
            }

            DirectoryInfo targetDirectory = null;
            try
            {
                targetDirectory = Directory.CreateDirectory(OutputDir);
            }
            catch (Exception e)
            {
                WriteMessageAndWaitForKey($"Cannot create the '{OutputDir}' directory 'output' : {e.Message}");
                return;
            }

            var filesToProcess = Directory.EnumerateFiles(InputDir, "*", SearchOption.AllDirectories);
            if (!filesToProcess.Any())
            {
                WriteMessageAndWaitForKey("No file found in the input directory.");
                return;
            }

            ProcessFiles(filesToProcess);
        }

        private static void ProcessFiles(IEnumerable<string> filesToProcess)
        {

        }

        private static bool TryParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length)
                {
                    continue;
                }

                var arg = args[i];
                var value = args[i + 1];

                if (!arg.StartsWith("--"))
                {
                    continue;
                }

                arg = arg.Substring(2, arg.Length - 2);

                switch (arg)
                {
                    case "input":
                        InputDir = value;
                        break;
                    case "output":
                        OutputDir = value;
                        break;
                }
            }

            if (string.IsNullOrEmpty(InputDir))
            {
                WriteMessageAndWaitForKey("InputDir is mandatory.");
                return false;
            }

            if (string.IsNullOrEmpty(OutputDir))
            {
                WriteMessageAndWaitForKey("OutputDir is mandatory.");
                return false;
            }

            return true;
        }

        private static void WriteMessageAndWaitForKey(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }
    }
}
