using BenchmarkDotNet.Attributes;
using QOISharp.PixelReaders;
using QOISharp.PixelWriters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private static readonly string encoderFilename = AppContext.BaseDirectory + @"..\..\..\..\decoded.ppm";
        private static readonly string decoderFilename = AppContext.BaseDirectory + @"..\..\..\..\encoded.qoi";

        private Stream encoderInputStream;
        private Stream encoderOutputStream;
        private Stream decoderInputStream;
        private Stream decoderOutputStream;

        private byte[] encoderFileData;
        private byte[] decoderFileData;
        //private Stream inputStream;
        //private Stream outputStream;
        private PixelReaderBase pixelReader;
        private PixelWriterBase pixelWriter;

        private Encoder encoder;
        private Decoder decoder;

        [GlobalSetup]
        public void GlobalInit()
        {
            if (!File.Exists(encoderFilename))
                throw new FileNotFoundException($"{encoderFilename} could not be found in {AppContext.BaseDirectory}.");

            if (!File.Exists(decoderFilename))
                throw new FileNotFoundException($"{decoderFilename} could not be found in {AppContext.BaseDirectory}.");

            encoderFileData = File.ReadAllBytes(encoderFilename);
            decoderFileData = File.ReadAllBytes(decoderFilename);

            encoderInputStream = new MemoryStream(encoderFileData);
            decoderInputStream = new MemoryStream(decoderFileData);

            encoderOutputStream = Stream.Null;
            decoderOutputStream = Stream.Null;
        }

        [IterationSetup]
        public void Init()
        {
            encoderInputStream.Seek(0, SeekOrigin.Begin);
            decoderInputStream.Seek(0, SeekOrigin.Begin);

            pixelReader = PixelReaderUtilities.GetPixelReader(encoderInputStream);
            pixelWriter = new NullPixelWriter();

            encoder = new Encoder(pixelReader, encoderOutputStream);
            decoder = new Decoder(decoderInputStream, pixelWriter);

            GC.Collect();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            encoderInputStream.Dispose();
            encoderOutputStream.Dispose();

            decoderInputStream.Dispose();
            decoderOutputStream.Dispose();
        }

        [Benchmark]
        public void Encode()
        {
            encoder.Encode();
        }

        [Benchmark]
        public void Decode()
        {
            decoder.Decode();
        }
    }
}
