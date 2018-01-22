using System;
using System.Text;
using System.Linq;

namespace GCore.Foundation
{
    public sealed class CStream
    {
        public const int DEFAULT_MAX_STRING_LEN = 0x2000;
        private readonly uint[] INT_SIGNED_BITMASK;
        private readonly uint[] INT_UNSIGNED_BITMASK;
        private readonly ulong[] LONG_SIGNED_BITMASK;
        private readonly ulong[] LONG_UNSIGNED_BITMASK;
        private byte[] m_byteData;
        private EWriteFlag m_eWriteFlag;
        private uint m_nBeginPos;
        private uint m_nDataLength;
        private uint m_nPos;
        private uint m_nSizeWritten;
        public const int MAX_STRING_LEN = 0xffff;

        public CStream()
        {
            this.INT_SIGNED_BITMASK = new uint[] { 0xffffff80, 0xffff8000, 0xff800000, 0x80000000 };
            this.INT_UNSIGNED_BITMASK = new uint[] { 0xffffff00, 0xffff0000, 0xff000000, 0 };
            this.LONG_SIGNED_BITMASK = new ulong[] { 18446744073709551488L, 18446744073709518848L, 18446744073701163008L, 18446744071562067968L, 18446743523953737728L, 18446603336221196288L, 18410715276690587648L, 9223372036854775808L };
            this.LONG_UNSIGNED_BITMASK = new ulong[] { 18446744073709551360L, 18446744073709486080L, 18446744073692774400L, 18446744069414584320L, 18446742974197923840L, 18446462598732840960L, 18374686479671623680L, 0L };
        }

        public CStream(uint nSize)
        {
            this.INT_SIGNED_BITMASK = new uint[] { 0xffffff80, 0xffff8000, 0xff800000, 0x80000000 };
            this.INT_UNSIGNED_BITMASK = new uint[] { 0xffffff00, 0xffff0000, 0xff000000, 0 };
            this.LONG_SIGNED_BITMASK = new ulong[] { 18446744073709551488L, 18446744073709518848L, 18446744073701163008L, 18446744071562067968L, 18446743523953737728L, 18446603336221196288L, 18410715276690587648L, 9223372036854775808L };
            this.LONG_UNSIGNED_BITMASK = new ulong[] { 18446744073709551360L, 18446744073709486080L, 18446744073692774400L, 18446744069414584320L, 18446742974197923840L, 18446462598732840960L, 18374686479671623680L, 0L };
            this.m_byteData = new byte[nSize];
            this.m_nDataLength = nSize;
        }

        public CStream(CStream memoryStream, uint nBeginPos, uint nStartPos, uint nLength, bool bCopy)
        {
            this.INT_SIGNED_BITMASK = new uint[] { 0xffffff80, 0xffff8000, 0xff800000, 0x80000000 };
            this.INT_UNSIGNED_BITMASK = new uint[] { 0xffffff00, 0xffff0000, 0xff000000, 0 };
            this.LONG_SIGNED_BITMASK = new ulong[] { 18446744073709551488L, 18446744073709518848L, 18446744073701163008L, 18446744071562067968L, 18446743523953737728L, 18446603336221196288L, 18410715276690587648L, 9223372036854775808L };
            this.LONG_UNSIGNED_BITMASK = new ulong[] { 18446744073709551360L, 18446744073709486080L, 18446744073692774400L, 18446744069414584320L, 18446742974197923840L, 18446462598732840960L, 18374686479671623680L, 0L };
            if (bCopy)
            {
                this.m_byteData = new byte[nLength];
                Buffer.BlockCopy(memoryStream.m_byteData, (int)nBeginPos, this.m_byteData, 0, (int)nLength);
                this.m_nBeginPos = 0;
                this.m_nDataLength = nLength;
            }
            else
            {
                this.m_byteData = memoryStream.m_byteData;
                this.m_nBeginPos = nBeginPos;
                this.m_nDataLength = memoryStream.m_nDataLength;
            }
            this.m_nPos = nStartPos;
            this.m_nSizeWritten = nLength;
        }

