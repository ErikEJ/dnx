using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Dnx
{
    class Program
    {
        private static ConsoleColor defaultConsoleColor;

        static int Main(string[] args)
        {
            Initialize();
            var arguments = new MainArgs(args);
            var (found, binary) = FindBinaryInPath(arguments.Command);
            if (!found)
                (found, binary) = Install(arguments.Package, arguments.Version, arguments.Command, arguments.Verbose);
            else if (arguments.Verbose)
                WriteLine($"Tool {arguments.Command} already installed.");
            if (!found)
                return 1;
            return Run(binary, arguments.Arguments, arguments.Verbose, showStdout: true);
        }


        private static void Initialize() => defaultConsoleColor = Console.ForegroundColor;

        private static int Run(string binary, string[] arguments, bool verbose, bool showStdout)
        {
            if (verbose)
                showStdout = true;
            var argumentsString = string.Join(' ', arguments);
            if (verbose)
                WriteLine($"Running: {binary} {argumentsString}");
            var startInfo = new ProcessStartInfo(binary, argumentsString)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var process = new Process() { StartInfo = startInfo };
            if (showStdout)
                process.OutputDataReceived += (sender, e) => WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => WriteErrorLine(e.Data);
            process.Start();
            if (showStdout)
                process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }

        private static (bool found, string binary) Install(string package, string version, string command, bool verbose)
        {
            if (verbose)
                WriteLine("Tool not found, installing...");
            var (dotnetFound, dotnet) = FindDotNet();
            if (!dotnetFound)
                return (false, null);
            var arguments = $"install tool -g {(version == null ? "" : $"--version {version}")} {package}".Trim().Split(' ');
            var exitCode = Run(dotnet, arguments, verbose, false);
            if (exitCode != 0)
            {
                WriteErrorLine("Could not install.");
                return (false, null);
            }
            return FindBinaryInPath(command);
        }

        private static (bool found, string binary) FindDotNet() => FindBinaryInPath("dotnet");

        private static (bool found, string binary) FindBinaryInPath(string command)
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var correctPath = GetPath(path, command);
                if (File.Exists(correctPath))
                    return (true, correctPath);
            }
            return (false, null);
        }

        private static string GetPath(string path, string command) =>
            Path.Combine(path, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{command}.exe" : command);

        private static readonly object sync = new object();

        private static void WriteLine(string message)
        {
            if (message == null)
                return;
            lock (sync)
                Console.WriteLine(message);
        }

        private static void WriteErrorLine(string message)
        {
            if (message == null)
                return;
            lock (sync)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine(message);
                Console.ForegroundColor = defaultConsoleColor;
            }
        }
    }
}
