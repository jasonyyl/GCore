using System;
using System.IO;
using System.Text;

namespace GCore.Foundation
{
    public sealed class CPath
    {
        private char m_cDirectorySeparator;
        private static CPath m_GlobalPath = new CPath("", true, '/');
        private string m_sDir;
        private string m_sDriver;
        private string m_sExt;
        private string m_sName;

        public CPath(string s, bool bIsFile = true, char cDirSeparator = '/')
        {
            if (cDirSeparator == '\0')
            {
                this.m_cDirectorySeparator = Path.DirectorySeparatorChar;
            }
            else
            {
                this.m_cDirectorySeparator = cDirSeparator;
            }
            this.Reset(s, bIsFile);
        }

        public CPath(string sDriver, string sPath, string sName, string sExt, char cDirSeparator = '/')
        {
            if (cDirSeparator == '\0')
            {
                this.m_cDirectorySeparator = Path.DirectorySeparatorChar;
            }
            else
            {
                this.m_cDirectorySeparator = cDirSeparator;
            }
            this.Reset(sDriver, sPath, sName, sExt);
        }

        public static void AddRightSlash(ref string sDir, char cDirSeparator = '/')
        {
            int num = sDir.Length - 1;
            if (num >= 0)
            {
                if (cDirSeparator == '\0')
                {
                    cDirSeparator = Path.DirectorySeparatorChar;
                }
                if ((sDir[num] != '/') && (sDir[num] != '\\'))
                {
                    sDir = sDir + cDirSeparator;
                }
            }
        }

        public void AssignFrom(CPath rhs)
        {
            this.m_sDriver = rhs.m_sDriver;
            this.m_sDir = rhs.m_sDir;
            this.m_sName = rhs.m_sName;
            this.m_sExt = rhs.m_sExt;
        }

        public static bool CopyDir(string sSrcDir, string sDestDir, bool bCopySubDirs = true, bool bOverwrite = true, bool bSupressErrMsg = true)
        {
            AddRightSlash(ref sSrcDir, '/');
            AddRightSlash(ref sDestDir, '/');
            DirectoryInfo info = new DirectoryInfo(sSrcDir);
            if (!info.Exists)
            {
                if (!bSupressErrMsg)
                {
                    Log.LogErrorMsg("CPath::CopyDir() error: Source directory does not exist or could not be found: " + sSrcDir);
                }
                return false;
            }
            if (!Directory.Exists(sDestDir))
            {
                Directory.CreateDirectory(sDestDir);
            }
            DirectoryInfo[] directories = info.GetDirectories();
            foreach (FileInfo info2 in info.GetFiles())
            {
                string destFileName = Path.Combine(sDestDir, info2.Name);
                info2.CopyTo(destFileName, bOverwrite);
            }
            if (bCopySubDirs)
            {
                foreach (DirectoryInfo info3 in directories)
                {
                    string str2 = Path.Combine(sDestDir, info3.Name);
                    CopyDir(info3.FullName, str2, bCopySubDirs, bOverwrite, bSupressErrMsg);
                }
            }
            return true;
        }

        public static bool CreateDir(string sDir, bool bSupressErrMsg)
        {
            if (sDir == null)
            {
                return false;
            }
            try
            {
                return (Directory.CreateDirectory(sDir) != null);
            }
            catch (Exception exception)
            {
                if (!bSupressErrMsg)
                {
                    Log.LogErrorMsg("CreateDir Exception: " + exception.ToString());
                }
                return false;
            }
        }

        public static bool DeleteDir(string sDir, bool bDelFilesIfExist = true, bool bDelSubDirsIfExist = true, bool bDelHidden = true, bool bDelSelfDir = true, bool bSupressErrMsg = true)
        {
            if (Directory.Exists(sDir))
            {
                Directory.Delete(sDir, bDelSubDirsIfExist);
            }
            return true;
        }

        public string Dir()
        {
            return this.m_sDir;
        }

        public static string Dir(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.Dir();
        }

        public string Driver()
        {
            return this.m_sDriver;
        }

        public static string Driver(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.Driver();
        }

