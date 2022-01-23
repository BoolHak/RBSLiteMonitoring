using RBSLiteMonitoring.Models;

namespace RBSLiteMonitoring.Collectors
{
    public class IOCollector : IDisposable
    {
        private GummyProcess? _process;
        private Action<IOData>? _onData;
        public void Start()
        {
            _process = new GummyProcess("dstat", "-nt --disk --noheaders --bits");
            _process.WhenStopped(StoppedEvent);
            _process.WhenLog(m =>
            {
                if (string.IsNullOrWhiteSpace(m)) return;
                if (m.StartsWith("-net")) return;
                if (m.StartsWith("recv")) return;
                if (m.StartsWith("log-begin")) return;

                try
                {
                    if (m.Contains("\u001b[0;0m")) m = m.Replace("\u001b[0;0m", "");
                    var values = m.Split("|");
                    var network = values[0].Trim().Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    var netIn = GetValueInBits(network[0]);
                    var netOut = GetValueInBits(network[1]);
                    var disk = values[2].Trim().Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    var diskRead = GetValueInBits(disk[0]);
                    var diskWrite = GetValueInBits(disk[1]);

                    _onData?.Invoke(new IOData
                    {
                        NetIn = netIn,
                        NetOut = netOut,
                        DiskRead = diskRead,
                        DiskWrite = diskWrite
                    });
                }
                catch
                {
                    // ignored
                }
            });
            _process.Start();
        }

        private async void StoppedEvent()
        {
            await Restart();
        }

        private async Task Restart()
        {
            Stop();
            await Task.Delay(100);
            Start();

        }

        public void Stop()
        {
            _process?.WhenStopped(() => { });
            _process?.Stop();
        }

        public void OnData(Action<IOData> onData)
        {
            _onData = onData;
        }

        private static long GetValueInBits(string value)
        {
            if (value == "0") return 0;
            if (value.EndsWith("b"))
            {
                value = value.Replace("b", "");
                if (long.TryParse(value, out long result))
                {
                    return result;
                }
            }
            if (value.EndsWith("k"))
            {
                value = value.Replace("k", "");
                if (long.TryParse(value, out long result))
                {
                    return result * 1024;
                }
            }

            if (value.EndsWith("M"))
            {
                value = value.Replace("M", "");
                if (long.TryParse(value, out long result))
                {
                    return result * 1024 * 1024;
                }
            }

            if (value.ToLower().EndsWith("G"))
            {
                value = value.Replace("G", "");
                if (long.TryParse(value, out long result))
                {
                    return result * 1024 * 1024 * 1024;
                }
            }

            return 0;
        }

        public void Dispose()
        {
            Stop();
        }

    }
}
