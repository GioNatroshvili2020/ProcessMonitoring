using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessKillerTests
{
    public  class FunctionalityTests
    {
        NLog.Logger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = NLog.LogManager.GetLogger("testLogger");//In Production application it will be created using dependency injection
        }

        [Test]
        public void Should_KillNotepadProcess_In_1minute()
        {
            Process.Start("Notepad");//starting notepad
            var processKiller = new ProcessKiller("Notepad", 1, 1, _logger);
        
            Task.Run(() => processKiller.MonitorProcess());
            Thread.Sleep(70000); //wait ~1 minute(little longer then lifetime to  compensate for program runtime)

            Process[] proc = Process.GetProcessesByName("Notepad");

            bool isNotepadRunning= !(proc.Length == 0 || proc == null);
            Assert.That(isNotepadRunning, Is.EqualTo(false));
        }

        [Test]
        public void Should_Keeps_Monitoring_When_Process_Killed()
        {
            Process.Start("Notepad");//starting notepad
            var processKiller = new ProcessKiller("Notepad", 1, 1, _logger);
            
            Task.Run(() => processKiller.MonitorProcess());
            Thread.Sleep(70000); //wait ~1 minute(little longer than given lifetime to  compensate for program runtime delay)

            Process[] proc = Process.GetProcessesByName("Notepad");

            bool isNotepadRunning = !(proc.Length == 0 || proc == null);
            Assert.That(isNotepadRunning, Is.EqualTo(false));// check if process got killed 

            //start notepad again
            Process.Start("Notepad");
            Thread.Sleep(120000); //wait ~2 minute(we don't know at what exact moment was last monitoring check)

            proc = Process.GetProcessesByName("Notepad");
            isNotepadRunning = !(proc.Length == 0 || proc == null);
            Assert.That(isNotepadRunning, Is.EqualTo(false));// check if process got killed  

        }
        [Test]
        public void Assert_Keeps_Monitoring_When_Process_Not_Found_First()
        {
            Process.Start("Notepad");//starting notepad
            var processKiller = new ProcessKiller("Notepad", 1, 1, _logger);

            Task.Run(() => processKiller.MonitorProcess());
            Thread.Sleep(2000); //wait few seconds for process monitor to see the process 
            //kill the process before process monitoring does it 
            Process[] proc = Process.GetProcessesByName("notepad");

            foreach(var process in proc)//kill all notepad processes
            {
                process.Kill();

            }

            //start process again 
            Process.Start("Notepad");//starting notepad        
            Thread.Sleep(120000); //wait ~2 minute(we don't know at what exact moment was last monitoring check)

            proc = Process.GetProcessesByName("Notepad");
            bool isNotepadRunning = !(proc.Length == 0 || proc == null);
            Assert.That(isNotepadRunning, Is.EqualTo(false));// check if process got killed
        }

    }
}
