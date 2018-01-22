using System;
using System.IO;
using System.Text;

namespace GCore.Foundation
{
    public sealed class CFile : IDisposable
    {
        public const int DEFAULT_FILE_VERSION = 0x3e8;
        public const int DEFAULT_HEADER_VERSION = 0x3e8;
        public const int FILE_BUF_LEN = 0x80;
        public const int FILE_DEFAULT_ALLOC_SIZE = 0x800;
        public const int FILE_MAX_REALLOC_SIZE = 0x8000;
        public const int FLOAT_SIZE_IN_BYTE = 4;
        public const int INT_SIZE_IN_BYTE = 4;
        public const int LONG_LONG_SIZE_IN_BYTE = 8;
        public const int LONG_SIZE_IN_BYTE = 4;
        private bool m_bDisposed;
        private byte[] m_byCryptSalt = s_byEmptyCryptSalt;
        private byte[] m_byDataForDecompressBuf;
        private byte[] m_byMemoryBuf;
        private int m_iCryptCount;
        private int m_iMemBufIdx;
        private int m_iMode;
        private int m_iTextEncoding;
        private object m_LockObject = new object();
        private uint m_nFileVersion = 0x3e8;
        private uint m_nHeaderVersion = 0x3e8;
        private string m_sFilename = "";
        private static CFileSystem m_theDefaulFileSystem = new CFileSystem();
        private CFileSystem.IFile m_theFileImpl = m_theFileSystem.NewFile();
        private static CFileSystem m_theFileSystem = m_theDefaulFileSystem;
        public const int MODE_ANSI = 0;
        public const int MODE_APPEND = 0x18;
        public const int MODE_BINARY = 2;
        public const int MODE_COMPRESS = 0x60;
        public const int MODE_MEMORY_ACCESS = 0x20;
        public const int MODE_READ = 4;
        public const int MODE_TEXT = 1;
        public const int MODE_UNICODE = 0x200;
        public const int MODE_UNICODE_BE = 0x400;
        public const int MODE_UTF8 = 0x100;
        public const int MODE_WRITE = 8;
        private const int NUM_FILE_VERSION = 4;
        private const int NUM_FILENAME_STR = 0x40;
        private const int NUM_HEADER_VERSION = 4;
        private const int NUM_MAGIC_STR = 8;
        private const int NUM_RESERVED = 0x10;
        private static byte[] s_byEmptyCryptSalt = new byte[0];
        public const int SHORT_SIZE_IN_BYTE = 2;
        public const int TEXT_ENCODING_MASK = 0x700;
        public const int WIDE_CHAR_SIZE_IN_BYTE = 2;

        private bool _Decrypt(byte[] theBuf, int iBufStartIndex, int iSize)
        {
            if (((this.m_byCryptSalt.Length == 0) || (theBuf == null)) || (iSize <= 0))
            {
                return false;
            }
            int length = this.m_byCryptSalt.Length;
            for (int i = 0; i < iSize; i++)
            {
                theBuf[i + iBufStartIndex] = (byte)(theBuf[i + iBufStartIndex] ^ this.m_byCryptSalt[this.m_iCryptCount]);
                this.m_iCryptCount++;
                if (this.m_iCryptCount == length)
                {
                    this.m_iCryptCount = 0;
                }
            }
            return true;
        }

        private bool _DeInitMemoryAccessData()
        {
            lock (this.m_LockObject)
            {
                Debug.Assert(this.IsMemoryAccessMode());
                if ((this.m_iMode & 8) == 8)
                {
                    this.m_iMode &= -33;
                    this.SeekBegin(0L);
                    Debug.Assert(this.Write(this.m_byMemoryBuf, this.m_iMemBufIdx) == this.m_iMemBufIdx);
                    this.SeekBegin((long)this.m_iMemBufIdx);
                    this.m_byMemoryBuf = null;
                    this.m_iMemBufIdx = 0;
                    return true;
                }
                if ((this.m_iMode & 4) == 4)
                {
                    this.m_iMode &= -33;
                    this.SeekBegin((long)this.m_iMemBufIdx);
                    this.m_byMemoryBuf = null;
                    this.m_iMemBufIdx = 0;
                    return true;
                }
            }
            return false;
        }

        private bool _Encrypt(byte[] theBuf, int iBufStartIndex, int iSize)
        {
            if (((this.m_byCryptSalt.Length == 0) || (theBuf == null)) || (iSize <= 0))
            {
                return false;
            }
            int length = this.m_byCryptSalt.Length;
            for (int i = 0; i < iSize; i++)
            {
                theBuf[i + iBufStartIndex] = (byte)(theBuf[i + iBufStartIndex] ^ this.m_byCryptSalt[this.m_iCryptCount]);
                this.m_iCryptCount++;
                this.m_iCryptCount = this.m_iCryptCount % length;
            }
            return true;
        }

