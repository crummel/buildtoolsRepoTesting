using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepoPilotUpdater
{
    class Program
    {
        const string BuildToolsPackageName = "Microsoft.DotNet.BuildTools";
        static void Main(string[] args)
        {
            var packages = ProcessPackageList(File.ReadAllLines(args[0]));
            var searchDir = args[1];
            var localPackageDir = args[2];
            foreach (var f in Directory.EnumerateFiles(searchDir, "BuildToolsVersion.txt", SearchOption.AllDirectories))
            {
                WriteLine($"Updating {f}...");
                File.WriteAllText(f, packages[BuildToolsPackageName]);
            }
            foreach (var f in Directory.EnumerateFiles(searchDir, ".toolversions", SearchOption.AllDirectories))
            {
                WriteLine($"Updating {f}...");
                var lines = File.ReadAllLines(f);
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    var split = line.Split('=');
                    var version = "invalid";
                    if (!packages.TryGetValue(split[0], out version))
                    {
                        version = split[1];
                    }
                    sb.AppendLine($"{split[0]}={version}");
                }
                File.WriteAllText(f, sb.ToString());
            }
            foreach (var f in Directory.EnumerateFiles(searchDir, "*.config", SearchOption.AllDirectories))
            {
                var changedAny = false;
                var sb = new StringBuilder();
                foreach (var line in File.ReadAllLines(f))
                {
                    bool done = false;
                    if (line.Contains("https://dotnet.myget.org/F/dotnet-buildtools"))
                    {
                        changedAny = true;
                        sb.AppendLine($"<add key=\"Local BuildTools\" value=\"{localPackageDir}\" />");
                        sb.AppendLine(line);
                        done = true;
                    }
                    foreach (var k in packages.Keys)
                    {
                        if (line.Contains($"\"{k}\""))
                        {
                            changedAny = true;
                            sb.AppendLine($"<package id=\"{k}\" version=\"{packages[k]}\" />");
                            done = true;
                        }
                    }
                    if (!done)
                    {
                        sb.AppendLine(line);
                    }
                }
                if (changedAny)
                {
                    WriteLine($"Updating {f}...");
                    File.WriteAllText(f, sb.ToString());
                }
            }
        }

        private static Dictionary<string, string> ProcessPackageList(string[] lines)
        {
            var dict = new Dictionary<string, string>();
            foreach (var package in lines)
            {
                var name = package.Replace(".nupkg", "");
                var split = name.Split('.');
                int temp;
                int idx = -1;
                for (int i = 0; i < split.Length; ++i)
                {
                    if (int.TryParse(split[i], out temp))
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx < 0)
                {
                    throw new Exception();
                }
                dict.Add(string.Join(".", split.Take(idx)), string.Join(".", split.Skip(idx)));
            }
            return dict;
        }

        static void WriteLine(string text)
        {
            Debug.WriteLine(text);
            Console.WriteLine(text);
        }
    }
}
