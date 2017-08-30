namespace GCore.Foundation
{
    public static class Log
    {
        static CLog m_Log = new CLog();
        public static CLog GetLog()
        {
            return m_Log;
        }

        public static bool EnableMultithread()
        {
            return m_Log.EnableMultithread();
        }

        public static void EnableMultithread(bool bEnable)
        {
            m_Log.EnableMultithread(bEnable);
        }

        public static string FileExt()
        {
            return m_Log.FileExt();
        }

        public static string Filename()
        {
            return m_Log.Filename();
        }

        public static void Filename(string sFile, int iFileMode = 0x109)
        {
            m_Log.Filename(sFile, iFileMode);
        }

        public static void Filename(string sFilename, string sFileExt, int iFileMode = 0x109)
        {
            m_Log.Filename(sFilename, sFileExt, iFileMode);
        }

        public static void Flush()
        {
            m_Log.Flush();
        }

        public static void LogErrorMsg(string format, params object[] args)
        {
            m_Log.LogErrorMsg(format, args);
        }

        public static void LogMsg(string format, params object[] args)
        {
            m_Log.LogMsg(format, args);
        }

        public static void LogMsg(CLog.ELevel eLevel, string format, params object[] args)
        {
            m_Log.LogMsg(eLevel, format, args);
        }

        public static string LogPath()
        {
            return m_Log.LogPath();
        }

        public static void LogPath(string sPath)
        {
            m_Log.LogPath(sPath);
        }

        public static bool LogTime()
        {
            return m_Log.LogTime();
        }

        public static void LogTime(bool bEnable)
        {
            m_Log.LogTime(bEnable);
        }

        public static bool LogToFile()
        {
            return m_Log.LogToFile();
        }

        public static void LogToFile(bool b)
        {
            m_Log.LogToFile(b);
        }

        public static void LogWarningMsg(string format, params object[] args)
        {
            m_Log.LogWarningMsg(format, args);
        }

        public static bool MarkLogLevel()
        {
            return m_Log.MarkLogLevel();
        }

        public static void MarkLogLevel(bool bEnable)
        {
            m_Log.MarkLogLevel(bEnable);
        }

        public static int MaxLogFileRotation()
        {
            return m_Log.MaxLogFileRotation();
        }

        public static void MaxLogFileRotation(int iMaxFileRotation)
        {
            m_Log.MaxLogFileRotation(iMaxFileRotation);
        }

        public static int MaxLogFileSize()
        {
            return m_Log.MaxLogFileSize();
        }

        public static void MaxLogFileSize(int iMaxFileSize)
        {
            m_Log.MaxLogFileSize(iMaxFileSize);
        }

        public static bool NewLine()
        {
            return m_Log.NewLine();
        }

        public static void NewLine(bool bEnable)
        {
            m_Log.NewLine(bEnable);
        }

        public static int NumQueuingLogTasks()
        {
            return m_Log.NumQueuingLogTasks();
        }

        public static bool OutputDebugString()
        {
            return m_Log.OutputDebugString();
        }

        public static void OutputDebugString(bool b)
        {
            m_Log.OutputDebugString(b);
        }

        public static void SetCustomLogFunction(CLog.CustomLogFunction fn)
        {
            m_Log.SetCustomLogFunction(fn);
        }

        public static bool ShowToConsole()
        {
            return m_Log.ShowToConsole();
        }

        public static void ShowToConsole(bool b)
        {
            m_Log.ShowToConsole(b);
        }
    }
}
