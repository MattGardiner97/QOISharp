namespace QOISharp.Benchmarks;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using QOISharp.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}