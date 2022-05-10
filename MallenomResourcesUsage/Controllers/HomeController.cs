using MallenomResourcesUsage.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace MallenomResourcesUsage.Controllers
{
    public class HomeController : Controller
    {

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        public IActionResult Index()
        {
            Resources res = new Resources();

            if (IsUnix() == true)
            {
                GetCPUunixMetrics(res);
                GetUnixDiskMetrics(res);
                res.TotalRAM = GetUnixMemoryMetrics().Total;
                res.FreeRAM = GetUnixMemoryMetrics().Free;
                res.UsedRAM = GetUnixMemoryMetrics().Used;
                return View(res);
            }
            else
            {
                GetCPUwindowsMetrics(res);
                GetWindowsDisksMetrics(res);
                res.TotalRAM = GetWindowsMemoryMetrics().Total;
                res.FreeRAM = GetWindowsMemoryMetrics().Free;
                res.UsedRAM = GetWindowsMemoryMetrics().Used;
                return View(res);
            }            
        }

        #region CPU
        public void GetCPUwindowsMetrics(Resources res)
        {
            ManagementObjectSearcher man = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
            foreach (ManagementObject obj in man.Get())
            {
                res.CPULoading = obj["LoadPercentage"].ToString();
            }
        }
        
        public void GetCPUunixMetrics(Resources res)
        {
            var output = BashCommands.GetCpuMetrics();
            var cpuloadarr = output.Split(" ");
            cpuloadarr[0] = cpuloadarr[0].Substring(2);
            res.CPULoading = cpuloadarr[0];
        }
        #endregion

        #region Disks
        public void GetWindowsDisksMetrics(Resources r)
        {
            ConnectionOptions options = new ConnectionOptions();
            ManagementScope scope = new ManagementScope("\\\\localhost\\root\\cimv2", options);
            scope.Connect();
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            SelectQuery query1 = new SelectQuery("Select * from Win32_LogicalDisk");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection queryCollection = searcher.Get();
            ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(scope, query1);
            ManagementObjectCollection queryCollection1 = searcher1.Get();

            foreach (ManagementObject mo in queryCollection1)
            {
                // Display Logical Disks information
                string name = mo["Name"].ToString();
                string freespace = (Convert.ToInt64(mo["FreeSpace"]) / 1000000000).ToString();
                string fullsize = (Convert.ToInt64(mo["Size"]) / 1000000000).ToString();
                string usedsize = (Convert.ToDouble(fullsize) - Convert.ToDouble(freespace)).ToString();
                string usedsizeonbar = "width: " + Convert.ToInt64((Convert.ToDouble(fullsize)-Convert.ToDouble(freespace) )/ Convert.ToDouble(fullsize) * 100).ToString() + "%";
                r.Drives.Add(new Drive(name, freespace, fullsize, usedsize, usedsizeonbar));
            }
        }

        public void GetUnixDiskMetrics(Resources r)
        {
            string dfresult = BashCommands.GetDiskSpace();
            string [] dfresultar = dfresult.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i=0; i<dfresultar.Length; i++)
            {
                if (dfresultar[i].Contains("sda"))
                {
                    string name = dfresultar[i];
                    /*name = name.Insert(name.Length - 9, "*");*/
                    name = name.Substring(dfresultar[i].Length - 9);
                    string freespace = Math.Round(Convert.ToDouble(dfresultar[i + 3])/Math.Pow(10, 6), 2).ToString();
                    string fullsize = Math.Round(Convert.ToDouble(dfresultar[i + 1])/Math.Pow(10, 6), 2).ToString();
                    string usedsize = Math.Round(Convert.ToDouble(dfresultar[i + 2])/Math.Pow(10, 6), 2).ToString();
                    string usedsizeonbar = "width: " + Convert.ToInt64((Convert.ToDouble(fullsize) - Convert.ToDouble(freespace)) / Convert.ToDouble(fullsize) * 100).ToString() + "%";
                    r.Drives.Add(new Drive(name, freespace, fullsize, usedsize, usedsizeonbar));
                }
            }
        }

        #endregion

        #region Memory
        public class MemoryMetrics
        {
            public double Total;
            public double Used;
            public double Free;
        }
        private MemoryMetrics GetWindowsMemoryMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private MemoryMetrics GetUnixMemoryMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = double.Parse(memory[1]);
            metrics.Used = double.Parse(memory[2]);
            metrics.Free = double.Parse(memory[3]);

            return metrics;
        }

        #endregion

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Don'tUse
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        #endregion

    }
}