        public CStream(byte[] byteData, uint nBeginPos, uint nStartPos = 0, uint nSizeWritten = 0, uint nDataLength = 0)
        {
            this.INT_SIGNED_BITMASK = new uint[] { 0xffffff80, 0xffff8000, 0xff800000, 0x80000000 };
            this.INT_UNSIGNED_BITMASK = new uint[] { 0xffffff00, 0xffff0000, 0xff000000, 0 };
            this.LONG_SIGNED_BITMASK = new ulong[] { 18446744073709551488L, 18446744073709518848L, 18446744073701163008L, 18446744071562067968L, 18446743523953737728L, 18446603336221196288L, 18410715276690587648L, 9223372036854775808L };
            this.LONG_UNSIGNED_BITMASK = new ulong[] { 18446744073709551360L, 18446744073709486080L, 18446744073692774400L, 18446744069414584320L, 18446742974197923840L, 18446462598732840960L, 18374686479671623680L, 0L };
            this.m_byteData = byteData;
            this.m_nBeginPos = nBeginPos;
            this.m_nPos = nStartPos;
            this.m_nSizeWritten = nSizeWritten;
            if (nDataLength <= 0)
            {
                this.m_nDataLength = (uint)byteData.Length;
            }
            else
            {
                this.m_nDataLength = nDataLength;
            }
        }

        private void __Write(byte[] byteData)
        {
            if (!BitConverter.IsLittleEndian)
            {
                byteData.Reverse<byte>();
            }
            byteData.CopyTo(this.m_byteData, (long)(this.m_nBeginPos + this.m_nPos));
            this.m_nPos += (uint)byteData.Length;
            if (this.m_nPos > this.m_nSizeWritten)
            {
                this.m_nSizeWritten = this.m_nPos;
            }
        }

        private void __Write(byte[] byteData, int iBegin, int iLength)
        {
            if (!BitConverter.IsLittleEndian)
            {
                byteData.Reverse<byte>();
            }
            Buffer.BlockCopy(byteData, iBegin, this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), iLength);
            this.m_nPos += (uint)iLength;
            if (this.m_nPos > this.m_nSizeWritten)
            {
                this.m_nSizeWritten = this.m_nPos;
            }
        }

        private bool _Read(out bool b)
        {
            if ((this.m_nPos + 1) > this.m_nSizeWritten)
            {
                b = false;
                return false;
            }
            b = this.m_byteData[this.m_nBeginPos + this.m_nPos] != 0;
            this.m_nPos++;
            return true;
        }