        public string DriverDir(bool bWithEndSeparatorSign = true)
        {
            if (this.m_sDriver.Length == 0)
            {
                if (this.m_sDir.Length == 0)
                {
                    return "";
                }
                if (bWithEndSeparatorSign)
                {
                    return this.m_sDir;
                }
                return this.m_sDir.Substring(0, this.m_sDir.Length - 1);
            }
            if (this.m_sDir.Length == 0)
            {
                string sDriver = this.m_sDriver;
                if (bWithEndSeparatorSign)
                {
                    sDriver = sDriver + this.m_cDirectorySeparator;
                }
                return sDriver;
            }
            string str2 = this.m_sDriver + this.m_sDir;
            if (!bWithEndSeparatorSign)
            {
                str2 = str2.Substring(0, str2.Length - 1);
            }
            return str2;
        }

        public static string DriverDir(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.DriverDir(true);
        }

        public string DriverDirName()
        {
            return (this.DriverDir(true) + this.Name());
        }

        public static string DriverDirName(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.DriverDirName();
        }

        public string Ext()
        {
            return this.m_sExt;
        }

        public static string Ext(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.Ext();
        }

        public string Full()
        {
            StringBuilder builder = new StringBuilder((((this.m_sDriver.Length + this.m_sDir.Length) + this.m_sName.Length) + this.m_sExt.Length) + 1);
            builder.Append(this.m_sDriver);
            builder.Append(this.m_sDir);
            builder.Append(this.m_sName);
            builder.Append(this.m_sExt);
            return builder.ToString();
        }

        public static string Full(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.Full();
        }

        public static string GetCurDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static bool IsDirExist(string sDir)
        {
            if (sDir == null)
            {
                return false;
            }
            return Directory.Exists(sDir);
        }

