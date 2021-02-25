﻿using System;
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
            Console.WriteLine("Starting Warm Sites v0.8 - 2021 (Made by Jacob)");

            // Read/create targets file
            var localPath = Path.Combine(GetBasePath(), _targetsFile);
            if (File.Exists(localPath))
            {
                // Read file
                FileStream fileStream = new FileStream(localPath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    var fileAsString = reader.ReadToEnd();
                    Targets = JsonSerializer.Deserialize<Targets>(fileAsString);
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
            }

            if (Targets.TargetUrls.Length > 0)
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 2, 0);

                    while (true)
                    {
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
                                Console.WriteLine("    Something went wrong");
                            }

                            Thread.Sleep(1000);
                        }

                        // Sleep for 5 mins
                        Thread.Sleep(300000);
                    }
                }
            }
        }

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
    }
}