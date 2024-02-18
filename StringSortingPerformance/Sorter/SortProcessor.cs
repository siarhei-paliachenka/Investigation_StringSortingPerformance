﻿using System.Diagnostics;

namespace Sorter;

public class SortProcessor
{
    const string ChunksDirectoryName = "chunks";

    public void Sort(string sourceDirectory,
        string sourceFileName,
        string resultFileName,
        CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var sourcePath = Path.Combine(sourceDirectory, sourceFileName);

        var chunksDirectoryPath = CreateChunkDirectory(sourceDirectory);
        var chunkPaths = CreatedSortedChunks(sourceDirectory, sourceFileName, chunksDirectoryPath);

        var resultFilePath = Path.Combine(sourceDirectory, resultFileName);

        stopwatch.Stop();

        Console.WriteLine($"File {resultFilePath} is created in {stopwatch.Elapsed} s");
    }

    private string CreateChunkDirectory(string sourceDirectory)
    {
        var directoryPath = Path.Combine(sourceDirectory, 
            $"{ChunksDirectoryName}_{DateTime.Now:yyyy'_'MM'_'dd'_'HH'_'mm'_'ss}");
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private IEnumerable<string> CreatedSortedChunks(string sourceDirectory, string sourceFileName, string chunksDirectoryPath)
    {
        var chunkCounter = 0;

        return File.ReadLines(Path.Combine(sourceDirectory, sourceFileName))
            .AsParallel()
            .Select(x => new Item(x))
            .Chunk(ChunkSize)
            .AsParallel()
            .Select(chunk =>
            {
                chunk.AsSpan().Sort();
                var chunkNumber = Interlocked.Increment(ref chunkCounter);
                var chunkFilePath = Path.Combine(chunksDirectoryPath, $"chunk_{chunkNumber}.txt");
                File.WriteAllLines(chunkFilePath, chunk.Select(x => x.Source));
                Console.WriteLine($"{DateTime.Now:hh:mm:ss fff}:_{chunkFilePath}");
                return chunkFilePath;
            }).AsUnordered().ToArray();
    }

    private int ChunkSize => 1_000_000;
}