        private bool _InitMemoryAccessData()
        {
            lock (this.m_LockObject)
            {
                Debug.Assert(!this.IsMemoryAccessMode());
                if ((this.m_iMode & 8) == 8)
                {
                    long num = this.FileSize();
                    int num2 = Math.Max(0x800, (int)num);
                    this.SeekEnd(0L);
                    this.m_iMemBufIdx = (int)this.Tell();
                    this.SeekBegin(0L);
                    this.m_byMemoryBuf = new byte[num2];
                    if (num != 0L)
                    {
                        Debug.Assert(this.Read(this.m_byMemoryBuf, (int)num) == this.m_iMemBufIdx);
                    }
                    this.m_iMode |= 0x20;
                    return true;
                }
                if ((this.m_iMode & 4) == 4)
                {
                    int iSizeInByte = (int)this.FileSize();
                    this.m_iMemBufIdx = (int)this.Tell();
                    this.SeekBegin(0L);
                    this.m_byMemoryBuf = new byte[iSizeInByte];
                    Debug.Assert(this.Read(this.m_byMemoryBuf, iSizeInByte) == iSizeInByte);
                    this.m_iMode |= 0x20;
                    return true;
                }
            }
            return false;
        }

        private bool _ModeChecking(int iMode)
        {
            if ((((iMode & 1) != 0) && ((iMode & 2) != 0)) || (((iMode & 1) == 0) && ((iMode & 2) == 0)))
            {
                Debug.Assert(false, "MODE_TEXT and MODE_BINARY must be exclusive!");
                return false;
            }
            if (((iMode & 0x18) != 0x18) && ((((iMode & 4) != 0) && ((iMode & 8) != 0)) || (((iMode & 4) == 0) && ((iMode & 8) == 0))))
            {
                Debug.Assert(false, "MODE_READ and MODE_WRITE must be exclusive!");
                return false;
            }
            int num = iMode & 0x700;
            if (((num != 0) && (num != 0x100)) && ((num != 0x200) && (num != 0x400)))
            {
                Debug.Assert(false, "MODE_ANSI, MODE_UTF8, MODE_UNICODE, and MODE_UNICODE_BE must be exclusive!");
                return false;
            }
            return true;
        }

        private int _ReadChar(out char ch)
        {
            if (this.m_iTextEncoding == 0x100)
            {
                byte num;
                int num2 = this.Read(out num);
                if (num2 == 0)
                {
                    ch = '\0';
                    return 0;
                }
                int num3 = 0;
                for (byte i = num; (i & 0x80) != 0; i = (byte)(i << 1))
                {
                    num3++;
                }
                switch (num3)
                {
                case 0:
                case 1:
                    ch = Convert.ToChar(num);
                    return num2;

                case 2:
                    {
                        byte num5;
                        int num6 = this.Read(out num5);
                        if (num6 == 0)
                        {
                            ch = '\0';
                            return 0;
                        }
                        ch = (char)(((0x1f & num) << 6) | (0x3f & num5));
                        return (num2 + num6);
                    }
                case 3:
                    {
                        byte num7;
                        byte num9;
                        int num8 = this.Read(out num7);
                        if (num8 == 0)
                        {
                            ch = '\0';
                            return 0;
                        }
                        int num10 = this.Read(out num9);
                        if (num10 == 0)
                        {
                            ch = '\0';
                            return 0;
                        }
                        ch = (char)((((15 & num) << 12) | ((0x3f & num7) << 6)) | (0x3f & num9));
                        return ((num2 + num8) + num10);
                    }
                }
                ch = (char)0xfffd;
                return 0;
            }
            if (this.m_iTextEncoding == 0)
            {
                byte num11;
                int num12 = this.Read(out num11);
                if (num12 == 0)
                {
                    ch = '\0';
                    return 0;
                }
                ch = Convert.ToChar(num11);
                return num12;
            }
            byte[] byteData = new byte[2];
            int num13 = this.Read(byteData);
            if (num13 == 0)
            {
                ch = '\0';
                return 0;
            }
            if (this.m_iTextEncoding == 0x400)
            {
                ch = (char)(((ushort)(byteData[0] << 8)) | byteData[1]);
                return num13;
            }
            ch = (char)(((ushort)(byteData[1] << 8)) | byteData[0]);
            return num13;
        }

