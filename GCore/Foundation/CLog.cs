using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace GCore
{
    public sealed class CLog : IDisposable
    {
        private CFile[] m_aLogFile = new CFile[3];
        private bool m_bDisposed;
        private bool m_bLogTime = true;
        private bool m_bLogToFile = true;
        private bool m_bMarkLogLevel = true;
        private bool m_bMultiThreadStdOut;
        private bool m_bMultiThreadStdOutRunning;
        private bool m_bNewLine = true;
        private bool m_bOutputDebugString = true;
        private bool m_bShowToConsole = true;
        private bool m_bStdOutEnabled = true;
        private ManualResetEvent m_EventMultithreadStdOut = new ManualResetEvent(false);
        private CustomLogFunction m_fnCustomLogMsg;
        private int[] m_iaCurLogFileCount = new int[3];
        private int m_iFileMode = 0x109;
        private int m_iMaxLogFileRotation = -1;
        private int m_iMaxLogFileSize = -1;
        private LinkedList<CStdOutLog> m_listStdOutLog = new LinkedList<CStdOutLog>();
        private object m_LockObject = new object();
        private object m_LockObjectForStdOutLogList = new object();
        private string m_sLogFileExt = ".log";
        private string m_sLogFilename = "default";
        private string m_sLogPath = "/logs/";
        private static string[] s_sLogLevelTags = new string[] { "", "_wr", "_er" };

        public CLog()
        {
            if (Environment.OSVersion.Platform.ToString() == "Win32NT")
            {
                this.m_sLogPath = Directory.GetCurrentDirectory();
                this.m_sLogPath = this.m_sLogPath + "/logs/";
            }
        }

        private void _CheckStdOutDisableFlag()
        {
            if ((this.m_bShowToConsole || this.m_bOutputDebugString) || this.m_bLogToFile)
            {
                this.m_bStdOutEnabled = true;
            }
            else
            {
                this.m_bStdOutEnabled = false;
            }
        }

        private void _CloseLogFiles()
        {
            if (this.m_bMultiThreadStdOutRunning)
            {
                this.m_bMultiThreadStdOut = false;
                lock (this.m_LockObjectForStdOutLogList)
                {
                    lock (this.m_LockObject)
                    {
                        this._RunStdOutLog(null);
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if (this.m_aLogFile[i] != null)
                {
                    this.m_aLogFile[i].Close(true);
                    this.m_aLogFile[i] = null;
                }
            }
        }

        private void _LogOut(ELevel eLevel, bool bLogTime, bool bNewLine, bool bMarkLogLevel, string format, params object[] args)
        {
            string sContext = string.Format(format, args);
            string sTime = null;
            if (bLogTime)
            {
                sTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (this.m_fnCustomLogMsg != null)
                {
                    this.m_fnCustomLogMsg(eLevel, sTime, sContext);
                }
            }
            else if (this.m_fnCustomLogMsg != null)
            {
                this.m_fnCustomLogMsg(eLevel, "", sContext);
            }
            if (this.m_bStdOutEnabled)
            {
                StringBuilder builder = new StringBuilder(0x40);
                if (bMarkLogLevel)
                {
                    if (eLevel == ELevel.ERROR)
                    {
                        builder.Append("!!");
                    }
                    else if (eLevel == ELevel.WARNING)
                    {
                        builder.Append("! ");
                    }
                    else
                    {
                        builder.Append("  ");
                    }
                }
                if (bLogTime)
                {
                    builder.Append(sTime);
                    builder.Append("  ");
                }
                builder.Append(sContext);
                if (bNewLine)
                {
                    builder.Append("\r\n");
                }
                if (this.m_bMultiThreadStdOut)
                {
                    this._MultithreadStdOut(eLevel, builder.ToString());
                }
                else
                {
                    this._StdOut(this.m_bShowToConsole, this.m_bLogToFile, this.m_bOutputDebugString, eLevel, builder.ToString());
                }
            }
        }

        private void _LogToFile(ELevel eLevel, string s)
        {
            for (int i = 0; i < 3; i++)
            {
                if ((int)eLevel < i)
                {
                    break;
                }
                if (this.m_aLogFile[i] == null)
                {
                    this.m_aLogFile[i] = new CFile();
                }
                if ((this.m_iMaxLogFileSize > 0) && (this.m_aLogFile[i].FileSize() > this.m_iMaxLogFileSize))
                {
                    this.m_aLogFile[i].Close(true);
                    this.m_aLogFile[i] = new CFile();
                }
                if (!this.m_aLogFile[i].IsOpened())
                {
                    string sFile = "";
                    do
                    {
                        sFile = this.m_sLogPath + this.m_sLogFilename + s_sLogLevelTags[i];
                        if (this.m_iMaxLogFileSize > 0)
                        {
                            sFile = sFile + "_" + this.m_iaCurLogFileCount[i].ToString("D4");
                            this.m_iaCurLogFileCount[i]++;
                            if ((this.m_iMaxLogFileRotation > 0) && (this.m_iaCurLogFileCount[i] > this.m_iMaxLogFileRotation))
                            {
                                this.m_iaCurLogFileCount[i] = 0;
                            }
                        }
                        sFile = sFile + this.m_sLogFileExt;
                    }
                    while ((CFile.IsFileExist(sFile) && (this.m_iMaxLogFileRotation <= 0)) && (this.m_iMaxLogFileSize > 0));
                    Debug.Assert(this.m_aLogFile[i].Open(sFile, this.m_iFileMode, true), "Log File: " + sFile + " create failed!");
                }
                this.m_aLogFile[i].Write(s);
                if (i != 0)
                {
                    this.m_aLogFile[i].Flush();
                }
                if (i == 1)
                {
                    this.m_aLogFile[0].Flush();
                }
            }
        }

        private void _MultithreadStdOut(ELevel eLevel, string s)
        {
            lock (this.m_LockObjectForStdOutLogList)
            {
                CStdOutLog log = new CStdOutLog(this.m_bShowToConsole, this.m_bLogToFile, this.m_bOutputDebugString, eLevel, s);
                this.m_listStdOutLog.AddLast(log);
                if (this.m_listStdOutLog.Count == 1)
                {
                    this.m_EventMultithreadStdOut.Set();
                }
            }
        }

        private void _OutputDebugString(ELevel eLevel, string s)
        {
        }

        private void _RunStdOutLog(object context)
        {
            CStdOutLog log;
        Label_0000:
            log = null;
            lock (this.m_LockObjectForStdOutLogList)
            {
                if (this.m_listStdOutLog.Count > 0)
                {
                    log = this.m_listStdOutLog.First.Value;
                    this.m_listStdOutLog.RemoveFirst();
                }
            }
            if (log != null)
            {
                this._StdOut(log.m_bShowToConsole, log.m_bLogToFile, log.m_bOutputDebugString, log.m_eLevel, log.m_sLogString);
                goto Label_0000;
            }
            if (!this.m_bMultiThreadStdOut)
            {
                this.m_bMultiThreadStdOutRunning = false;
            }
            else
            {
                this.m_EventMultithreadStdOut.WaitOne();
                this.m_EventMultithreadStdOut.Reset();
                goto Label_0000;
            }
        }

        private void _ShowToConsole(ELevel eLevel, string s)
        {
            if (eLevel > ELevel.NORMAL)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                if (eLevel == ELevel.ERROR)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.Write(s);
                Console.ForegroundColor = foregroundColor;
            }
            else
            {
                Console.Write(s);
            }
        }

        private void _StdOut(bool bShowToConsole, bool bLogToFile, bool bOutputDebugString, ELevel eLevel, string s)
        {
            lock (this.m_LockObject)
            {
                if (bShowToConsole)
                {
                    this._ShowToConsole(eLevel, s);
                }
                if (bOutputDebugString)
                {
                    this._OutputDebugString(eLevel, s);
                }
                if (bLogToFile)
                {
                    this._LogToFile(eLevel, s);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool bDisposing)
        {
            if (!this.m_bDisposed)
            {
                this._CloseLogFiles();
                this.m_bDisposed = true;
            }
        }

        public bool EnableMultithread()
        {
            return this.m_bMultiThreadStdOut;
        }

        public void EnableMultithread(bool bEnable)
        {
            this.m_bMultiThreadStdOut = bEnable;
            if (bEnable && !this.m_bMultiThreadStdOutRunning)
            {
                this.m_bMultiThreadStdOutRunning = true;
                WaitCallback callBack = new WaitCallback(this._RunStdOutLog);
                ThreadPool.QueueUserWorkItem(callBack, this);
            }
        }

        public string FileExt()
        {
            return this.m_sLogFileExt;
        }

        public string Filename()
        {
            return this.m_sLogFilename;
        }

        public void Filename(string sFile, int iFileMode = 0x109)
        {
            string sFilename = CPath.DriverDirName(sFile);
            string sFileExt = CPath.Ext(sFile);
            this.Filename(sFilename, sFileExt, iFileMode);
        }

        public void Filename(string sFilename, string sFileExt, int iFileMode = 0x109)
        {
            if (sFilename == null)
            {
                sFilename = this.m_sLogFilename;
            }
            if (sFileExt == null)
            {
                sFileExt = this.m_sLogFileExt;
            }
            this.m_iFileMode = iFileMode;
            if ((this.m_sLogFilename != sFilename) || (this.m_sLogFileExt != sFileExt))
            {
                this.m_sLogFilename = sFilename;
                this.m_sLogFileExt = sFileExt;
                this._CloseLogFiles();
            }
        }

        ~CLog()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            for (int i = 0; i < 3; i++)
            {
                if (this.m_aLogFile[i] != null)
                {
                    this.m_aLogFile[i].Flush();
                }
            }
        }

        public void LogErrorMsg(string format, params object[] args)
        {
            this._LogOut(ELevel.ERROR, this.m_bLogTime, this.m_bNewLine, this.m_bMarkLogLevel, format, args);
        }

        public void LogMsg(string format, params object[] args)
        {
            this._LogOut(ELevel.NORMAL, this.m_bLogTime, this.m_bNewLine, this.m_bMarkLogLevel, format, args);
        }

        public void LogMsg(ELevel eLevel, string format, params object[] args)
        {
            this._LogOut(eLevel, this.m_bLogTime, this.m_bNewLine, this.m_bMarkLogLevel, format, args);
        }

        public string LogPath()
        {
            return this.m_sLogPath;
        }

        public void LogPath(string sPath)
        {
            this.m_sLogPath = sPath;
            CPath.AddRightSlash(ref this.m_sLogPath, '/');
        }

        public bool LogTime()
        {
            return this.m_bLogTime;
        }

        public void LogTime(bool bEnable)
        {
            this.m_bLogTime = bEnable;
        }

        public bool LogToFile()
        {
            return this.m_bLogToFile;
        }

        public void LogToFile(bool bEnable)
        {
            this.m_bLogToFile = bEnable;
            this._CheckStdOutDisableFlag();
        }

        public void LogWarningMsg(string format, params object[] args)
        {
            this._LogOut(ELevel.WARNING, this.m_bLogTime, this.m_bNewLine, this.m_bMarkLogLevel, format, args);
        }

        public bool MarkLogLevel()
        {
            return this.m_bMarkLogLevel;
        }

        public void MarkLogLevel(bool bEnable)
        {
            this.m_bMarkLogLevel = bEnable;
        }

        public int MaxLogFileRotation()
        {
            return this.m_iMaxLogFileRotation;
        }

        public void MaxLogFileRotation(int iMaxFileRotation)
        {
            this.m_iMaxLogFileRotation = iMaxFileRotation;
        }

        public int MaxLogFileSize()
        {
            return this.m_iMaxLogFileSize;
        }

        public void MaxLogFileSize(int iMaxFileSize)
        {
            this.m_iMaxLogFileSize = iMaxFileSize;
        }

        public bool NewLine()
        {
            return this.m_bNewLine;
        }

        public void NewLine(bool bEnable)
        {
            this.m_bNewLine = bEnable;
        }

        public int NumQueuingLogTasks()
        {
            return this.m_listStdOutLog.Count;
        }

        public bool OutputDebugString()
        {
            return this.m_bOutputDebugString;
        }

        public void OutputDebugString(bool bEnable)
        {
            this.m_bOutputDebugString = bEnable;
            this._CheckStdOutDisableFlag();
        }

        public void SetCustomLogFunction(CustomLogFunction fn)
        {
            this.m_fnCustomLogMsg = fn;
        }

        public bool ShowToConsole()
        {
            return this.m_bShowToConsole;
        }

        public void ShowToConsole(bool bEnable)
        {
            this.m_bShowToConsole = bEnable;
            this._CheckStdOutDisableFlag();
        }

        private class CStdOutLog
        {
            public bool m_bLogToFile;
            public bool m_bOutputDebugString;
            public bool m_bShowToConsole;
            public CLog.ELevel m_eLevel;
            public string m_sLogString;

            public CStdOutLog(bool bShowToConsole, bool bLogToFile, bool bOutputDebugString, CLog.ELevel eLevel, string s)
            {
                this.m_sLogString = s;
                this.m_eLevel = eLevel;
                this.m_bShowToConsole = bShowToConsole;
                this.m_bLogToFile = bLogToFile;
                this.m_bOutputDebugString = bOutputDebugString;
            }
        }

        public delegate void CustomLogFunction(CLog.ELevel eLevel, string sTime, string sContent);

        public enum ELevel
        {
            ERROR = 2,
            NONE = -1,
            NORMAL = 0,
            NUM_LEVELS = 3,
            WARNING = 1
        }
    }
}
