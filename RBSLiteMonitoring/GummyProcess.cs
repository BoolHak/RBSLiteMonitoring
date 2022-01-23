using System.Diagnostics;

namespace RBSLiteMonitoring
{
    public class GummyProcess : IDisposable
    {
        private bool _isStarted;
        private Action<string>? _logHandler;
        private Process? _process;
        private Action? _startedEvent;
        private Action? _stoppedEvent;

        public GummyProcess(string program, string command)
        {
            Program = program;
            Command = command;
        }

        public string Program { get; init; }
        public string Command { get; init; }

        public void Dispose()
        {
            _process?.Dispose();
            _process = null;
        }

        public void Start()
        {
            if (string.IsNullOrWhiteSpace(Program))
            {
                _stoppedEvent?.Invoke();
                return;
            }

            if (_process != null) throw new IOException("Process all ready started!");

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Program,
                    Arguments = Command,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },

                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                var message = e.Data;
                _logHandler?.Invoke(message);
            };

            _process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                var message = e.Data;
                _logHandler?.Invoke(message);
            };

            _process.Exited += async (s, e) =>
            {
                _isStarted = false;
                _stoppedEvent?.Invoke();
            };

            try
            {
                _isStarted = _process.Start();
            }
            catch (Exception exp)
            {
                _logHandler?.Invoke(exp.Message);
                _isStarted = false;
            }

            if (!_isStarted)
            {
                _process.Dispose();
                _stoppedEvent?.Invoke();
                return;
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _logHandler?.Invoke("log-begin");
            _startedEvent?.Invoke();
        }

        public bool IsStarted()
        {
            return _isStarted;
        }

        public void WhenStopped(Action stoppedEvent)
        {
            _stoppedEvent = stoppedEvent;
        }

        public void WhenStarted(Action startedEvent)
        {
            _startedEvent = startedEvent;
        }

        public async Task Stop()
        {
            if (_process != null)
            {
                _process?.CancelErrorRead();
                _process?.CancelOutputRead();
                _process?.Kill(true);
                await _process?.WaitForExitAsync()!;
            }

            _process?.Dispose();
            _process = null;
            _stoppedEvent?.Invoke();
        }

        public void WhenLog(Action<string> logHandler)
        {
            _logHandler = logHandler;
        }

        public int GetId()
        {
            if (_process == null) return -1;
            return _process.Id;
        }

        public string GetFullCommand()
        {
            return $"{Program} {Command}";
        }
    }
}