        private int _ReadUnicodeBOM(string sFilename, int iMode)
        {
            int iTextEncoding = 0;
            if ((iMode & 0x18) == 0x18)
            {
                if (IsFileExist(sFilename))
                {
                    int num2 = 5;
                    CFile file = new CFile();
                    if (file.Open(sFilename, num2, true))
                    {
                        iTextEncoding = file.m_iTextEncoding;
                        file.Close(true);
                        file.Dispose();
                    }
                }
            }
            else if (this.IsOpened())
            {
                long lDistanceToMove = this.Tell();
                this.SeekBegin(0L);
                byte[] byteData = new byte[2];
                this.Read(byteData);
                long num4 = this.Tell();
                if ((byteData[0] == 0xff) && (byteData[1] == 0xfe))
                {
                    iTextEncoding = 0x200;
                }
                else if ((byteData[0] == 0xfe) && (byteData[1] == 0xff))
                {
                    iTextEncoding = 0x400;
                }
                else if ((byteData[0] == 0xef) && (byteData[1] == 0xbb))
                {
                    byte num5;
                    this.Read(out num5);
                    num4 = this.Tell();
                    if (num5 == 0xbf)
                    {
                        iTextEncoding = 0x100;
                    }
                }
                if ((iTextEncoding != 0) && (lDistanceToMove < num4))
                {
                    lDistanceToMove = num4;
                }
                this.SeekBegin(lDistanceToMove);
            }
            if (iTextEncoding == 0)
            {
                if ((iMode & 0x100) != 0)
                {
                    return 0x100;
                }
                if ((iMode & 0x200) != 0)
                {
                    return 0x200;
                }
                if ((iMode & 0x400) != 0)
                {
                    iTextEncoding = 0x400;
                }
            }
            return iTextEncoding;
        }

        private void _SkipBlank()
        {
            char ch;
            long lDistanceToMove = this.Tell();
            if (this._ReadChar(out ch) != 0)
            {
                while (!this.IsEnd() && (((ch == ' ') || (ch == '\n')) || ((ch == '\r') || (ch == '\t'))))
                {
                    lDistanceToMove = this.Tell();
                    if (this._ReadChar(out ch) == 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                return;
            }
            this.SeekBegin(lDistanceToMove);
        }

        private int _WriteChar(char ch)
        {
            if ((this.m_iTextEncoding & 0x400) != 0)
            {
                byte[] byteData = new byte[] { (byte)(ch >> 8), (byte)ch };
                return this.Write(byteData);
            }
            if ((this.m_iMode & 0x200) != 0)
            {
                byte[] buffer2 = new byte[] { (byte)ch, (byte)(ch >> 8) };
                return this.Write(buffer2);
            }
            if ((this.m_iMode & 0x100) != 0)
            {
                byte[] theBuf = new byte[3];
                if (ch <= '\x007f')
                {
                    theBuf[0] = (byte)ch;
                    return this.Write(theBuf, 1);
                }
                if (ch <= '߿')
                {
                    theBuf[0] = (byte)('\x00c0' | (ch >> 6));
                    theBuf[1] = (byte)(0x80 | (ch & '?'));
                    return this.Write(theBuf, 2);
                }
                if (ch <= 0xffff)
                {
                    theBuf[0] = (byte)('\x00e0' | (ch >> 12));
                    theBuf[1] = (byte)(0x80 | ((ch >> 6) & '?'));
                    theBuf[2] = (byte)(0x80 | (ch & '?'));
                    return this.Write(theBuf, 3);
                }
                Debug.Assert(false);
                return 0;
            }
            if ((ch < '\0') || (ch > '\x00ff'))
            {
                byte[] bytes = BitConverter.GetBytes(ch);
                return this.Write(bytes);
            }
            byte by = Convert.ToByte(ch);
            return this.Write(by);
        }

        private void _WriteUnicodeBOM(int iTextEncoding)
        {
            if (this.IsOpened())
            {
                long lDistanceToMove = this.Tell();
                long num2 = lDistanceToMove;
                this.SeekBegin(0L);
                if (iTextEncoding != 0)
                {
                    if ((iTextEncoding == 0x200) || (iTextEncoding == 0x400))
                    {
                        this._WriteChar((char)0xfeff);
                        num2 = this.Tell();
                    }
                    else if (iTextEncoding == 0x100)
                    {
                        byte[] byteData = new byte[] { 0xef, 0xbb, 0xbf };
                        this.Write(byteData);
                        num2 = this.Tell();
                    }
                    else
                    {
                        Debug.Assert(false, "You can only specify 1 text encode format!");
                    }
                }
                if (lDistanceToMove > num2)
                {
                    this.SeekBegin(lDistanceToMove);
                }
            }
        }

        public void Close(bool bFlush = true)
        {
            lock (this.m_LockObject)
            {
                if (this.IsMemoryAccessMode())
                {
                    Debug.Assert(this._DeInitMemoryAccessData());
                }
                this.m_theFileImpl.Close();
                if (this.m_byMemoryBuf != null)
                {
                    this.m_byMemoryBuf = null;
                }
                if (this.m_byDataForDecompressBuf != null)
                {
                    this.m_byDataForDecompressBuf = null;
                }
                this.m_iCryptCount = 0;
            }
        }

        public bool DeclareMemoryAccessMode(bool bMemAccess)
        {
            if (bMemAccess)
            {
                return (this.IsMemoryAccessMode() || this._InitMemoryAccessData());
            }
            return (!this.IsMemoryAccessMode() || this._DeInitMemoryAccessData());
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
                if (bDisposing)
                {
                    this.Close(true);
                }
                this.m_bDisposed = true;
            }
        }

        public static bool FileCompare(string sFile1, string sFile2)
        {
            bool flag;
            using (FileStream stream = new FileStream(sFile1, FileMode.Open))
            {
                using (FileStream stream2 = new FileStream(sFile2, FileMode.Open))
                {
                    int num;
                    byte[] buffer = new byte[0x1000];
                    byte[] buffer2 = new byte[0x1000];
                Label_0026:
                    num = stream.Read(buffer, 0, 0x1000);
                    int num2 = stream2.Read(buffer2, 0, 0x1000);
                    if (num != num2)
                    {
                        return false;
                    }
                    if (num == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        int num3 = (int)Math.Ceiling((double)(((double)num) / 8.0));
                        for (int i = 0; i < num3; i++)
                        {
                            if (BitConverter.ToInt64(buffer, i * 8) != BitConverter.ToInt64(buffer2, i * 8))
                            {
                                return false;
                            }
                        }
                        goto Label_0026;
                    }
                }
            }
            return flag;
        }

        public static bool FileCopy(string sSrcFile, string sDestFile, bool bOverWrite)
        {
            if (!File.Exists(sSrcFile))
            {
                return false;
            }
            string sDir = CPath.DriverDir(sDestFile);
            if (!CPath.IsDirExist(sDir))
            {
                CPath.CreateDir(sDir, true);
            }
            File.Copy(sSrcFile, sDestFile, bOverWrite);
            return true;
        }

        public static bool FileDelete(string sFile, bool bSupressErrMsg = false)
        {
            File.Delete(sFile);
            return true;
        }

        public static bool FileMove(string sSrcFile, string sDestFile)
        {
            if (!File.Exists(sSrcFile))
            {
                return false;
            }
            string sDir = CPath.DriverDir(sDestFile);
            if (!CPath.IsDirExist(sDir))
            {
                CPath.CreateDir(sDir, true);
            }
            File.Move(sSrcFile, sDestFile);
            return true;
        }

        public string Filename()
        {
            return this.m_sFilename;
        }

        public long FileSize()
        {
            return this.m_theFileImpl.FileSize();
        }

        public uint FileVersion()
        {
            return this.m_nFileVersion;
        }

        public void FileVersion(uint nFileVersion)
        {
            this.m_nFileVersion = nFileVersion;
        }

        ~CFile()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            lock (this.m_LockObject)
            {
                if (this.IsOpened())
                {
                    if (this.IsMemoryAccessMode())
                    {
                        Debug.Assert(this._DeInitMemoryAccessData());
                    }
                    this.m_theFileImpl.Flush();
                }
            }
        }

