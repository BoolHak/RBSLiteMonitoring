using RBSLiteMonitoring.Models;
using static System.Double;

namespace RBSLiteMonitoring.Collectors
{
    public class HardwearCollector
    {
        public static async Task<List<DiskData>?> GetDisks()
        {
            using var process = new GummyProcess("df", "-h -t ext4");
            var messages = new List<string>();
            process.WhenLog(m => {
                if (m.Contains("log-begin")) return;
                if (m.Contains("Filesystem")) return;
                messages.Add(m);
            });
            var count = 0;
            process.Start();

            while (count < 20)
            {
                count++;
                await Task.Delay(100);
                if (!process.IsStarted()) break;
            }

            var disks = new List<DiskData>();

            foreach (var line in messages)
            {
                var tabs = line.Split(" ").Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();
                if (tabs.Length < 6) continue;
                if (tabs[1].Contains('M')) continue;
                var diskData = new DiskData
                {
                    FileSystem = tabs[0],
                    Size = tabs[1],
                    Used = tabs[2],
                    Available = tabs[3],
                    Use = tabs[4],
                    MontedOn = tabs[5]
                };
                disks.Add(diskData);

            }

            return disks;
        }

        public static async Task<CpuData?> GetCpuData()
        {
            using var process = new GummyProcess("mpstat", "1 1");
            var message = "";
            process.WhenLog(m => {
                if (m.Contains("all"))
                {
                    message = m;
                }
            });
            var count = 0;
            process.Start();

            while (count < 20)
            {
                count++;
                await Task.Delay(100);
                if (!process.IsStarted() || !string.IsNullOrEmpty(message)) break;
            }

            if (string.IsNullOrWhiteSpace(message)) return null;
            var tabs = message.Split(" ").Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] == "all")
                {

                    _ = TryParse(tabs[i + 1], out var user);
                    _ = TryParse(tabs[i + 2], out var nice);
                    _ = TryParse(tabs[i + 3], out var sys);
                    _ = TryParse(tabs[i + 4], out var iowait);
                    _ = TryParse(tabs[i + 5], out var irq);
                    _ = TryParse(tabs[i + 6], out var soft);
                    _ = TryParse(tabs[i + 7], out var steal);
                    _ = TryParse(tabs[i + 8], out var guest);
                    _ = TryParse(tabs[i + 9], out var gnice);
                    _ = TryParse(tabs[i + 10], out var idle);

                    return new CpuData
                    {
                        User = user,
                        Nice = nice,
                        Sys = sys,
                        IoWait = iowait,
                        IRQ = irq,
                        Soft = soft,
                        Steal = steal,
                        Guest = guest,
                        Gnice = gnice,
                        Idle = idle
                    };
                }
            }
            return null;
        }

        public static async Task<RamData?> GetRamData()
        {
            using var process = new GummyProcess("vmstat", "-s");
            long total = 0;
            long used = 0;
            long cache = 0;
            long swap = 0;
            long boot = 0;

            process.WhenLog(m =>
            {
                if (m.Contains("total memory"))
                {
                    var value = m.Split(" ").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (value != null) _ = long.TryParse(value, out total);
                }
                if (m.Contains("used memory"))
                {
                    var value = m.Split(" ").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (value != null) _ = long.TryParse(value, out used);
                }

                if (m.Contains("swap cache"))
                {
                    var value = m.Split(" ").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (value != null) _ = long.TryParse(value, out cache);
                }

                if (m.Contains("used swap"))
                {
                    var value = m.Split(" ").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (value != null) _ = long.TryParse(value, out swap);
                }

                if (m.Contains("boot time"))
                {
                    var value = m.Split(" ").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (value != null) _ = long.TryParse(value, out boot);
                }

            });

            var count = 0;
            process.Start();

            while (count < 20)
            {
                count++;
                await Task.Delay(100);
                if (!process.IsStarted()) break;
            }

            return new RamData
            {
                Total = total,
                Used = used,
                Cache = cache,
                Swap = swap,
                Boot = boot
            };
        }
    }
}
