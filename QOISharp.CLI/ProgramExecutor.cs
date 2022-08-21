using QOISharp.PixelReaders;
using QOISharp.PixelWriters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.CLI
{
    public class ProgramExecutor
    {
        public class ProgramExecutorArguments
        {
            public enum ExecutionMode { Encode, Decode };
            public enum OutputFileFormats { Raw };

            public ExecutionMode Mode { get; set; }
            public string InputFilename { get; set; }
            public string OutputFilename { get; set; }
            public OutputFileFormats OutputFormat { get; set; }
        }

        public class ProgramExecutionResult
        {
            public TimeSpan ExecutionTime { get; set; }
            public long OriginalFilesize { get; set; }
            public long ResultingFilesize { get; set; }
        }

        private ProgramExecutorArguments arguments;

        public ProgramExecutionResult ExecutionResult { get; private set; }

        public ProgramExecutor(ProgramExecutorArguments arguments)
        {
            this.arguments = arguments;
            this.ExecutionResult = new ProgramExecutionResult();
        }

        public void Execute()
        {
            var sw = new Stopwatch();
            sw.Start();
            if (arguments.Mode == ProgramExecutorArguments.ExecutionMode.Encode)
                Encode();
            else
                Decode();
            sw.Stop();
            ExecutionResult.ExecutionTime = sw.Elapsed;

            if (File.Exists(arguments.InputFilename))
                ExecutionResult.OriginalFilesize = new FileInfo(arguments.InputFilename).Length;

            if (File.Exists(arguments.OutputFilename))
                ExecutionResult.ResultingFilesize = new FileInfo(arguments.OutputFilename).Length;
        }

        public void Encode()
        {
            using (var inputStream = File.OpenRead(arguments.InputFilename))
            {
                var pixelReader = PixelReaderUtilities.GetPixelReader(inputStream);
                if (File.Exists(arguments.OutputFilename))
                    File.Delete(arguments.OutputFilename);

                using (var outputStream = File.OpenWrite(arguments.OutputFilename))
                {
                    var encoder = new Encoder(pixelReader, outputStream);
                    encoder.Encode();
                }
            }
        }

        private PixelWriterBase GetPixelWriter(ProgramExecutorArguments.OutputFileFormats outputFileFormat, FileStream outputStream)
        {
            switch(outputFileFormat)
            {
                case ProgramExecutorArguments.OutputFileFormats.Raw:
                    return new RawPixelWriter(outputStream);
                default:
                    throw new ArgumentException($"Invalid output file format '{outputFileFormat.ToString()}'.");
            }
        }

        public void Decode()
        {
            using(var inputStream = File.OpenRead(arguments.InputFilename))
            {
                using (var outputStream = File.OpenWrite(arguments.OutputFilename))
                {
                    var pixelWriter = GetPixelWriter(arguments.OutputFormat, outputStream);

                    var decoder = new Decoder(inputStream, pixelWriter);
                    decoder.Decode();
                }

            }
        }
    }
}
