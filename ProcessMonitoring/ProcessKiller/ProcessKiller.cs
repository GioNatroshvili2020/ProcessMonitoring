using System.Diagnostics;
using System.Management;

namespace ProcessMonitoring.ProcessKiller
{
    public class ProcessKiller
    {
        private string _processName;
        private int _lifeTime;
        private DateTime _processStartTime;
        private int _frequency;
        private DateTime? _processLastCheckDate;
        private bool _stopApplication;
        private object _lock;
        private Thread _processMonitoringThread;
        private NLog.Logger _logger;

        public ProcessKiller(string processName, int lifeTime, int frequency, NLog.Logger logger)
        {
            _processName = processName;
            _lifeTime = lifeTime;
            _frequency = frequency;
            _lock = new object();
            _processLastCheckDate = null;
            _processMonitoringThread = new Thread(StartMonitoring) { IsBackground = true, Name = "monitoringThread" };
            _logger = logger;
        }

        private void StartMonitoring()
        {
            lock (_lock)
            {
                while (!_stopApplication)
                {
                    _logger.Info($"Checking status of process named {_processName}");
                    if (IsProcessRunning())
                    {

                        _logger.Info($"Process named {_processName} is Active");

                        if (_processLastCheckDate == null)
                            _processStartTime = DateTime.Now;

                        _processLastCheckDate = DateTime.Now;

                        TimeSpan ts = _processLastCheckDate.Value- _processStartTime;
                        if (ts.TotalMinutes > _lifeTime)
                        {
                            _logger.Info($"Process named {_processName} has exceeded lifetime. attempting to stop process...");
                            try
                            {
                                KillProcess();
                                _processLastCheckDate = null;

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("An Error Occured, Process could not be terminated, Application will now stop. Error Details: \n " + ex.Message);
                                _logger.Fatal("An Error Occured, Process could not be terminated, Application will now stop. Error Details and stacktrace: \n " + ex.Message+ "\n"+ex.StackTrace);
                                break;
                            }

                        }
                    }
                    _logger.Info($"Monitoring continues after {_frequency} minute delay");
                    Thread.Sleep(_frequency * 60000);
                }
            }
        }
        public void MonitorProcess()
        {
            Console.WriteLine($"Beigning monitoring process named {_processName}, Press 'Q' to stop monitoring");
            _logger.Info($"Beigning monitoring process named {_processName}");

            _processMonitoringThread.Start();
            while (Console.ReadKey(true).Key != ConsoleKey.Q){}
            _logger.Info($"Stopping monitoring(User Requested)");

            _stopApplication = true;          
        }
      
        private bool IsProcessRunning()
        {
            Process[] proc = Process.GetProcessesByName(_processName);
            return !(proc.Length == 0 && proc == null);
        }
        private void KillProcess()
        {

            Process[] proc = Process.GetProcessesByName(_processName);            
            KillProcessAndChildrens(proc.First().Id);       
        }
        private  void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();
            
            _logger.Info($"Stopping Child Processes of {_processName}");

            // We must kill child processes first
            if (processCollection != null)
            {

                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
            _logger.Info($"Sucessfully stopped Child Processes of {_processName}, stopping parent processes");

            // Then kill parents.
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                _logger.Info("Process was exited during while before stopping.");
            }
            _logger.Info($"Sucessfully stopped process named: {_processName}");
        }
    }
}
