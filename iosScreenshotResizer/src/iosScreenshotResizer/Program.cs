using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iosScreenshotResizer
{
    public class Program
    {
        const int TARGET_SMALL_SIDE = 1242;
        const int TARGET_LONG_SIDE = 2208;
        const int FINAL_QUALITY = 95;

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

            Console.WriteLine($"All {filesToProcess.Count()} files processed.");
        }

        private static void ProcessFiles(IEnumerable<string> filesToProcess)
        {
            // this is an utility but let's try to do this fast
            Parallel.ForEach(filesToProcess,
               fileToProcess =>
               {
                   // we keep the folder hierarchy
                   var targetOuputPath = fileToProcess.Replace(InputDir, OutputDir);

                   ProcessFile(fileToProcess, targetOuputPath);
               });
        }

        private static void ProcessFile(string inputPath, string targetOuputPath)
        {
            try
            {
                // let's be sure the target directory exists
                var directoryName = Path.GetDirectoryName(targetOuputPath);
                Directory.CreateDirectory(directoryName);

                using (var input = File.OpenRead(inputPath))
                {
                    using (var inputStream = new SKManagedStream(input))
                    {
                        using (var original = SKBitmap.Decode(inputStream))
                        {

                            // we force the wanted size, no rounding issues
                            int width, height;
                            if (original.Width > original.Height)
                            {
                                width = TARGET_LONG_SIDE;
                                height = TARGET_SMALL_SIDE;
                            }
                            else
                            {
                                height = TARGET_LONG_SIDE;
                                width = TARGET_SMALL_SIDE;
                            }


                            // resize the image
                            using (var resized = original
                                   .Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3))
                            {
                                if (resized == null)
                                {
                                    return;
                                }

                                using (var image = SKImage.FromBitmap(resized))
                                {
                                    // write the image to the disk
                                    using (var output = File.OpenWrite(targetOuputPath))
                                    {
                                        // encode to PNG
                                        image.Encode().SaveTo(output);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"'{inputPath}' processed.");
            }
            catch (Exception e)
            {

                Console.WriteLine($"Cannot process '{inputPath}' : {e.Message}.");
            }
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
            Console.ReadLine();
        }
    }
}
