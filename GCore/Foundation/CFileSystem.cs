using System;
using System.IO;

namespace GCore.Foundation
{
    public class CFileSystem
    {
        public virtual bool IsFileExist(string sFile)
        {
            return File.Exists(sFile);
        }

        public virtual IFile NewFile()
        {
            return new CSystemFile(this);
        }

        public class CSystemFile : CFileSystem.IFile
        {
            private FileStream m_BinaryStream;

            public CSystemFile(CFileSystem theFileSystem)
                : base(theFileSystem)
            {
            }

            public override void Close()
            {
                if (this.m_BinaryStream != null)
                {
                    this.m_BinaryStream.Close();
                    this.m_BinaryStream.Dispose();
                    this.m_BinaryStream = null;
                }
            }

            public override long FileSize()
            {
                if (this.m_BinaryStream == null)
                {
                    return 0L;
                }
                return this.m_BinaryStream.Length;
            }

            public override void Flush()
            {
                if (this.m_BinaryStream != null)
                {
                    this.m_BinaryStream.Flush();
                }
            }

            public override bool IsOpened()
            {
                return (this.m_BinaryStream != null);
            }

            public override bool Open(string sFilename, int iMode, bool bSuppressOpenFileError = true)
            {
                try
                {
                    FileMode open = FileMode.Open;
                    FileAccess read = FileAccess.Read;
                    if ((iMode & 0x18) == 0x18)
                    {
                        open = FileMode.Append;
                        read = FileAccess.Write;
                    }
                    else if ((iMode & 4) == 4)
                    {
                        open = FileMode.Open;
                        read = FileAccess.Read;
                    }
                    else if ((iMode & 8) == 8)
                    {
                        open = FileMode.Create;
                        read = FileAccess.Write;
                        string directoryName = Path.GetDirectoryName(sFilename);
                        if (directoryName.Length != 0)
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    this.m_BinaryStream = new FileStream(sFilename, open, read);
                    return true;
                }
                catch (Exception exception)
                {
                    if (!bSuppressOpenFileError)
                    {
                        Log.LogMsg("CSystemFile::Open(): Open binary file " + sFilename + " Fail! - " + exception.ToString());
                    }
                    return false;
                }
            }

            public override int Read(byte[] byData, int iSizeInBytes)
            {
                if (this.m_BinaryStream == null)
                {
                    return 0;
                }
                return this.m_BinaryStream.Read(byData, 0, iSizeInBytes);
            }

            public override bool SeekBegin(long lDistanceToMove)
            {
                if (this.m_BinaryStream == null)
                {
                    return false;
                }
                return (this.m_BinaryStream.Seek(lDistanceToMove, SeekOrigin.Begin) == lDistanceToMove);
            }

            public override bool SeekCurrent(long lDistanceToMove)
            {
                if (this.m_BinaryStream == null)
                {
                    return false;
                }
                long position = this.m_BinaryStream.Position;
                long num2 = this.m_BinaryStream.Seek(lDistanceToMove, SeekOrigin.Current);
                return ((position + lDistanceToMove) == num2);
            }

            public override bool SeekEnd(long lDistanceToMove)
            {
                if (this.m_BinaryStream == null)
                {
                    return false;
                }
                return (this.m_BinaryStream.Seek(lDistanceToMove, SeekOrigin.End) == (this.m_BinaryStream.Length + lDistanceToMove));
            }

            public override long Tell()
            {
                if (this.m_BinaryStream == null)
                {
                    return 0L;
                }
                return this.m_BinaryStream.Position;
            }

            public override int Write(byte[] byData, int iSizeInBytes)
            {
                if (this.m_BinaryStream == null)
                {
                    return 0;
                }
                try
                {
                    this.m_BinaryStream.Write(byData, 0, iSizeInBytes);
                    return iSizeInBytes;
                }
                catch (Exception exception)
                {
                    Log.LogErrorMsg(string.Concat(new object[] { "CSystemFile::Write(): Write ", iSizeInBytes, " bytes failed! - ", exception.ToString() }));
                    return 0;
                }
            }
        }

        public abstract class IFile
        {
            private CFileSystem m_theFileSystemRef;
            public const int MODE_APPEND = 0x18;
            public const int MODE_BINARY = 2;
            public const int MODE_READ = 4;
            public const int MODE_TEXT = 1;
            public const int MODE_WRITE = 8;

            public IFile(CFileSystem theFileSystem)
            {
                this.m_theFileSystemRef = theFileSystem;
            }

            public abstract void Close();
            public abstract long FileSize();
            public abstract void Flush();
            public abstract bool IsOpened();
            public abstract bool Open(string sFilename, int iMode, bool bSuppressOpenFileError = true);
            public abstract int Read(byte[] byData, int iSizeInBytes);
            public abstract bool SeekBegin(long lDistanceToMove);
            public abstract bool SeekCurrent(long lDistanceToMove);
            public abstract bool SeekEnd(long lDistanceToMove = 0L);
            public abstract long Tell();
            public abstract int Write(byte[] byData, int iSizeInBytes);
        }
    }
}