        public static CFileSystem GetFileSystem()
        {
            return m_theFileSystem;
        }

        public bool IsBinary()
        {
            if ((this.m_iMode & 2) == 0)
            {
                return false;
            }
            return true;
        }

        public bool IsEnd()
        {
            if (this.IsOpened())
            {
                if ((this.m_iMode & 0x20) != 0)
                {
                    if (this.m_iMemBufIdx < this.m_byMemoryBuf.Length)
                    {
                        return false;
                    }
                    return true;
                }
                if (this.m_theFileImpl.Tell() < this.m_theFileImpl.FileSize())
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsFileExist(string sFile)
        {
            return m_theFileSystem.IsFileExist(sFile);
        }

        public bool IsMemoryAccessMode()
        {
            if ((this.m_iMode & 0x20) == 0)
            {
                return false;
            }
            return true;
        }

        public bool IsOpened()
        {
            return this.m_theFileImpl.IsOpened();
        }

        public bool Open(string sFilename, int iMode = 5, bool bSuppressOpenFileError = true)
        {
            this.Close(true);
            if (!this._ModeChecking(iMode))
            {
                return false;
            }
            if (sFilename.Length <= 0)
            {
                return false;
            }
            if ((iMode & 8) != 0)
            {
                string sDir = CPath.DriverDir(sFilename);
                if (!CPath.IsDirExist(sDir))
                {
                    CPath.CreateDir(sDir, true);
                }
            }
            if (((iMode & 1) == 1) && ((iMode & 0x18) == 0x18))
            {
                this.m_iTextEncoding = this._ReadUnicodeBOM(sFilename, iMode);
            }
            if (!this.m_theFileImpl.Open(sFilename, iMode, bSuppressOpenFileError))
            {
                return false;
            }
            this.m_sFilename = sFilename;
            this.m_iMode = iMode;
            if (this.IsMemoryAccessMode())
            {
                this.m_iMode &= -33;
                if (!this._InitMemoryAccessData())
                {
                    return false;
                }
            }
            if ((iMode & 1) == 1)
            {
                if ((iMode & 4) == 4)
                {
                    this.m_iTextEncoding = this._ReadUnicodeBOM(sFilename, iMode);
                }
                else if (this.Tell() == 0L)
                {
                    this.m_iTextEncoding = iMode & 0x700;
                    this._WriteUnicodeBOM(this.m_iTextEncoding);
                }
            }
            if ((iMode & 0x18) == 0x18)
            {
                this.m_theFileImpl.SeekEnd(0L);
            }
            return true;
        }

        public int Read(out bool b)
        {
            string str;
            if (this.IsEnd())
            {
                b = false;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[1];
                int num = this.Read(theBuf, 1);
                Debug.Assert(num == 1);
                b = BitConverter.ToBoolean(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            str = str.ToUpper();
            switch (str)
            {
            case "TRUE":
                b = true;
                return num2;

            case "FALSE":
                b = false;
                return num2;
            }
            b = Convert.ToBoolean(str);
            return num2;
        }

        public int Read(out byte by)
        {
            if (this.IsEnd())
            {
                by = 0;
                return 0;
            }
            byte[] theBuf = new byte[1];
            int num = this.Read(theBuf, 1);
            Debug.Assert(num == 1);
            by = theBuf[0];
            return num;
        }

        public int Read(out char c)
        {
            if (this.IsEnd())
            {
                c = '\0';
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[2];
                int num = this.Read(theBuf, 2);
                Debug.Assert(num == 2);
                c = BitConverter.ToChar(theBuf, 0);
                return num;
            }
            return this._ReadChar(out c);
        }

        public int Read(out double d)
        {
            string str;
            if (this.IsEnd())
            {
                d = 0.0;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[8];
                int num = this.Read(theBuf, 8);
                Debug.Assert(num == 8);
                d = BitConverter.ToDouble(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            d = Convert.ToDouble(str);
            return num2;
        }

        public int Read(out short i)
        {
            string str;
            if (this.IsEnd())
            {
                i = 0;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[2];
                int num = this.Read(theBuf, 2);
                Debug.Assert(num == 2);
                i = BitConverter.ToInt16(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            i = Convert.ToInt16(str);
            return num2;
        }

        public int Read(out int i)
        {
            string str;
            if (this.IsEnd())
            {
                i = 0;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[4];
                int num = this.Read(theBuf, 4);
                Debug.Assert(num == 4);
                i = BitConverter.ToInt32(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            i = Convert.ToInt32(str);
            return num2;
        }

        public int Read(out long l)
        {
            string str;
            if (this.IsEnd())
            {
                l = 0L;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[8];
                int num = this.Read(theBuf, 8);
                Debug.Assert(num == 8);
                l = BitConverter.ToInt64(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            l = Convert.ToInt64(str);
            return num2;
        }

        public int Read(out sbyte by)
        {
            if (this.IsEnd())
            {
                by = 0;
                return 0;
            }
            byte[] theBuf = new byte[1];
            int num = this.Read(theBuf, 1);
            Debug.Assert(num == 1);
            by = (sbyte)theBuf[0];
            return num;
        }

        public int Read(out float f)
        {
            string str;
            if (this.IsEnd())
            {
                f = 0f;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[4];
                int num = this.Read(theBuf, 4);
                Debug.Assert(num == 4);
                f = BitConverter.ToSingle(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            f = Convert.ToSingle(str);
            return num2;
        }

        public int Read(out string s)
        {
            char ch;
            if (this.IsEnd())
            {
                s = null;
                return 0;
            }
            if (this.IsBinary())
            {
                int num;
                int num2 = this.Read(out num);
                byte[] theBuf = new byte[num];
                int num3 = this.Read(theBuf, num);
                Debug.Assert(num3 == num);
                s = Encoding.UTF8.GetString(theBuf);
                return (num2 + num3);
            }
            s = "";
            int index = 0;
            char[] chArray = new char[0x80];
            this._SkipBlank();
            int num5 = this._ReadChar(out ch);
            int num6 = num5;
            while (((num5 != 0) && (ch != ' ')) && (((ch != '\n') && (ch != '\r')) && (ch != '\t')))
            {
                chArray[index] = ch;
                index++;
                if (index == 0x80)
                {
                    s = s + new string(chArray, 0, index);
                    index = 0;
                }
                if (this.IsEnd())
                {
                    break;
                }
                num5 = this._ReadChar(out ch);
                num6 += num5;
            }
            if (index > 0)
            {
                s = s + new string(chArray, 0, index);
            }
            return num6;
        }

        public int Read(out ushort i)
        {
            string str;
            if (this.IsEnd())
            {
                i = 0;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[2];
                int num = this.Read(theBuf, 2);
                Debug.Assert(num == 2);
                i = BitConverter.ToUInt16(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            i = Convert.ToUInt16(str);
            return num2;
        }

        public int Read(out uint i)
        {
            string str;
            if (this.IsEnd())
            {
                i = 0;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[4];
                int num = this.Read(theBuf, 4);
                Debug.Assert(num == 4);
                i = BitConverter.ToUInt32(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            i = Convert.ToUInt32(str);
            return num2;
        }

        public int Read(out ulong l)
        {
            string str;
            if (this.IsEnd())
            {
                l = 0L;
                return 0;
            }
            if (this.IsBinary())
            {
                byte[] theBuf = new byte[8];
                int num = this.Read(theBuf, 8);
                Debug.Assert(num == 8);
                l = BitConverter.ToUInt64(theBuf, 0);
                return num;
            }
            int num2 = this.Read(out str);
            l = Convert.ToUInt64(str);
            return num2;
        }

        public int Read(byte[] byteData)
        {
            return this.Read(byteData, byteData.Length);
        }

        public int Read(byte[] theBuf, int iSizeInByte)
        {
            Debug.Assert(iSizeInByte <= theBuf.Length);
            lock (this.m_LockObject)
            {
                if ((this.m_iMode & 0x20) != 0)
                {
                    if ((this.m_iMemBufIdx + iSizeInByte) > this.m_byMemoryBuf.Length)
                    {
                        iSizeInByte = this.m_byMemoryBuf.Length - this.m_iMemBufIdx;
                    }
                    Buffer.BlockCopy(this.m_byMemoryBuf, this.m_iMemBufIdx, theBuf, 0, iSizeInByte);
                    this.m_iMemBufIdx += iSizeInByte;
                    return iSizeInByte;
                }
                int num2 = this.m_theFileImpl.Read(theBuf, iSizeInByte);
                if (this.m_byCryptSalt.Length != 0)
                {
                    this._Decrypt(theBuf, 0, iSizeInByte);
                }
                return num2;
            }
        }

        public bool ReadHeader(string sMagicStr, string sCheckName, uint iCheckFileVersion, out uint iFileVersion, bool bShowErrorMessage = true)
        {
            uint num2;
            uint num3;
            byte[] bytes = Encoding.UTF8.GetBytes(sMagicStr);
            byte[] buffer2 = Encoding.UTF8.GetBytes(sCheckName.ToUpper());
            iFileVersion = 0;
            byte[] byteData = new byte[8];
            this.Read(byteData);
            for (int i = 0; i < 8; i++)
            {
                byteData[i] = (byte)(byteData[i] ^ ((byte)(0x41 + (i * 3))));
            }
            this.Read(out num2);
            num2 ^= 0x41374137;
            this.Read(out num3);
            num3 ^= 0x41374137;
            byte[] buffer4 = new byte[0x40];
            this.Read(buffer4);
            for (int j = 0; j < 0x40; j++)
            {
                buffer4[j] = (byte)(buffer4[j] ^ ((byte)(((0x41 + j) + j) + j)));
            }
            buffer4[0x3f] = 0;
            int count = 0;
            for (int k = 0; k < 0x40; k++)
            {
                if (buffer4[k] == 0)
                {
                    break;
                }
                count++;
            }
            byte[] buffer5 = new byte[0x10];
            this.Read(buffer5);
            for (int m = 0; m < 0x10; m++)
            {
                buffer5[m] = (byte)(buffer5[m] ^ ((byte)((0x41 + m) + 4)));
            }
            int length = bytes.Length;
            if (length > 8)
            {
                length = 8;
            }
            for (int n = 0; n < length; n++)
            {
                if (byteData[n] != bytes[n])
                {
                    if (bShowErrorMessage)
                    {
                        Log.LogMsg("CFile::ReadHeader(): File[ " + this.Filename() + " ], File magic[ " + byteData.ToString() + " ], parameter magic[ " + sMagicStr + " ]");
                    }
                    return false;
                }
            }
            this.m_nHeaderVersion = num2;
            this.m_nFileVersion = num3;
            if (num3 != iCheckFileVersion)
            {
                if (bShowErrorMessage)
                {
                    Log.LogMsg("CFile::ReadHeader(): File[ " + this.Filename() + " ], File version[ " + num3.ToString() + " ], parameter version[ " + iCheckFileVersion.ToString() + " ]");
                }
                return false;
            }
            iFileVersion = num3;
            string str = Encoding.UTF8.GetString(buffer4, 0, count);
            if (buffer2.Length > 0x3f)
            {
                if (sCheckName.Substring(0, 0x3f).ToUpper() != str.ToUpper())
                {
                    if (bShowErrorMessage)
                    {
                        Log.LogMsg("CFile::ReadHeader(): File[ " + this.Filename() + " ], File name[ " + buffer4.ToString() + " ], parameter name[ " + sCheckName + " ]");
                    }
                    return false;
                }
            }
            else if (sCheckName.ToUpper() != str.ToUpper())
            {
                if (bShowErrorMessage)
                {
                    Log.LogMsg("CFile::ReadHeader(): File[ " + this.Filename() + " ], File name[ " + buffer4.ToString() + " ], parameter name[ " + sCheckName + " ]");
                }
                return false;
            }
            return true;
        }

        public bool SeekBegin(long lDistanceToMove)
        {
            lock (this.m_LockObject)
            {
                if (!this.IsOpened())
                {
                    return false;
                }
                if (this.m_byCryptSalt.Length > 0)
                {
                    this.m_iCryptCount = ((int)lDistanceToMove) % this.m_byCryptSalt.Length;
                }
                if ((this.m_iMode & 0x20) != 0)
                {
                    this.m_iMemBufIdx = (int)lDistanceToMove;
                    Debug.Assert(this.m_iMemBufIdx <= this.m_byMemoryBuf.Length);
                    return true;
                }
                return this.m_theFileImpl.SeekBegin(lDistanceToMove);
            }
        }

        public bool SeekCurrent(long lDistanceToMove)
        {
            lock (this.m_LockObject)
            {
                if (!this.IsOpened())
                {
                    return false;
                }
                if (this.m_byCryptSalt.Length > 0)
                {
                    this.m_iCryptCount = (this.m_iCryptCount + ((int)lDistanceToMove)) % this.m_byCryptSalt.Length;
                }
                if ((this.m_iMode & 0x20) != 0)
                {
                    this.m_iMemBufIdx += (int)lDistanceToMove;
                    Debug.Assert(this.m_iMemBufIdx <= this.m_byMemoryBuf.Length);
                    return true;
                }
                return this.m_theFileImpl.SeekCurrent(lDistanceToMove);
            }
        }

        public bool SeekEnd(long lDistanceToMove = 0L)
        {
            lock (this.m_LockObject)
            {
                if (!this.IsOpened())
                {
                    return false;
                }
                if ((this.m_iMode & 0x20) != 0)
                {
                    this.m_iMemBufIdx = this.m_byMemoryBuf.Length + ((int)lDistanceToMove);
                    Debug.Assert(this.m_iMemBufIdx <= this.m_byMemoryBuf.Length);
                    if (this.m_byCryptSalt.Length > 0)
                    {
                        this.m_iCryptCount = this.m_iMemBufIdx % this.m_byCryptSalt.Length;
                    }
                    return true;
                }
                if (this.m_byCryptSalt.Length > 0)
                {
                    this.m_iCryptCount = ((int)(this.m_theFileImpl.FileSize() + lDistanceToMove)) % this.m_byCryptSalt.Length;
                }
                return this.m_theFileImpl.SeekEnd(lDistanceToMove);
            }
        }

        public void SetCryptSalt(byte[] bySalt)
        {
            this.m_byCryptSalt = bySalt;
        }

        public static void SetCustomFileSystem(CFileSystem theFileSystem)
        {
            if (theFileSystem == null)
            {
                m_theFileSystem = m_theDefaulFileSystem;
            }
            else
            {
                m_theFileSystem = theFileSystem;
            }
        }

        public long Tell()
        {
            if (!this.IsOpened())
            {
                return -1L;
            }
            if ((this.m_iMode & 0x20) != 0)
            {
                return (long)this.m_iMemBufIdx;
            }
            return this.m_theFileImpl.Tell();
        }

        public int TextEncoding()
        {
            return this.m_iTextEncoding;
        }

        public int Write(bool b)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(b);
                return this.Write(bytes, bytes.Length);
            }
            char ch = b ? '1' : '0';
            return this._WriteChar(ch);
        }

        public int Write(byte by)
        {
            Debug.Assert(this.IsOpened());
            byte[] theBuf = new byte[] { by };
            return this.Write(theBuf, theBuf.Length);
        }

        public int Write(char c)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(c);
                return this.Write(bytes, bytes.Length);
            }
            return this._WriteChar(c);
        }

        public int Write(double d)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(d);
                return this.Write(bytes, bytes.Length);
            }
            string s = d.ToString();
            return this.Write(s);
        }

        public int Write(short i)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(i);
                return this.Write(bytes, bytes.Length);
            }
            string s = i.ToString();
            return this.Write(s);
        }

        public int Write(int i)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(i);
                return this.Write(bytes, bytes.Length);
            }
            string s = i.ToString();
            return this.Write(s);
        }

        public int Write(long l)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(l);
                return this.Write(bytes, bytes.Length);
            }
            string s = l.ToString();
            return this.Write(s);
        }

        public int Write(sbyte by)
        {
            Debug.Assert(this.IsOpened());
            byte[] theBuf = new byte[] { (byte)by };
            return this.Write(theBuf, theBuf.Length);
        }

        public int Write(float f)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(f);
                return this.Write(bytes, bytes.Length);
            }
            string s = f.ToString();
            return this.Write(s);
        }

        public int Write(string s)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                if (s == null)
                {
                    return this.Write(0);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                int num = this.Write(bytes.Length);
                return (this.Write(bytes, bytes.Length) + num);
            }
            int num2 = 0;
            for (int i = 0; i < s.Length; i++)
            {
                num2 += this._WriteChar(s[i]);
            }
            return num2;
        }

        public int Write(ushort i)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(i);
                return this.Write(bytes, bytes.Length);
            }
            string s = i.ToString();
            return this.Write(s);
        }

        public int Write(uint i)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(i);
                return this.Write(bytes, bytes.Length);
            }
            string s = i.ToString();
            return this.Write(s);
        }

