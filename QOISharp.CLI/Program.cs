using QOISharp.PixelReaders;
using System.Diagnostics;

namespace QOISharp.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var arguments = new ProgramExecutor.ProgramExecutorArguments();

            if(args.Length == 0)
            {
                PrintHelp();
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                var currentArg = args[i].ToLower();
                string GetSubArgument()
                {
                    if (i + 1 > args.Length || args[i + 1].StartsWith('-'))
                        throw new ArgumentException($"Incorrect number of values for argument {currentArg}");

                    return args[++i].ToLower();
                }

                bool IsArgument(string argumentName, bool acceptShorthand = true) => currentArg == $"--{argumentName}" || (acceptShorthand && currentArg == $"-{argumentName[0]}");

                if(IsArgument("help"))
                {
                    PrintHelp();
                    return;
                }
                else if (IsArgument("decode"))
                {
                    arguments.Mode = ProgramExecutor.ProgramExecutorArguments.ExecutionMode.Decode;
                }
                else if (IsArgument("encode"))
                {
                    arguments.Mode = ProgramExecutor.ProgramExecutorArguments.ExecutionMode.Encode;
                }
                else if (IsArgument("input"))
                {
                    arguments.InputFilename = GetSubArgument();
                }
                else if (IsArgument("output"))
                {
                    arguments.OutputFilename = GetSubArgument();
                }
                else if(IsArgument("format"))
                {
                    var formatString = GetSubArgument();
                    arguments.OutputFormat = Enum.Parse<ProgramExecutor.ProgramExecutorArguments.OutputFileFormats>(formatString);
                }
                else
                {
                    Console.WriteLine($"Unrecognised argument '{currentArg}'. For usage, type 'QOISharp --help'.");
                }
            }

            var programExecutor = new ProgramExecutor(arguments);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            programExecutor.Execute();
            stopwatch.Stop();
            Console.WriteLine($"Operation completed in {programExecutor.ExecutionResult.ExecutionTime.TotalMilliseconds}ms");
            Console.Write($"Original filesize: {programExecutor.ExecutionResult.OriginalFilesize / 1024 / 1024}mb, ");
            Console.Write($"Resulting filesize: {programExecutor.ExecutionResult.ResultingFilesize / 1024 / 1024}mb");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("================================================================");

            Console.WriteLine("Usage:");
            Console.WriteLine("QOISharp.CLI <command> [options]");
            Console.WriteLine();

            Console.WriteLine("Commands:");
            Console.WriteLine("--encode|-e      Indicates that an encoding operation will be performed.");
            Console.WriteLine("--decode|-d      Indicates that a decoding operation will be performed.");
            Console.WriteLine();

            Console.WriteLine("Options:");
            Console.WriteLine("--input|-i       Specifies the input file for the operation.");
            Console.WriteLine("--output|-o      Specifies the output file for the operation.");
            Console.WriteLine("--format|-f      Specifies the output file format for decoding operations. Currently only raw decoding is supported.");
            Console.WriteLine();

            Console.WriteLine("================================================================");
        }
    }
}