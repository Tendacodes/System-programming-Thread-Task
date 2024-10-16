using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        JsonProcessor processor = new JsonProcessor();
        CancellationTokenSource cts = new CancellationTokenSource();

        Console.WriteLine("Select processing mode (1 - Single, 2 - Multi): ");
        string mode = Console.ReadLine();

        Console.WriteLine("Enter JSON file paths (comma-separated): ");
        string input = Console.ReadLine();
        string[] filePaths = input.Split(',');

        
        Task.Run(() => WaitForCancellation(cts));

        await processor.ProcessFilesAsync(mode, filePaths, cts.Token);
    }

    static void WaitForCancellation(CancellationTokenSource cts)
    {
        Console.WriteLine("Press 'c' to cancel the process.");
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.C)
            {
                cts.Cancel();
                break;
            }
        }
    }
}

class JsonProcessor
{
    public async Task ProcessFilesAsync(string mode, string[] filePaths, CancellationToken token)
    {
        if (mode == "1")
        {
            await ProcessSingleThreadAsync(filePaths, token);
        }
        else if (mode == "2")
        {
            await ProcessMultiThreadAsync(filePaths, token);
        }
        else
        {
            Console.WriteLine("Invalid mode selected.");
        }
    }

    private async Task ProcessSingleThreadAsync(string[] filePaths, CancellationToken token)
    {
        foreach (var path in filePaths)
        {
            token.ThrowIfCancellationRequested();
            await Task.Run(() => DisplayJsonContent(path));
        }
    }

    private async Task ProcessMultiThreadAsync(string[] filePaths, CancellationToken token)
    {
        List<Task> tasks = new List<Task>();

        foreach (var path in filePaths)
        {
            token.ThrowIfCancellationRequested();
            tasks.Add(Task.Run(() => DisplayJsonContent(path), token));
        }

        await Task.WhenAll(tasks);
    }

    private void DisplayJsonContent(string filePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            Console.WriteLine($"File: {Path.GetFileName(filePath)}\nContent:\n{jsonContent}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
        }
    }
}
