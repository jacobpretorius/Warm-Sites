using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Warm.Sites.Core.Models;

namespace Warm.Sites.Core
{
    public static class Program
    {
        private static readonly string _targetsFile = "targets.json";
        private static Targets Targets;

        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Starting Warm Sites v1.0 - 2021 (Made by Jacob)");

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 2, 0);

                while (true)
                {
                    // Read targets
                    ReadTargetsFile();

                    if (Targets?.TargetUrls?.Any() != true)
                    {
                        // Sleep for 5 mins and retry
                        Thread.Sleep(300000);
                        continue;
                    }

                    foreach (var url in Targets.TargetUrls)
                    {
                        Console.WriteLine($"  {DateTime.Now.ToShortTimeString()} checking: {url}");

                        // Visit url
                        try
                        {
                            var result = await client.GetAsync(url);
                            if (result.IsSuccessStatusCode)
                            {
                                result.Headers.TryGetValues("x-backend-server", out var serverHeader);

                                Console.WriteLine($"    Up - {serverHeader.First()}");
                            }
                            else
                            {
                                Console.WriteLine($"    ?? - Timed out or server error");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("    xx - Could not reach server, host mapped?");
                        }

                        Thread.Sleep(100);
                    }

                    Console.WriteLine($"-- {DateTime.Now.ToShortTimeString()} all done for now.");
                    // Sleep for 5 mins
                    Thread.Sleep(300000);
                }
            }
        }

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }

        private static void ReadTargetsFile()
        {
            // Read/create targets file
            var localPath = Path.Combine(GetBasePath(), _targetsFile);
            if (File.Exists(localPath))
            {
                try
                {
                    // Read file
                    FileStream fileStream = new FileStream(localPath, FileMode.Open);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        var fileAsString = reader.ReadToEnd();
                        Targets = JsonSerializer.Deserialize<Targets>(fileAsString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading the targets file, is it open in other program?");
                }
            }
            else
            {
                // Make the targets file
                using (var fileStream = System.IO.File.Create(localPath))
                {
                    using (var fileWriter = new System.IO.StreamWriter(fileStream))
                    {
                        Console.WriteLine("Making the json file, restart once you have added links");
                        fileWriter.Write(JsonSerializer.Serialize(new Targets()));
                    }
                }

                throw new Exception("Please edit 'targets.json' and restart the program.");
            }
        }
    }
}