        public int Write(ulong l)
        {
            Debug.Assert(this.IsOpened());
            if (this.IsBinary())
            {
                byte[] bytes = BitConverter.GetBytes(l);
                return this.Write(bytes, bytes.Length);
            }
            string s = l.ToString();
            return this.Write(s);
        }

        public int Write(byte[] byteData)
        {
            return this.Write(byteData, byteData.Length);
        }

        public int Write(byte[] theBuf, int iSizeInByte)
        {
            Debug.Assert(this.IsOpened());
            Debug.Assert(iSizeInByte <= theBuf.Length);
            lock (this.m_LockObject)
            {
                if ((this.m_iMode & 0x20) != 0)
                {
                    if ((this.m_iMemBufIdx + iSizeInByte) > this.m_byMemoryBuf.Length)
                    {
                        byte[] array = new byte[Math.Max(Math.Min((int)(this.m_byMemoryBuf.Length * 2), (int)(this.m_byMemoryBuf.Length + 0x8000)), (((this.m_iMemBufIdx + iSizeInByte) / 0x800) + 1) * 0x800)];
                        this.m_byMemoryBuf.CopyTo(array, 0);
                        this.m_byMemoryBuf = array;
                    }
                    Buffer.BlockCopy(theBuf, 0, this.m_byMemoryBuf, this.m_iMemBufIdx, iSizeInByte);
                    this.m_iMemBufIdx += iSizeInByte;
                    return iSizeInByte;
                }
                int num3 = 0;
                if (this.m_byCryptSalt.Length != 0)
                {
                    byte[] dst = new byte[iSizeInByte];
                    Buffer.BlockCopy(theBuf, 0, dst, 0, iSizeInByte);
                    this._Encrypt(dst, 0, iSizeInByte);
                    num3 = this.m_theFileImpl.Write(dst, iSizeInByte);
                }
                else
                {
                    num3 = this.m_theFileImpl.Write(theBuf, iSizeInByte);
                }
                return num3;
            }
        }