        private bool _Read(out byte i)
        {
            if ((this.m_nPos + 1) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            i = this.m_byteData[this.m_nBeginPos + this.m_nPos];
            this.m_nPos++;
            return true;
        }

        private bool _Read(out char c)
        {
            if ((this.m_nPos + 2) > this.m_nSizeWritten)
            {
                c = '\0';
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[2];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                c = BitConverter.ToChar(dst, 0);
            }
            else
            {
                c = BitConverter.ToChar(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 2;
            return true;
        }

        private bool _Read(out double d)
        {
            if ((this.m_nPos + 8) > this.m_nSizeWritten)
            {
                d = 0.0;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[8];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                d = BitConverter.ToDouble(dst, 0);
            }
            else
            {
                d = BitConverter.ToDouble(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 8;
            return true;
        }

        private bool _Read(out short i)
        {
            if ((this.m_nPos + 2) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[2];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                i = BitConverter.ToInt16(dst, 0);
            }
            else
            {
                i = BitConverter.ToInt16(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 2;
            return true;
        }

        private bool _Read(out int i)
        {
            if ((this.m_nPos + 4) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[4];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                i = BitConverter.ToInt32(dst, 0);
            }
            else
            {
                i = BitConverter.ToInt32(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 4;
            return true;
        }

        private bool _Read(out long l)
        {
            if ((this.m_nPos + 8) > this.m_nSizeWritten)
            {
                l = 0L;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[8];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                l = BitConverter.ToInt64(dst, 0);
            }
            else
            {
                l = BitConverter.ToInt64(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 8;
            return true;
        }

        private bool _Read(out float f)
        {
            if ((this.m_nPos + 4) > this.m_nSizeWritten)
            {
                f = 0f;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[4];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                f = BitConverter.ToSingle(dst, 0);
            }
            else
            {
                f = BitConverter.ToSingle(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 4;
            return true;
        }

        private bool _Read(out ushort i)
        {
            if ((this.m_nPos + 2) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[2];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                i = BitConverter.ToUInt16(dst, 0);
            }
            else
            {
                i = BitConverter.ToUInt16(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 2;
            return true;
        }

        private bool _Read(out uint i)
        {
            if ((this.m_nPos + 4) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[4];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                i = BitConverter.ToUInt32(dst, 0);
            }
            else
            {
                i = BitConverter.ToUInt32(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 4;
            return true;
        }

        private bool _Read(out ulong l)
        {
            if ((this.m_nPos + 8) > this.m_nSizeWritten)
            {
                l = 0L;
                return false;
            }
            if (!BitConverter.IsLittleEndian)
            {
                byte[] dst = new byte[8];
                Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, dst.Length);
                dst.Reverse<byte>();
                l = BitConverter.ToUInt64(dst, 0);
            }
            else
            {
                l = BitConverter.ToUInt64(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos));
            }
            this.m_nPos += 8;
            return true;
        }

        private bool _Read(out int i, int iByteCnt)
        {
            if ((this.m_nPos + iByteCnt) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            byte[] dst = new byte[4];
            Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, iByteCnt);
            if (!BitConverter.IsLittleEndian)
            {
                dst.Reverse<byte>();
            }
            i = BitConverter.ToInt32(dst, 0);
            this.m_nPos += (uint)iByteCnt;
            return true;
        }

        private bool _Read(out long l, int iByteCnt)
        {
            if ((this.m_nPos + iByteCnt) > this.m_nSizeWritten)
            {
                l = 0L;
                return false;
            }
            byte[] dst = new byte[8];
            Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, iByteCnt);
            if (!BitConverter.IsLittleEndian)
            {
                dst.Reverse<byte>();
            }
            l = BitConverter.ToInt64(dst, 0);
            this.m_nPos += (uint)iByteCnt;
            return true;
        }

        private bool _Read(out uint i, int iByteCnt)
        {
            if ((this.m_nPos + iByteCnt) > this.m_nSizeWritten)
            {
                i = 0;
                return false;
            }
            byte[] dst = new byte[4];
            Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, iByteCnt);
            if (!BitConverter.IsLittleEndian)
            {
                dst.Reverse<byte>();
            }
            i = BitConverter.ToUInt32(dst, 0);
            this.m_nPos += (uint)iByteCnt;
            return true;
        }

        private bool _Read(out ulong l, int iByteCnt)
        {
            if ((this.m_nPos + iByteCnt) > this.m_nSizeWritten)
            {
                l = 0L;
                return false;
            }
            byte[] dst = new byte[8];
            Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), dst, 0, iByteCnt);
            if (!BitConverter.IsLittleEndian)
            {
                dst.Reverse<byte>();
            }
            l = BitConverter.ToUInt64(dst, 0);
            this.m_nPos += (uint)iByteCnt;
            return true;
        }

        private bool _Read(ref byte[] byteData, int iByteCnt)
        {
            if ((this.m_nPos + iByteCnt) > this.m_nSizeWritten)
            {
                return false;
            }
            Buffer.BlockCopy(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), byteData, 0, iByteCnt);
            this.m_nPos += (uint)iByteCnt;
            return true;
        }

        private bool _Write(bool b)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 1) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            this.m_byteData[this.m_nBeginPos + this.m_nPos] = b ? ((byte)1) : ((byte)0);
            this.m_nPos++;
            if (this.m_nPos > this.m_nSizeWritten)
            {
                this.m_nSizeWritten = this.m_nPos;
            }
            return true;
        }

        private bool _Write(byte i)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 1) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            this.m_byteData[this.m_nBeginPos + this.m_nPos] = i;
            this.m_nPos++;
            if (this.m_nPos > this.m_nSizeWritten)
            {
                this.m_nSizeWritten = this.m_nPos;
            }
            return true;
        }

        private bool _Write(char c)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 2) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(c);
            Debug.Assert(bytes.Length == 2);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(double d)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 8) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(d);
            Debug.Assert(bytes.Length == 8);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(short i)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 2) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length == 2);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(int i)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 4) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length == 4);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(long l)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 8) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(l);
            Debug.Assert(bytes.Length == 8);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(float f)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 4) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(f);
            Debug.Assert(bytes.Length == 4);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(ushort i)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 2) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length == 2);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(uint i)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 4) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length == 4);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(ulong l)
        {
            if (((this.m_nBeginPos + this.m_nPos) + 8) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(l);
            Debug.Assert(bytes.Length == 8);
            this.__Write(bytes);
            return true;
        }

        private bool _Write(byte[] byteData)
        {
            if (((this.m_nBeginPos + this.m_nPos) + byteData.Length) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            this.__Write(byteData);
            return true;
        }

        private bool _Write(int i, int iByteCnt)
        {
            if (((this.m_nBeginPos + this.m_nPos) + iByteCnt) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length >= iByteCnt);
            this.__Write(bytes, 0, iByteCnt);
            return true;
        }

        private bool _Write(long l, int iByteCnt)
        {
            if (((this.m_nBeginPos + this.m_nPos) + iByteCnt) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(l);
            Debug.Assert(bytes.Length >= iByteCnt);
            this.__Write(bytes, 0, iByteCnt);
            return true;
        }

        private bool _Write(uint i, int iByteCnt)
        {
            if (((this.m_nBeginPos + this.m_nPos) + iByteCnt) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(i);
            Debug.Assert(bytes.Length >= iByteCnt);
            this.__Write(bytes, 0, iByteCnt);
            return true;
        }

        private bool _Write(ulong l, int iByteCnt)
        {
            if (((this.m_nBeginPos + this.m_nPos) + iByteCnt) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            byte[] bytes = BitConverter.GetBytes(l);
            Debug.Assert(bytes.Length >= iByteCnt);
            this.__Write(bytes, 0, iByteCnt);
            return true;
        }

        private bool _Write(byte[] byteData, int iBegin, int iLength)
        {
            if (((this.m_nBeginPos + this.m_nPos) + iLength) > this.m_nDataLength)
            {
                this.m_eWriteFlag = EWriteFlag.WRITE_BUFFER_FULL;
                return false;
            }
            this.__Write(byteData, iBegin, iLength);
            return true;
        }

        public bool AtEnd()
        {
            if (this.m_nPos < this.m_nSizeWritten)
            {
                return false;
            }
            return true;
        }

        public uint BeginPos()
        {
            return this.m_nBeginPos;
        }

        public uint CurPos()
        {
            return this.m_nPos;
        }

        public bool Read(out byte[] byteData)
        {
            int num;
            if (!this._Read(out num))
            {
                byteData = null;
                return false;
            }
            if (num < 0)
            {
                byteData = null;
                return true;
            }
            if ((this.m_nPos + num) > this.m_nSizeWritten)
            {
                byteData = null;
                return false;
            }
            byteData = new byte[num];
            if (!this._Read(ref byteData, num))
            {
                return false;
            }
            return true;
        }

        public bool Read(out bool b)
        {
            return this._Read(out b);
        }

        public bool Read(out byte i)
        {
            return this._Read(out i);
        }

        public bool Read(out char c)
        {
            return this._Read(out c);
        }

        public bool Read(out short i)
        {
            return this._Read(out i);
        }

        public bool Read(out sbyte i)
        {
            byte num;
            if (!this._Read(out num))
            {
                i = 0;
                return false;
            }
            i = (sbyte)(num + -128);
            return true;
        }

        public bool Read(out ushort i)
        {
            return this._Read(out i);
        }

        public bool Read(ref CStream theStream, int iByteCnt = -1)
        {
            if (theStream != null)
            {
                Debug.Assert(theStream != null);
                if (iByteCnt < 0)
                {
                    uint num;
                    if (this._Read(out num))
                    {
                        theStream.Reset(0, num);
                        if (this._Read(ref theStream.m_byteData, (int)num))
                        {
                            theStream.m_nDataLength = num;
                            return true;
                        }
                    }
                    return false;
                }
                theStream.Reset(0, (uint)iByteCnt);
                if (this._Read(ref theStream.m_byteData, iByteCnt))
                {
                    theStream.m_nDataLength = (uint)iByteCnt;
                    return true;
                }
            }
            return false;
        }

        public bool Read(out DateTime dt, int iByteCnt = 8)
        {
            long num;
            if (!this._Read(out num))
            {
                dt = new DateTime();
                return false;
            }
            dt = CTime.ConvertToDateTime(num, DateTimeKind.Unspecified);
            return true;
        }

        public bool Read(out double d, int iByteCnt = 8)
        {
            return this._Read(out d);
        }

        public bool Read(out int i, int iByteCnt = 4)
        {
            if (iByteCnt == 4)
            {
                return this._Read(out i);
            }
            bool flag = this._Read(out i, iByteCnt);
            if ((((long)i) & this.INT_SIGNED_BITMASK[iByteCnt - 1]) != 0L)
            {
                i = (int)((uint)i | this.INT_SIGNED_BITMASK[iByteCnt - 1]);
            }
            return flag;
        }

        public bool Read(out long l, int iByteCnt = 8)
        {
            if (iByteCnt == 8)
            {
                return this._Read(out l);
            }
            bool flag = this._Read(out l, iByteCnt);
            if (((ulong)l & this.LONG_SIGNED_BITMASK[iByteCnt - 1]) != 0L)
            {
                l = (long)((ulong)l | this.LONG_SIGNED_BITMASK[iByteCnt - 1]);
            }
            return flag;
        }

        public bool Read(out float f, int iByteCnt = 4)
        {
            return this._Read(out f);
        }

        public bool Read(out string s, int iMaxStringLen = 0xffff)
        {
            int num;
            bool flag;
            if (!this._Read(out num))
            {
                s = null;
                return false;
            }
            if (num < 0)
            {
                s = null;
                return true;
            }
            if (num == 0)
            {
                s = "";
                return true;
            }
            if (num > iMaxStringLen)
            {
                s = null;
                return false;
            }
            if (!this._Read(out flag))
            {
                s = "";
                return false;
            }
            if (flag)
            {
                char ch;
                num *= 2;
                if ((this.m_nPos + num) > this.m_nSizeWritten)
                {
                    s = null;
                    return false;
                }
                s = Encoding.Unicode.GetString(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), num);
                this.m_nPos += (uint)num;
                if (!this._Read(out ch))
                {
                    return false;
                }
            }
            else
            {
                byte num2;
                if ((this.m_nPos + num) > this.m_nSizeWritten)
                {
                    s = null;
                    return false;
                }
                s = Encoding.ASCII.GetString(this.m_byteData, (int)(this.m_nBeginPos + this.m_nPos), num);
                this.m_nPos += (uint)num;
                if (!this._Read(out num2))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Read(out uint i, int iByteCnt = 4)
        {
            if (iByteCnt == 4)
            {
                return this._Read(out i);
            }
            return this._Read(out i, iByteCnt);
        }

        public bool Read(out ulong l, int iByteCnt = 8)
        {
            if (iByteCnt == 8)
            {
                return this._Read(out l);
            }
            return this._Read(out l, iByteCnt);
        }

        public void Reset(uint nPos = 0)
        {
            Debug.Assert(nPos >= 0);
            Debug.Assert(nPos < this.m_nDataLength);
            this.m_nPos = nPos;
            this.m_nSizeWritten = nPos;
        }

        public void Reset(uint nPos, uint nSizeWritten)
        {
            Debug.Assert(nPos >= 0);
            Debug.Assert(nPos < this.m_nDataLength);
            this.m_nPos = nPos;
            this.m_nSizeWritten = nSizeWritten;
        }

        public void ResetWriteFlag()
        {
            this.m_eWriteFlag = EWriteFlag.NORMAL;
        }

        public bool Seek(uint nPos)
        {
            if (nPos > this.m_nDataLength)
            {
                return false;
            }
            this.m_nPos = nPos;
            return true;
        }

        public uint TotalSizeWritten()
        {
            return this.m_nSizeWritten;
        }

        public bool Write(byte[] byteData)
        {
            int length;
            if (byteData == null)
            {
                length = -1;
            }
            else
            {
                length = byteData.Length;
            }
            uint nPos = this.m_nPos;
            uint nSizeWritten = this.m_nSizeWritten;
            if (!this._Write(length))
            {
                return false;
            }
            if ((length > 0) && !this._Write(byteData))
            {
                this.Reset(nPos, nSizeWritten);
                return false;
            }
            return true;
        }

        public bool Write(bool b)
        {
            return this._Write(b);
        }

        public bool Write(byte i)
        {
            return this._Write(i);
        }

        public bool Write(char c)
        {
            return this._Write(c);
        }

        public bool Write(short i)
        {
            return this._Write(i);
        }

        public bool Write(sbyte i)
        {
            int num = i - -128;
            return this._Write((byte)num);
        }

        public bool Write(ushort i)
        {
            return this._Write(i);
        }

        public bool Write(DateTime dt, int iByteCnt = 8)
        {
            long l = CTime.ConvertToTimestamp(dt, DateTimeKind.Unspecified);
            return this._Write(l);
        }

        public bool Write(double d, int iByteCnt = 8)
        {
            return this._Write(d);
        }

        public bool Write(int i, int iByteCnt = 4)
        {
            if (iByteCnt == 4)
            {
                return this._Write(i);
            }
            Debug.Assert(iByteCnt > 0);
            if (i >= 0)
            {
                if ((i & this.INT_SIGNED_BITMASK[iByteCnt - 1]) != 0)
                {
                    this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                    Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write +int out of range!(using ", iByteCnt, " bytes to write ", i, "%d)" }));
                    return false;
                }
            }
            else if ((i & this.INT_SIGNED_BITMASK[iByteCnt - 1]) != this.INT_SIGNED_BITMASK[iByteCnt - 1])
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write -int out of range!(using ", iByteCnt, " bytes to write ", i, "%d)" }));
                return false;
            }
            return this._Write(i, iByteCnt);
        }

        public bool Write(long l, int iByteCnt = 8)
        {
            if (iByteCnt == 8)
            {
                return this._Write(l);
            }
            Debug.Assert(iByteCnt > 0);
            if (l >= 0L)
            {
                if ((((ulong)l) & this.LONG_SIGNED_BITMASK[iByteCnt - 1]) != 0L)
                {
                    this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                    Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write +long out of range!(using ", iByteCnt, " bytes to write ", l, ")" }));
                    return false;
                }
            }
            else if ((((ulong)l) & this.LONG_SIGNED_BITMASK[iByteCnt - 1]) != this.LONG_SIGNED_BITMASK[iByteCnt - 1])
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write -long out of range!(using ", iByteCnt, " bytes to write ", l, ")" }));
                return false;
            }
            return this._Write(l, iByteCnt);
        }

        public bool Write(float f, int iByteCnt = 4)
        {
            return this._Write(f);
        }

        public bool Write(string s, int iMaxStringLen = 0x2000)
        {
            int length;
            if ((iMaxStringLen < 0) || (iMaxStringLen > 0xffff))
            {
                throw new Exception("CStream: specifying iMaxStringLen:" + iMaxStringLen + " out of range.");
            }
            if (s == null)
            {
                length = -1;
            }
            else
            {
                length = s.Length;
            }
            if (length > iMaxStringLen)
            {
                length = iMaxStringLen;
                Log.LogWarningMsg(string.Concat(new object[] { "CMemoryStream::Write() Warning: trim input string size(", length, ") according to iMaxStringLen(", iMaxStringLen, ")" }));
            }
            uint nPos = this.m_nPos;
            uint nSizeWritten = this.m_nSizeWritten;
            if (!this._Write(length))
            {
                return false;
            }
            if (length > 0)
            {
                if (!this._Write(true))
                {
                    return false;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(s);
                Debug.Assert(bytes.Length == (s.Length * 2));
                if (length == s.Length)
                {
                    if (!this._Write(bytes))
                    {
                        this.Reset(nPos, nSizeWritten);
                        return false;
                    }
                }
                else if (!this._Write(bytes, 0, length * 2))
                {
                    this.Reset(nPos, nSizeWritten);
                    return false;
                }
                if (!this._Write('\0'))
                {
                    this.Reset(nPos, nSizeWritten);
                    return false;
                }
            }
            return true;
        }

        public bool Write(uint i, int iByteCnt = 4)
        {
            if (iByteCnt == 4)
            {
                return this._Write(i);
            }
            Debug.Assert(iByteCnt > 0);
            if ((i & this.INT_UNSIGNED_BITMASK[iByteCnt - 1]) != 0)
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write unsigned int out of range!(using ", iByteCnt, " bytes to write ", i, "%d)" }));
                return false;
            }
            return this._Write(i, iByteCnt);
        }

        public bool Write(ulong l, int iByteCnt = 8)
        {
            if (iByteCnt == 8)
            {
                return this._Write(l);
            }
            Debug.Assert(iByteCnt > 0);
            if ((l & this.LONG_UNSIGNED_BITMASK[iByteCnt - 1]) != 0L)
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_OUT_OF_RANGE;
                Log.LogErrorMsg(string.Concat(new object[] { "CMemoryStream::Write() Error: Write unsigned long out of range!(using ", iByteCnt, " bytes to write ", l, ")" }));
                return false;
            }
            return this._Write(l, iByteCnt);
        }

        public bool Write(ref CStream theStream, int iByteCnt = -1)
        {
            if (theStream == null)
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_NULL;
                return false;
            }
            Debug.Assert(theStream != null);
            if (iByteCnt < 0)
            {
                if (!this._Write(theStream.TotalSizeWritten()))
                {
                    return false;
                }
                return this._Write(theStream.m_byteData, (int)theStream.m_nBeginPos, (int)theStream.m_nSizeWritten);
            }
            Debug.Assert(theStream.m_nDataLength >= (theStream.m_nBeginPos + iByteCnt));
            return this._Write(theStream.m_byteData, (int)theStream.m_nBeginPos, iByteCnt);
        }

        public bool Write(ref CStream theStream, int iPos, int iByteCnt)
        {
            if (theStream == null)
            {
                this.m_eWriteFlag = EWriteFlag.VALUE_NULL;
                return false;
            }
            Debug.Assert(theStream != null);
            int iBegin = ((int)theStream.m_nBeginPos) + iPos;
            Debug.Assert(theStream.m_nDataLength >= (iBegin + iByteCnt));
            return this._Write(theStream.m_byteData, iBegin, iByteCnt);
        }

        public bool WriteBool(bool b)
        {
            return this.Write(b);
        }

        public bool WriteByte(byte by)
        {
            return this.Write(by);
        }

        public bool WriteChar(char c)
        {
            return this.Write(c);
        }

        public bool WriteDateTime(DateTime dt, int iByteCnt = 8)
        {
            return this.Write(dt, iByteCnt);
        }

        public bool WriteDouble(double d, int iByteCnt = 8)
        {
            return this.Write(d, iByteCnt);
        }

        public EWriteFlag WriteFlag()
        {
            return this.m_eWriteFlag;
        }

        public bool WriteFloat(float f, int iByteCnt = 4)
        {
            return this.Write(f, iByteCnt);
        }

        public bool WriteInt(int i, int iByteCnt = 4)
        {
            return this.Write(i, iByteCnt);
        }

        public bool WriteLong(long l, int iByteCnt = 8)
        {
            return this.Write(l, iByteCnt);
        }

        public bool WriteSByte(sbyte by)
        {
            return this.Write(by);
        }

        public bool WriteShort(short i)
        {
            return this.Write(i);
        }

        public bool WriteUInt(uint i, int iByteCnt = 4)
        {
            return this.Write(i, iByteCnt);
        }

        public bool WriteULong(ulong ul, int iByteCnt = 8)
        {
            return this.Write(ul, iByteCnt);
        }

        public bool WriteUShort(ushort i)
        {
            return this.Write(i);
        }

        public byte[] ByteData
        {
            get
            {
                return this.m_byteData;
            }
        }

        public enum EWriteFlag
        {
            NORMAL,
            VALUE_NULL,
            VALUE_OUT_OF_RANGE,
            WRITE_BUFFER_FULL,
            NOT_SUPPORT
        }
    }
}
