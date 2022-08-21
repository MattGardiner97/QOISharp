# QOISharp
## About
A C# implementation of the Quite OK Image (QOI) format by Dominic Szablewski desribed [here](https://qoiformat.org/). QOI is able to compress images to a size very comparable to that of the PNG format, however it can encode and decode images significantly faster. QOI's biggest draw is its simplicity - the format specification fits on a single page ([here](https://qoiformat.org/qoi-specification.pdf)) compared to the 92 page specification of the PNG format.

## Requirements
- .NET 6.0+

## Usage
QOISharp can be used via the command line or via the QOISharp library.

### Supported File Formats
QOISharp is currently only capable of encoding images in the Netpbm Portable Pixel Map format ([described here](http://netpbm.sourceforge.net/doc/ppm.html)) and is currently only able to decode images to a raw binary format.

## Benchmarks
QOISharp was tested using a number of files from the image compression test image collection found [here](http://imagecompression.info/test_images/).

A benchmark of the encoding and decoding process was completed using the "big_building" image from this collection. The original file measured at 228mb and was reduced down to 77mb using QOISharp.

The Benchmark.NET results are as follows.
```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT
  Job-XPIDBR : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT

InvocationCount=1  UnrollFactor=1

| Method |       Mean |    Error |   StdDev | Allocated |
|------- |-----------:|---------:|---------:|----------:|
| Encode | 2,143.5 ms | 17.95 ms | 15.91 ms |     528 B |
| Decode |   780.4 ms |  5.73 ms |  4.78 ms |     528 B |
```
