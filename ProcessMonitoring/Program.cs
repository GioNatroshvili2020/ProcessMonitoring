
using ProcessMonitoring.ProcessKiller;

var logger = NLog.LogManager.GetLogger("processKillerLogger");//In Production application it will be created using dependency injection

ProcessKiller processKiller = new ProcessKiller("Notepad", 1, 1, logger );
processKiller.StartMonitoring();