        public bool WriteHeader(string sMagicStr, string sCheckName, uint nCheckFileVersion)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(sMagicStr);
            byte[] buffer2 = Encoding.UTF8.GetBytes(sCheckName.ToUpper());
            byte[] buffer4 = new byte[8];
            int num = Math.Min(8, bytes.Length);
            for (int i = 0; i < num; i++)
            {
                buffer4[i] = bytes[i];
            }
            buffer4[7] = 0;
            byte[] theBuf = new byte[8];
            for (int j = 0; j < 8; j++)
            {
                theBuf[j] = (byte)(buffer4[j] ^ (0x41 + (j * 3)));
            }
            if (this.Write(theBuf, 8) != 8)
            {
                Log.LogErrorMsg("CFile::WriteHeader() failed! Write magic word [ " + sMagicStr + " ] failed");
                return false;
            }
            uint num4 = 0x413742df;
            this.Write(num4);
            this.m_nFileVersion = nCheckFileVersion;
            uint num5 = nCheckFileVersion ^ 0x41374137;
            this.Write(num5);
            int length = buffer2.Length;
            if (length > 0x3f)
            {
                length = 0x3f;
                string[] strArray = new string[] { "CFile::WriteHeader() error! ", buffer2.ToString(), " file name too long, please reduce them in ", 0x3f.ToString(), " characters!" };
                Log.LogErrorMsg(string.Concat(strArray));
                return false;
            }
            byte[] buffer6 = new byte[0x40];
            for (int k = 0; k < 0x40; k++)
            {
                if (k < length)
                {
                    buffer6[k] = (byte)(buffer2[k] ^ (((0x41 + k) + k) + k));
                }
                else
                {
                    buffer6[k] = (byte)(((0x41 + k) + k) + k);
                }
            }
            if (this.Write(buffer6, 0x40) != 0x40)
            {
                Log.LogErrorMsg("CFile::WriteHeader() failed! Write file name [ " + buffer2.ToString() + " ] failed");
                return false;
            }
            byte[] buffer7 = new byte[0x10];
            for (int m = 0; m < 0x10; m++)
            {
                buffer7[m] = (byte)((0x41 + m) + 4);
            }
            if (this.Write(buffer7, 0x10) != 0x10)
            {
                Log.LogErrorMsg("CFile::WriteHeader() failed! Write reserved data failed!");
                return false;
            }
            return true;
        }
    }
}
