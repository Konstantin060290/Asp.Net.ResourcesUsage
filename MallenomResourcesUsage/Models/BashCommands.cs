using System.Diagnostics;

namespace MallenomResourcesUsage.Models;

public static class BashCommands
{  
        public static string GetDiskSpace()
        {
            return string.Join(" ", "df").Bash();
        }

        public static string GetCpuMetrics()
        {
            return string.Join(" ", "cat /proc/loadavg").Bash();
        }
        private static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
}