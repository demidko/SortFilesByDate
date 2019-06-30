using System;
using System.IO;
using System.Linq;
using System.Threading;

static class Program
{
    static DirectoryInfo DirectoryInfoFrom(in string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Invalid arguments!\n" +
                "Usage: dotnet SortFilesByDate [Directory]\n");
            Environment.Exit(1);
        }
        var res = new DirectoryInfo(args.First());
        if (!res.Exists)
        {
            Console.WriteLine(
                $"'{res.Name}' directory does not exist!");
            Environment.Exit(1);
        }
        return res;
    }

    static readonly Mutex IOMutex = new Mutex();

    static bool Prepare(this DirectoryInfo dir)
    {
        if (dir.Exists)
        {
            return true;
        }
        try
        {
            IOMutex.WaitOne();
            dir.Create();
            return true;
        }
        catch (Exception e)
        {
            var currColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = currColor;
            return false;
        }
        finally
        {
            IOMutex.ReleaseMutex();
        }
    }

    static void MoveFileByDate(in FileInfo file)
    {
        var directory = new DirectoryInfo(Path.Combine(
                file.DirectoryName,
                file.LastWriteTime.ToString("yyyy.MM.dd")));
        if (directory.Prepare())
        {
            file.MoveTo(Path.Combine(directory.FullName, file.Name));
            Console.WriteLine($"{file.Name} --> {directory.Name}");
        }
    }

    static void Main(string[] args)
    {
        var directory = DirectoryInfoFrom(args);
        directory.EnumerateFiles()
            .AsParallel()
            .ForAll(MoveFileByDate);
        Console.WriteLine($"All files in the '{directory.Name}'" +
            " directory are sorted.");
        Console.ReadKey(true);
    }
}

