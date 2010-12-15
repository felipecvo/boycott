namespace Boycott.Logging {
    using System.IO;
    using log4net;
    using log4net.Config;
    using log4net.Layout;

    public class FileLogger {
        public ILog logger;

        public FileLogger(string filePath) {
            var path = Directory.GetDirectoryRoot(filePath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(filePath))
                File.Create(filePath);
            
            var appender = new log4net.Appender.FileAppender();
            appender.Layout = new SimpleLayout();
            appender.File = filePath;
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);
            
            logger = LogManager.GetLogger(typeof(FileLogger));
        }

        public void Log(string text) {
            logger.Debug(text);
        }
    }
}