        public bool IsEqual(CPath rhs)
        {
            if (this.m_sDriver != rhs.m_sDriver)
            {
                return false;
            }
            if (this.m_sName != rhs.m_sName)
            {
                return false;
            }
            if (this.m_sExt != rhs.m_sExt)
            {
                return false;
            }
            if (this.m_sDir != rhs.m_sDir)
            {
                int index = this.m_sDir.IndexOf("../");
                if (index == -1)
                {
                    index = this.m_sDir.IndexOf(@"..\");
                }
                int num2 = rhs.m_sDir.IndexOf("../");
                if (num2 == -1)
                {
                    num2 = rhs.m_sDir.IndexOf(@"..\");
                }
                if (((index == -1) && (num2 == -1)) || ((index != -1) && (num2 != -1)))
                {
                    return false;
                }
                index = this.m_sDir.IndexOf("./");
                if (index == -1)
                {
                    index = this.m_sDir.IndexOf(@".\");
                }
                num2 = rhs.m_sDir.IndexOf("./");
                if (num2 == -1)
                {
                    num2 = rhs.m_sDir.IndexOf(@".\");
                }
                if (((index == -1) && (num2 == -1)) || ((index != -1) && (num2 != -1)))
                {
                    return false;
                }
            }
            return true;
        }

        public string Name()
        {
            return this.m_sName;
        }

        public static string Name(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.Name();
        }

        public string NameExt()
        {
            if (this.m_sExt.Length == 0)
            {
                return this.m_sName;
            }
            return (this.m_sName + this.m_sExt);
        }

        public static string NameExt(string sPath)
        {
            m_GlobalPath.Reset(sPath, true);
            return m_GlobalPath.NameExt();
        }

        public static void RemoveRightSlash(ref string sDir)
        {
            int length = sDir.Length;
            while (true)
            {
                if (length == 0)
                {
                    break;
                }
                int num2 = length - 1;
                if ((sDir[num2] != '/') && (sDir[num2] != '\\'))
                {
                    break;
                }
                length--;
            }
            sDir = sDir.Substring(0, length);
        }

        public static bool RenameDir(string sOrgDir, string sNewDir, bool bSupressErrMsg = true)
        {
            try
            {
                Directory.Move(sOrgDir, sNewDir);
                return true;
            }
            catch (Exception exception)
            {
                if (!bSupressErrMsg)
                {
                    Log.LogErrorMsg("RenameDir Exception: " + exception.ToString());
                }
                return false;
            }
        }

        public void Reset(string s, bool bIsFile = true)
        {
            this.m_sDriver = this.m_sDir = this.m_sName = this.m_sExt = "";
            int startIndex = 0;
            int num2 = s.Length - 1;
            if (num2 >= 0)
            {
                int index = s.IndexOf(':');
                if (index >= 0)
                {
                    this.m_sDriver = s.Substring(0, index + 1);
                    startIndex = index + 1;
                    if (startIndex > num2)
                    {
                        return;
                    }
                }
                if (bIsFile)
                {
                    index = s.LastIndexOf('\\');
                    int num4 = s.LastIndexOf('/');
                    if (index < num4)
                    {
                        index = num4;
                    }
                    if (index >= startIndex)
                    {
                        this.m_sName = s.Substring(index + 1, num2 - index);
                        num2 = index;
                        this.m_sDir = s.Substring(startIndex, (num2 - startIndex) + 1);
                    }
                    else
                    {
                        this.m_sName = s.Substring(startIndex, (num2 - startIndex) + 1);
                    }
                    num2 = this.m_sName.Length - 1;
                    index = this.m_sName.LastIndexOf('.');
                    if (index >= 0)
                    {
                        this.m_sExt = this.m_sName.Substring(index, (num2 - index) + 1);
                        this.m_sName = this.m_sName.Substring(0, index);
                    }
                }
                else
                {
                    this.m_sDir = s.Substring(startIndex, (num2 - startIndex) + 1);
                    if ((s[num2] != '/') && (s[num2] != '\\'))
                    {
                        this.m_sDir = this.m_sDir + this.m_cDirectorySeparator;
                    }
                }
            }
        }

        public void Reset(string sDriver, string sPath, string sName, string sExt)
        {
            this.SetDriver(sDriver);
            this.SetDir(sPath);
            this.SetName(sName);
            this.SetExt(sExt);
        }

        public static void SetCurDirectory(string sDir)
        {
            Directory.SetCurrentDirectory(sDir);
        }

        public void SetDir(string s)
        {
            this.m_sDir = "";
            int length = s.Length;
            if (length != 0)
            {
                int index = s.IndexOf(':');
                if (index < 0)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
                this.m_sDir = s.Substring(index, length - index);
                char ch = s[length - 1];
                if ((ch != '/') && (ch != '\\'))
                {
                    this.m_sDir = this.m_sDir + this.m_cDirectorySeparator;
                }
            }
        }

        public void SetDriver(string s)
        {
            int index = s.IndexOf(':');
            if (index >= 0)
            {
                this.m_sDriver = s.Substring(0, index + 1);
            }
            else
            {
                this.m_sDriver = s;
                if (this.m_sDriver.Length != 0)
                {
                    this.m_sDriver = this.m_sDriver + ":";
                }
            }
        }

        public void SetExt(string s)
        {
            this.m_sExt = "";
            int length = s.Length;
            if (length != 0)
            {
                int startIndex = s.LastIndexOf('.');
                if (startIndex < 0)
                {
                    this.m_sExt = ".";
                    this.m_sExt = this.m_sExt + s.Substring(0, length);
                }
                else
                {
                    this.m_sExt = s.Substring(startIndex, length - startIndex);
                }
            }
        }

        public void SetName(string s)
        {
            this.m_sName = "";
            int length = s.Length;
            if (length != 0)
            {
                int index = s.IndexOf(':');
                if (index < 0)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
                int num3 = s.LastIndexOf('/');
                int num4 = s.LastIndexOf('\\');
                if (num4 > num3)
                {
                    num3 = num4;
                }
                if (num3 >= index)
                {
                    index = num3 + 1;
                }
                int num5 = s.LastIndexOf('.');
                if (num5 < 0)
                {
                    num5 = length - 1;
                }
                else
                {
                    num5--;
                }
                if (num5 >= index)
                {
                    this.m_sName = s.Substring(index, (num5 - index) + 1);
                }
            }
        }
    }
}
