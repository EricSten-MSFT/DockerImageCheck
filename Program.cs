using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DockerImageCheck
{
    public enum ErrorStatusCodes
    {
        DockerConfigNotFound = 1,
        DockerConfigDeserializeError,
        DockerConfigInvalid,
        DockerRepositoryDeserializeError,
        DockerRepositoryNotFound,
    }

    class Program
    {
        static readonly string DockerConfigFile = "/etc/docker/daemon.json";

        static int Main(string[] args)
        {
            DockerDaemonConfig dockerConfig;
            DockerRepository dockerRepo = null;
            Stopwatch sw = new Stopwatch();
            int imagesChecked = 0;

            if (!File.Exists(DockerConfigFile))
            {
                Console.Error.WriteLine($"error: {DockerConfigFile} not found.");
                return (int)ErrorStatusCodes.DockerConfigNotFound;
            }

            try
            {
                dockerConfig = JsonConvert.DeserializeObject<DockerDaemonConfig>(File.ReadAllText(DockerConfigFile));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"error: could not deserialize {DockerConfigFile}. Exception: {ex}");
                return (int)ErrorStatusCodes.DockerConfigDeserializeError;
            }

            if (string.IsNullOrEmpty(dockerConfig.Graph))
            {
                Console.Error.WriteLine($"error: Could not find docker images (graph value empty).");
                return (int)ErrorStatusCodes.DockerConfigInvalid;
            }

            Console.WriteLine($"Docker config: graph: {dockerConfig.Graph}, storage-driver: {dockerConfig.StorageDriver}, insecure-registries: {string.Join(',', dockerConfig.InsecureRegistries)}");

            // find the repositories.json file
            if (Directory.Exists(dockerConfig.Graph))
            {
                string repositoryJsonFile = string.Empty;
                foreach (string imageSubDir in Directory.GetDirectories(dockerConfig.Graph))
                {
                    repositoryJsonFile = Path.Combine(imageSubDir, $"image/{dockerConfig.StorageDriver}/repositories.json");
                    if (File.Exists(repositoryJsonFile))
                    {
                        // Deserialize repository json
                        try
                        {
                            dockerRepo = JsonConvert.DeserializeObject<DockerRepository>(File.ReadAllText(repositoryJsonFile));
                            Console.WriteLine($"Successfully deserialized repositories.json file: {repositoryJsonFile}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"error: could not deserialize {repositoryJsonFile}. Exception: {ex}");
                            return (int)ErrorStatusCodes.DockerRepositoryDeserializeError;
                        }
                    }
                }
                if (string.IsNullOrEmpty(repositoryJsonFile) || dockerRepo == null)
                {
                    Console.Error.WriteLine($"error: could not find docker repositories.json file under {dockerConfig.Graph}.");
                    return (int)ErrorStatusCodes.DockerRepositoryNotFound;
                }
            }

            sw.Start();
            bool skip;
            foreach (KeyValuePair<string, Dictionary<string, string>> images in dockerRepo.Repositories)
            {
                skip = false;
                // Check if the appsvc* or custom images are broken.  Filter out any image that start with
                // any of the entries in dockerConfig.InsecureRegistries (as these are duplicates).
                foreach(string insecureRegistry in dockerConfig.InsecureRegistries)
                {
                    if (images.Key.StartsWith(insecureRegistry))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                {
                    continue;
                }

                foreach(KeyValuePair<string, string> image in images.Value)
                {
                    imagesChecked++;
                    int ret = ExecuteProcessIgnoreStdOut("docker", $"save {image.Key}");
                    if (ret != 0)
                    {
                        Console.WriteLine($"*** Found corrupted docker image! image: {image.Key}, hash: {image.Value}");
                    }
                }
            }
            sw.Stop();

            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"Docker image checking complete.  Images checked: {imagesChecked}, Image check elapsed time: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:00}");

            // Bonus Points:
            // Find all the layers in the corrupt docker image, and walk the files in the docker image volume, and get the file info (STAT(2)).
            // For any file where STAT(2) fails, record the file name.
            // Note: for symlinks, don't use stat, use lstat().

            return 0;
        }

        static int ExecuteProcessIgnoreStdOut(string command, string args)
        {
            int ret = 0;
            Stopwatch sw = new Stopwatch();

            var info = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = args,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };

            using (var process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo = info;

                sw.Start();
                process.Start();
                process.StandardOutput.BaseStream.CopyToAsync(Stream.Null);
                process.WaitForExit();
                sw.Stop();

                Console.WriteLine($"command: '{command} {args}' : time: {sw.ElapsedMilliseconds}ms");

                ret = process.ExitCode;
            }
            return ret;
        }
    }
}
