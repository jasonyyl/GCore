using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GCore
{
    public sealed class CTextParser : IDisposable
    {
        private bool m_bDisposed;
        private char m_cCacheChar;
        private char m_cNextLineFlag;
        private int m_iCurParsingLine;
        private List<string> m_listCachedTokens;
        private string m_sSeparatorChars;
        private string m_sTokenChars;
        private _ITextStream m_TextStream = null;
        private const int NUM_SEPARATOR_CHAR_SET = 4;
        private const int NUM_STOP_CHARS = 2;
        private static char[] s_cSeparatorCharSet = new char[] { ' ', '\n', '\r', '\t' };
        private static char[] s_cStopCharSet = new char[] { '\n', '\r' };

        public CTextParser(string sTokenCharSet = null, string sSeparatorCharSet = null)
        {
            if (sTokenCharSet != null)
            {
                this.m_sTokenChars = sTokenCharSet;
            }
            else
            {
                this.m_sTokenChars = "";
            }
            if (sSeparatorCharSet != null)
            {
                this.m_sSeparatorChars = sSeparatorCharSet;
            }
            else
            {
                this.m_sSeparatorChars = new string(s_cSeparatorCharSet);
            }
            this._RemoveTokenCharsFromSeparatorChars();
            this.m_listCachedTokens = new List<string>();
            this.m_iCurParsingLine = 0;
            this.m_cCacheChar = '\0';
            this.m_cNextLineFlag = '\0';
        }

        private bool _GetNextChar(out char ch)
        {
            if (this.m_cCacheChar != '\0')
            {
                ch = this.m_cCacheChar;
                this.m_cCacheChar = '\0';
                if ((ch == '\n') || (ch == '\r'))
                {
                    if ((this.m_cNextLineFlag == '\0') || (this.m_cNextLineFlag == ch))
                    {
                        this.m_iCurParsingLine++;
                        this.m_cNextLineFlag = ch;
                    }
                    else
                    {
                        this.m_cNextLineFlag = '\0';
                    }
                }
                else
                {
                    this.m_cNextLineFlag = '\0';
                }
                return true;
            }
            if (!this.m_TextStream.GetChar(out ch))
            {
                return false;
            }
            if ((ch == '\n') || (ch == '\r'))
            {
                if ((this.m_cNextLineFlag == '\0') || (this.m_cNextLineFlag == ch))
                {
                    this.m_iCurParsingLine++;
                    this.m_cNextLineFlag = ch;
                }
                else
                {
                    this.m_cNextLineFlag = '\0';
                }
            }
            else
            {
                this.m_cNextLineFlag = '\0';
            }
            return true;
        }

        private bool _IsSeparatorChar(char ch)
        {
            int length = this.m_sSeparatorChars.Length;
            for (int i = 0; i < length; i++)
            {
                if (ch == this.m_sSeparatorChars[i])
                {
                    return true;
                }
            }
            return false;
        }

        private bool _IsTokenChar(char ch)
        {
            int length = this.m_sTokenChars.Length;
            for (int i = 0; i < length; i++)
            {
                if (ch == this.m_sTokenChars[i])
                {
                    return true;
                }
            }
            return false;
        }

        private void _RemoveTokenCharsFromSeparatorChars()
        {
            string str = "";
            foreach (char ch in this.m_sSeparatorChars)
            {
                bool flag = false;
                foreach (char ch2 in this.m_sTokenChars)
                {
                    if (ch == ch2)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    str = str + ch;
                }
            }
            this.m_sSeparatorChars = str;
        }

        private void _UngetNextChar(char ch)
        {
            this.m_cCacheChar = ch;
            if ((ch == '\n') || (ch == '\r'))
            {
                if ((this.m_cNextLineFlag == '\0') || (this.m_cNextLineFlag == ch))
                {
                    this.m_iCurParsingLine--;
                    this.m_cNextLineFlag = ch;
                }
                else
                {
                    this.m_cNextLineFlag = '\0';
                }
            }
            else
            {
                this.m_cNextLineFlag = '\0';
            }
        }

        public void BindStream(CFile file)
        {
            if (this.m_TextStream != null)
            {
                this.m_TextStream.Close();
                this.m_TextStream = null;
            }
            this.m_iCurParsingLine = 0;
            this.m_TextStream = new _CFileStream(file);
        }

        public void BindStream(StreamReader file)
        {
            if (this.m_TextStream != null)
            {
                this.m_TextStream.Close();
                this.m_TextStream = null;
            }
            this.m_iCurParsingLine = 0;
            this.m_TextStream = new _CTextFileStream(file);
        }

        public void BindStream(string str)
        {
            if (this.m_TextStream != null)
            {
                this.m_TextStream.Close();
                this.m_TextStream = null;
            }
            this.m_iCurParsingLine = 0;
            this.m_TextStream = new _CStringStream(str);
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
                if (bDisposing && (this.m_TextStream != null))
                {
                    this.m_TextStream.Close();
                    this.m_TextStream = null;
                }
                this.m_bDisposed = true;
            }
        }

        ~CTextParser()
        {
            this.Dispose(false);
        }

        public int GetCurParsingLine()
        {
            return (this.m_iCurParsingLine + 1);
        }

        public bool GetNextQuotedToken(out string sToken, string sQuoteCharSet, bool bToUpper = false)
        {
            if (!this.GetNextToken(out sToken, false))
            {
                return false;
            }
            if (sToken.Length == 0)
            {
                return false;
            }
            int length = sQuoteCharSet.Length;
            for (int i = 0; i < length; i++)
            {
                if (sToken[0] == sQuoteCharSet[i])
                {
                    if (!this.ReadUntil(out sToken, sQuoteCharSet, false))
                    {
                        return false;
                    }
                    if (bToUpper)
                    {
                        sToken = sToken.ToUpper();
                    }
                    return true;
                }
            }
            return true;
        }

        public bool GetNextToken(out string sToken, bool bToUpper = false)
        {
            char ch;
            if (this.m_listCachedTokens.Count > 0)
            {
                int index = this.m_listCachedTokens.Count - 1;
                sToken = this.m_listCachedTokens[index];
                this.m_listCachedTokens.RemoveAt(index);
                if (bToUpper)
                {
                    sToken = sToken.ToUpper();
                }
                return true;
            }
            sToken = "";
            if (this.m_TextStream == null)
            {
                return false;
            }
            do
            {
                if (!this._GetNextChar(out ch))
                {
                    return false;
                }
            }
            while (this._IsSeparatorChar(ch));
            if (this._IsTokenChar(ch))
            {
                sToken = sToken + ch;
                if (bToUpper)
                {
                    sToken = sToken.ToUpper();
                }
                return true;
            }
            StringBuilder builder = new StringBuilder(8);
            builder.Append(ch);
            while (this._GetNextChar(out ch))
            {
                if (this._IsTokenChar(ch) || this._IsSeparatorChar(ch))
                {
                    this._UngetNextChar(ch);
                    break;
                }
                builder.Append(ch);
            }
            if (builder.Length <= 0)
            {
                return false;
            }
            sToken = builder.ToString();
            if (bToUpper)
            {
                sToken = sToken.ToUpper();
            }
            return true;
        }

        public bool HasMoreToken()
        {
            string str;
            if (!this.GetNextToken(out str, false))
            {
                return false;
            }
            this.UngetToken(str);
            return true;
        }

        public bool ReadLine(out string sLine)
        {
            char ch;
            sLine = "";
            if (this.m_TextStream == null)
            {
                return false;
            }
            if (!this._GetNextChar(out ch))
            {
                return false;
            }
            StringBuilder builder = new StringBuilder(8);
            do
            {
                if ((ch == s_cStopCharSet[0]) || (ch == s_cStopCharSet[1]))
                {
                    char ch2;
                    if (this._GetNextChar(out ch2))
                    {
                        if ((ch2 == s_cStopCharSet[0]) || (ch2 == s_cStopCharSet[1]))
                        {
                            if (ch2 == ch)
                            {
                                this._UngetNextChar(ch2);
                            }
                        }
                        else
                        {
                            this._UngetNextChar(ch2);
                        }
                        sLine = builder.ToString();
                    }
                    return true;
                }
                builder.Append(ch);
            }
            while (this._GetNextChar(out ch));
            sLine = builder.ToString();
            return true;
        }

        public bool ReadUntil(out string sString, char cStopChar, bool bUngetStopChar = true)
        {
            return this.ReadUntil(out sString, new string(cStopChar, 1), bUngetStopChar);
        }

        public bool ReadUntil(out string sString, string sStopCharSet, bool bUngetStopChar = true)
        {
            char ch;
            int num2;
            sString = "";
            if (this.m_TextStream == null)
            {
                return false;
            }
            if (!this._GetNextChar(out ch))
            {
                return false;
            }
            StringBuilder builder = new StringBuilder(8);
            if (sStopCharSet.Length == 0)
            {
                do
                {
                    builder.Append(ch);
                }
                while (this._GetNextChar(out ch));
                goto Label_009F;
            }
            int length = sStopCharSet.Length;
        Label_0053:
            num2 = 0;
            while (num2 < length)
            {
                if (ch == sStopCharSet[num2])
                {
                    break;
                }
                num2++;
            }
            if (num2 == length)
            {
                builder.Append(ch);
                if (!this._GetNextChar(out ch))
                {
                    sString = builder.ToString();
                    return false;
                }
                goto Label_0053;
            }
            if (bUngetStopChar)
            {
                this._UngetNextChar(ch);
            }
        Label_009F:
            sString = builder.ToString();
            return true;
        }

        public string SeparatorChars()
        {
            return this.m_sSeparatorChars;
        }

        public void SeparatorChars(string sChars)
        {
            this.m_sSeparatorChars = sChars;
            this._RemoveTokenCharsFromSeparatorChars();
        }

        public string TokenChars()
        {
            return this.m_sTokenChars;
        }

        public void TokenChars(string sChars)
        {
            this.m_sTokenChars = sChars;
            this._RemoveTokenCharsFromSeparatorChars();
        }

        public void UngetToken(string sToken)
        {
            this.m_listCachedTokens.Add(sToken);
        }

        private class _CFileStream : CTextParser._ITextStream
        {
            protected CFile m_File;

            public _CFileStream(CFile file)
            {
                Debug.Assert(file != null);
                this.m_File = file;
            }

            public override void Close()
            {
                this.m_File.Close(true);
            }

            public override bool GetChar(out char c)
            {
                if (this.m_File.IsEnd())
                {
                    c = (char)0xffff;
                    return false;
                }
                return (this.m_File.Read(out c) != 0);
            }
        }

        private class _CStringStream : CTextParser._ITextStream
        {
            protected int m_iIndex;
            protected string m_String;

            public _CStringStream(string s)
            {
                Debug.Assert(s != null);
                this.m_String = s;
                this.m_iIndex = 0;
            }

            public override void Close()
            {
            }

            public override bool GetChar(out char c)
            {
                if (this.m_iIndex < this.m_String.Length)
                {
                    c = this.m_String[this.m_iIndex++];
                    return true;
                }
                c = (char)0xffff;
                return false;
            }
        }

        private class _CTextFileStream : CTextParser._ITextStream
        {
            protected StreamReader m_File;

            public _CTextFileStream(StreamReader file)
            {
                Debug.Assert(file != null);
                this.m_File = file;
            }

            public override void Close()
            {
                this.m_File.Close();
            }

            public override bool GetChar(out char c)
            {
                if (this.m_File.EndOfStream)
                {
                    c = (char)0xffff;
                    return false;
                }
                int num = this.m_File.Read();
                c = (char)num;
                return true;
            }
        }

        private abstract class _ITextStream
        {
            public abstract void Close();
            public abstract bool GetChar(out char c);
        }
    }
}
