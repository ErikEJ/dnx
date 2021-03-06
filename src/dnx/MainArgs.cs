using DocoptNet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dnx
{
    public class MainArgs
    {
        private const string usage = @"dnx - The .NET Runner tool

Usage:
  dnx [options] <command> [--] [<arguments>]...

Options:
  -p PACKAGE, --package PACKAGE     Package name to use, instead of command name
  --package-version VERSION         Version to install
  --verbose                         Verbose install and run
  --version, -v                     Show version number
  --help, -h                        Show help
";

        public MainArgs(string[] argv)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var args = new Docopt().Apply(usage, argv, version: version, exit: true);
            Command = args["<command>"].ToString();
            Arguments = args["<arguments>"].AsList.Cast<ValueObject>().Select(d => d.ToString()).ToArray();
            Package = args["--package"]?.ToString() ?? Command;
            Version = args["--package-version"]?.ToString();
            Verbose = args["--verbose"].IsTrue;
        }

        public string Command { get; }
        public string[] Arguments { get; }
        public string Package { get; }
        public string Version { get; }
        public bool Verbose { get; }
    }
}
