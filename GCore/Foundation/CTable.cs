using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

namespace GCore.Foundation
{
    public sealed class CTable : IDisposable
    {
        private bool[] m_abSchemaAttrRead = new bool[11];
        private readonly string[] m_asSchemaAttr = new string[] { "RUNTIMETYPE:", "DATATYPE:", "PRIMARYKEY:", "NOTNULL:", "UNIQUE:", "INDEX:", "BINARY:", "UNSIGNED:", "ZEROFILL:", "AUTOINCREMENT:", "DEFAULT:" };
        private bool m_bDisposed;
        private static byte[] m_byCryptSalt = new byte[] { 0x4a, 0x8f, 13, 0xe2, 0xe4, 0x7c, 0x92, 0x4b, 0x88, 2, 0xdb, 0xc7, 0x59, 0x63, 0x27, 0xc4 };
        private static CustomBindStreamFunction m_fnCustomBindStream;
        private int m_iNumDataRows;
        private int m_iNumSchemaAttrRead;
        private int m_iSchemaVersion;
        private List<CColumnInfo> m_listColumns = new List<CColumnInfo>();
        private static List<string> m_listTextExtSequences = new List<string> { ".csv", ".txt" };
        private string m_sBinaryExtName = ".bcs";
        private string m_sFilename = "";
        private string m_sTextExtName = m_listTextExtSequences[0];
        private const int NUM_SCHEMA_ATTRS = 11;

        private bool _CheckDataRowType<T2>(T2 data, FieldInfo[] aAvailableFields, bool bRuntimeTypeChecking)
        {
            int count = this.m_listColumns.Count;
            int length = aAvailableFields.Length;
            int num3 = 0;
            for (int i = 0; num3 < count; i++)
            {
                if (i >= length)
                {
                    Log.LogErrorMsg(string.Concat(new object[] { "CTable::_CheckDataRowType(): There's not enough Fields for table loading(", typeof(T2).ToString(), ":", length, " available fields)" }));
                    return false;
                }
                FieldInfo info = aAvailableFields[i];
                if (info.FieldType.IsArray)
                {
                    if (this.m_listColumns[num3].m_sName.Split(new char[] { '[' })[0] != info.Name)
                    {
                        Log.LogErrorMsg("CTable::_CheckDataRowType(): The column names of the Table and Structure(" + typeof(T2).ToString() + ") are not match - '" + this.m_listColumns[num3].m_sName + " != " + info.Name + "'");
                        return false;
                    }
                    object obj2 = info.GetValue(data);
                    if ((info.FieldType != typeof(int[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT32) && bRuntimeTypeChecking))
                    {
                        if ((info.FieldType != typeof(uint[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT32) && bRuntimeTypeChecking))
                        {
                            if ((info.FieldType != typeof(string[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_STRING) && bRuntimeTypeChecking))
                            {
                                if ((info.FieldType != typeof(long[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT64) && bRuntimeTypeChecking))
                                {
                                    if ((info.FieldType != typeof(ulong[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT64) && bRuntimeTypeChecking))
                                    {
                                        if ((info.FieldType != typeof(float[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_SINGLE) && bRuntimeTypeChecking))
                                        {
                                            if ((info.FieldType != typeof(double[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_DOUBLE) && bRuntimeTypeChecking))
                                            {
                                                if ((info.FieldType != typeof(short[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT16) && bRuntimeTypeChecking))
                                                {
                                                    if ((info.FieldType != typeof(ushort[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT16) && bRuntimeTypeChecking))
                                                    {
                                                        if ((info.FieldType != typeof(char[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_CHAR) && bRuntimeTypeChecking))
                                                        {
                                                            if ((info.FieldType != typeof(bool[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_BOOL) && bRuntimeTypeChecking))
                                                            {
                                                                if ((info.FieldType != typeof(DateTime[])) || (this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_DATETIME))
                                                                {
                                                                    if ((info.FieldType != typeof(byte[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_BYTE) && bRuntimeTypeChecking))
                                                                    {
                                                                        if ((info.FieldType != typeof(sbyte[])) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_SBYTE) && bRuntimeTypeChecking))
                                                                        {
                                                                            Log.LogErrorMsg(string.Concat(new object[] { "CTable::_CheckDataRowType(): column's runtime type not support in table(type", info.FieldType.ToString(), "<-->", this.m_listColumns[num3].m_eRuntimeType, ")" }));
                                                                            return false;
                                                                        }
                                                                        sbyte[] numArray9 = (sbyte[])obj2;
                                                                        num3 += numArray9.Length - 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        byte[] buffer = (byte[])obj2;
                                                                        num3 += buffer.Length - 1;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    DateTime[] timeArray = (DateTime[])obj2;
                                                                    num3 += timeArray.Length - 1;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                bool[] flagArray = (bool[])obj2;
                                                                num3 += flagArray.Length - 1;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            char[] chArray2 = (char[])obj2;
                                                            num3 += chArray2.Length - 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ushort[] numArray8 = (ushort[])obj2;
                                                        num3 += numArray8.Length - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    short[] numArray7 = (short[])obj2;
                                                    num3 += numArray7.Length - 1;
                                                }
                                            }
                                            else
                                            {
                                                double[] numArray6 = (double[])obj2;
                                                num3 += numArray6.Length - 1;
                                            }
                                        }
                                        else
                                        {
                                            float[] numArray5 = (float[])obj2;
                                            num3 += numArray5.Length - 1;
                                        }
                                    }
                                    else
                                    {
                                        ulong[] numArray4 = (ulong[])obj2;
                                        num3 += numArray4.Length - 1;
                                    }
                                }
                                else
                                {
                                    long[] numArray3 = (long[])obj2;
                                    num3 += numArray3.Length - 1;
                                }
                            }
                            else
                            {
                                string[] strArray3 = (string[])obj2;
                                num3 += strArray3.Length - 1;
                            }
                        }
                        else
                        {
                            uint[] numArray2 = (uint[])obj2;
                            num3 += numArray2.Length - 1;
                        }
                    }
                    else
                    {
                        int[] numArray = (int[])obj2;
                        num3 += numArray.Length - 1;
                    }
                }
                else
                {
                    if (this.m_listColumns[num3].m_sName != info.Name)
                    {
                        Log.LogErrorMsg("CTable::_CheckDataRowType(): The column names of the Table and Structure(" + typeof(T2).ToString() + ") are not match - '" + this.m_listColumns[num3].m_sName + " != " + info.Name + "'");
                        return false;
                    }
                    if ((((((info.FieldType != typeof(int)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT32) && bRuntimeTypeChecking)) && (!info.FieldType.IsEnum || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_ENUM) && bRuntimeTypeChecking))) && (((info.FieldType != typeof(uint)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT32) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(string)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_STRING) && bRuntimeTypeChecking)))) && ((((info.FieldType != typeof(long)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT64) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(ulong)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT64) && bRuntimeTypeChecking))) && (((info.FieldType != typeof(float)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_SINGLE) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(double)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_DOUBLE) && bRuntimeTypeChecking))))) && (((((info.FieldType != typeof(short)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_INT16) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(ushort)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_UINT16) && bRuntimeTypeChecking))) && (((info.FieldType != typeof(char)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_CHAR) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(bool)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_BOOL) && bRuntimeTypeChecking)))) && ((((info.FieldType != typeof(DateTime)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_DATETIME) && bRuntimeTypeChecking)) && ((info.FieldType != typeof(byte)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_BYTE) && bRuntimeTypeChecking))) && ((info.FieldType != typeof(sbyte)) || ((this.m_listColumns[num3].m_eRuntimeType != CColumnInfo.ERuntimeType.TYPE_SBYTE) && bRuntimeTypeChecking)))))
                    {
                        Log.LogErrorMsg(string.Concat(new object[] { "CTable::_CheckDataRowType(): column's runtime type not support in table(type:", info.FieldType.ToString(), "<-->", this.m_listColumns[num3].m_eRuntimeType, ")" }));
                        return false;
                    }
                }
                num3++;
            }
            return true;
        }

        private bool _DisplayCastingErrorMessage(CRowData row, int iIdx, CColumnInfo col)
        {
            if (row.m_asData[iIdx] == null)
            {
                Log.LogWarningMsg(string.Concat(new object[] { "Cast a null string of rowdata[", iIdx, "] to ", col.m_eRuntimeType, " ! (col name: ", col.m_sName, ")" }));
                return true;
            }
            Log.LogErrorMsg(string.Concat(new object[] { "Cast rowdata[", iIdx, "] = '", row.m_asData[iIdx], "' to ", col.m_eRuntimeType, " falied! (col name: ", col.m_sName, ")" }));
            return false;
        }

        private bool _ExportBinaryData(CFile file, CMapT<string, CRowData> mapRowData)
        {
            if (file.Write(this.m_iNumDataRows) == 0)
            {
                return false;
            }
            int count = this.m_listColumns.Count;
            foreach (CRowData data in mapRowData.Values)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!this._ExportToBinaryTypeData(file, data, i))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool _ExportBinarySchema(CFile file)
        {
            if (this.m_listColumns.Count == 0)
            {
                return false;
            }
            if (file.Write(this.m_iSchemaVersion) == 0)
            {
                return false;
            }
            if (file.Write(this.m_listColumns.Count) == 0)
            {
                return false;
            }
            foreach (CColumnInfo info in this.m_listColumns)
            {
                if (file.Write(info.m_sName) == 0)
                {
                    return false;
                }
                if (file.Write((int)info.m_eRuntimeType) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_sDatatype) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_sDefault) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bPrimaryKey) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bNotNull) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bUnique) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bIndex) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bBinary) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bUnsigned) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bZeroFill) == 0)
                {
                    return false;
                }
                if (file.Write(info.m_bAutoIncrement) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool _ExportData(StringBuilder sb, string sTheTableName, CMapT<string, CRowData> mapRowData)
        {
            if (mapRowData == null)
            {
                return false;
            }
            bool flag = true;
            foreach (string str in mapRowData.Keys)
            {
                CRowData data = mapRowData[str];
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append("\r\n");
                }
                sb.Append("INSERT INTO ");
                sb.Append(sTheTableName);
                sb.Append(" (");
                bool flag2 = true;
                for (int i = 0; i < this.m_listColumns.Count; i++)
                {
                    if (flag2)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append("`");
                    sb.Append(this.m_listColumns[i].m_sName);
                    sb.Append("`");
                }
                sb.Append(") VALUES (");
                flag2 = true;
                for (int j = 0; j < data.m_asData.Length; j++)
                {
                    if (flag2)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append("\"");
                    sb.Append(data.m_asData[j]);
                    sb.Append("\"");
                }
                sb.Append(");");
            }
            return true;
        }

        private bool _ExportSchema(StringBuilder sb, string sTheTableName)
        {
            if (this.m_sFilename.Length == 0)
            {
                return false;
            }
            sb.Append("CREATE TABLE ");
            sb.Append(sTheTableName);
            sb.Append(" (\r\n  ");
            List<int> list = new List<int>();
            List<int> list2 = new List<int>();
            List<int> list3 = new List<int>();
            bool flag = true;
            for (int i = 0; i < this.m_listColumns.Count; i++)
            {
                CColumnInfo info = this.m_listColumns[i];
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(",\r\n  ");
                }
                if (info.m_bPrimaryKey)
                {
                    list.Add(i);
                }
                if (info.m_bUnique)
                {
                    list2.Add(i);
                }
                else if (info.m_bIndex)
                {
                    list3.Add(i);
                }
                sb.Append("`");
                sb.Append(info.m_sName);
                sb.Append("` ");
                sb.Append(info.m_sDatatype);
                if (info.m_bUnsigned)
                {
                    sb.Append(" UNSIGNED");
                }
                if (info.m_bZeroFill)
                {
                    sb.Append(" ZEROFILL");
                }
                if (info.m_bBinary)
                {
                    sb.Append(" BINARY");
                    if (info.m_bNotNull)
                    {
                        sb.Append(" NOT NULL");
                        if (info.m_sDefault.Length > 0)
                        {
                            sb.Append(" DEFAULT ");
                            sb.Append(info.m_sDefault);
                        }
                    }
                    else
                    {
                        sb.Append(" NULL");
                    }
                }
                else if (info.m_bNotNull)
                {
                    sb.Append(" NOT NULL");
                    if (info.m_sDefault.Length > 0)
                    {
                        sb.Append(" DEFAULT ");
                        sb.Append(info.m_sDefault);
                    }
                }
                else
                {
                    sb.Append(" DEFAULT NULL");
                }
                if (info.m_bAutoIncrement)
                {
                    sb.Append(" AUTO_INCREMENT");
                }
            }
            if (list.Count > 0)
            {
                sb.Append(",\r\n  ");
                sb.Append("PRIMARY KEY (");
                bool flag2 = true;
                foreach (int num2 in list)
                {
                    if (flag2)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append("`");
                    CColumnInfo info2 = this.m_listColumns[num2];
                    sb.Append(info2.m_sName);
                    sb.Append("`");
                }
                sb.Append(")");
            }
            if (list2.Count > 0)
            {
                foreach (int num3 in list2)
                {
                    sb.Append(",\r\n  ");
                    sb.Append("UNIQUE KEY `");
                    CColumnInfo info3 = this.m_listColumns[num3];
                    sb.Append(info3.m_sName);
                    sb.Append("_UNIQUE` (`");
                    sb.Append(info3.m_sName);
                    sb.Append("`)");
                }
            }
            if (list3.Count > 0)
            {
                foreach (int num4 in list3)
                {
                    sb.Append(",\r\n  ");
                    sb.Append("KEY `");
                    CColumnInfo info4 = this.m_listColumns[num4];
                    sb.Append(info4.m_sName);
                    sb.Append("_INDEX` (`");
                    sb.Append(info4.m_sName);
                    sb.Append("`)");
                }
            }
            sb.Append("\r\n) ENGINE=InnoDB DEFAULT CHARSET=utf8;");
            return true;
        }

        private bool _ExportToBinaryTypeData(CFile file, CRowData row, int iIdx)
        {
            CColumnInfo col = this.m_listColumns[iIdx];
            if ((col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT32) || (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_ENUM))
            {
                int num;
                if (!int.TryParse(row.m_asData[iIdx], out num) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT32)
            {
                uint num2;
                if (!uint.TryParse(row.m_asData[iIdx], out num2) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num2) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_SINGLE)
            {
                float num3;
                if (!float.TryParse(row.m_asData[iIdx], out num3) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num3) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_DOUBLE)
            {
                double num4;
                if (!double.TryParse(row.m_asData[iIdx], out num4) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num4) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_STRING)
            {
                if (file.Write(row.m_asData[iIdx]) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_BOOL)
            {
                bool flag;
                if (!bool.TryParse(row.m_asData[iIdx], out flag) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(flag) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_BYTE)
            {
                byte num5;
                if (!byte.TryParse(row.m_asData[iIdx], out num5) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num5) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_SBYTE)
            {
                sbyte num6;
                if (!sbyte.TryParse(row.m_asData[iIdx], out num6) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num6) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_CHAR)
            {
                char ch;
                if (!char.TryParse(row.m_asData[iIdx], out ch) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(ch) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT16)
            {
                short num7;
                if (!short.TryParse(row.m_asData[iIdx], out num7) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num7) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT16)
            {
                ushort num8;
                if (!ushort.TryParse(row.m_asData[iIdx], out num8) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num8) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT64)
            {
                long num9;
                if (!long.TryParse(row.m_asData[iIdx], out num9) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num9) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT64)
            {
                ulong num10;
                if (!ulong.TryParse(row.m_asData[iIdx], out num10) && !this._DisplayCastingErrorMessage(row, iIdx, col))
                {
                    return false;
                }
                if (file.Write(num10) == 0)
                {
                    return false;
                }
            }
            else if (col.m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_DATETIME)
            {
                if (file.Write(row.m_asData[iIdx]) == 0)
                {
                    return false;
                }
            }
            else
            {
                Log.LogErrorMsg(string.Concat(new object[] { "Type casting not support! rowdata[", iIdx, "] = '", row.m_asData[iIdx], "' to ", col.m_eRuntimeType, " falied! (col name: ", col.m_sName, ")" }));
                return false;
            }
            return true;
        }

        private bool _LoadBinaryData<T1, T2>(CFile file, CMapT<T1, T2> mapTable, int iKeyColumnIdx) where T2 : new()
        {
            if (file.Read(out this.m_iNumDataRows) == 0)
            {
                return false;
            }
            T2 data = (default(T2) == null) ? Activator.CreateInstance<T2>() : default(T2);
            FieldInfo[] fields = data.GetType().GetFields();
            int num = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsStatic && !fields[i].IsLiteral)
                {
                    num++;
                }
            }
            FieldInfo[] aAvailableFields = new FieldInfo[num];
            num = 0;
            for (int j = 0; j < fields.Length; j++)
            {
                if (!fields[j].IsStatic && !fields[j].IsLiteral)
                {
                    aAvailableFields[num++] = fields[j];
                }
            }
            if (!this._CheckDataRowType<T2>(data, aAvailableFields, true))
            {
                return false;
            }
            for (int k = 0; k < this.m_iNumDataRows; k++)
            {
                if (!this._LoadBinaryDataRow<T1, T2>(file, aAvailableFields, mapTable, iKeyColumnIdx))
                {
                    return false;
                }
            }
            return true;
        }

        private bool _LoadBinaryDataRow<T1, T2>(CFile file, FieldInfo[] aAvailableFields, CMapT<T1, T2> mapTable, int iKeyColumnIdx) where T2 : new()
        {
            T2 local = default(T2);
            T2 local2 = (local == null) ? Activator.CreateInstance<T2>() : default(T2);
            int count = this.m_listColumns.Count;
            int length = aAvailableFields.Length;
            int num3 = 0;
            for (int i = 0; num3 < count; i++)
            {
                FieldInfo info = aAvailableFields[i];
                if (info.FieldType.IsArray)
                {
                    try
                    {
                        object obj2 = info.GetValue(local2);
                        if (info.FieldType == typeof(int[]))
                        {
                            int[] numArray = (int[])obj2;
                            for (int j = 0; j < numArray.Length; j++)
                            {
                                int num6;
                                if (file.Read(out num6) == 0)
                                {
                                    return false;
                                }
                                numArray[j] = num6;
                            }
                            num3 += numArray.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(uint[]))
                        {
                            uint[] numArray2 = (uint[])obj2;
                            for (int k = 0; k < numArray2.Length; k++)
                            {
                                uint num8;
                                if (file.Read(out num8) == 0)
                                {
                                    return false;
                                }
                                numArray2[k] = num8;
                            }
                            num3 += numArray2.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(string[]))
                        {
                            string[] strArray = (string[])obj2;
                            for (int m = 0; m < strArray.Length; m++)
                            {
                                string str;
                                if (file.Read(out str) == 0)
                                {
                                    return false;
                                }
                                strArray[m] = str;
                            }
                            num3 += strArray.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(long[]))
                        {
                            long[] numArray3 = (long[])obj2;
                            for (int n = 0; n < numArray3.Length; n++)
                            {
                                long num11;
                                if (file.Read(out num11) == 0)
                                {
                                    return false;
                                }
                                numArray3[n] = num11;
                            }
                            num3 += numArray3.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(ulong[]))
                        {
                            ulong[] numArray4 = (ulong[])obj2;
                            for (int num12 = 0; num12 < numArray4.Length; num12++)
                            {
                                ulong num13;
                                if (file.Read(out num13) == 0)
                                {
                                    return false;
                                }
                                numArray4[num12] = num13;
                            }
                            num3 += numArray4.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(float[]))
                        {
                            float[] numArray5 = (float[])obj2;
                            for (int num14 = 0; num14 < numArray5.Length; num14++)
                            {
                                float num15;
                                if (file.Read(out num15) == 0)
                                {
                                    return false;
                                }
                                numArray5[num14] = num15;
                            }
                            num3 += numArray5.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(double[]))
                        {
                            double[] numArray6 = (double[])obj2;
                            for (int num16 = 0; num16 < numArray6.Length; num16++)
                            {
                                double num17;
                                if (file.Read(out num17) == 0)
                                {
                                    return false;
                                }
                                numArray6[num16] = num17;
                            }
                            num3 += numArray6.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(short[]))
                        {
                            short[] numArray7 = (short[])obj2;
                            for (int num18 = 0; num18 < numArray7.Length; num18++)
                            {
                                short num19;
                                if (file.Read(out num19) == 0)
                                {
                                    return false;
                                }
                                numArray7[num18] = num19;
                            }
                            num3 += numArray7.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(ushort[]))
                        {
                            ushort[] numArray8 = (ushort[])obj2;
                            for (int num20 = 0; num20 < numArray8.Length; num20++)
                            {
                                ushort num21;
                                if (file.Read(out num21) == 0)
                                {
                                    return false;
                                }
                                numArray8[num20] = num21;
                            }
                            num3 += numArray8.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(char[]))
                        {
                            char[] chArray = (char[])obj2;
                            for (int num22 = 0; num22 < chArray.Length; num22++)
                            {
                                char ch;
                                if (file.Read(out ch) == 0)
                                {
                                    return false;
                                }
                                chArray[num22] = ch;
                            }
                            num3 += chArray.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(bool[]))
                        {
                            bool[] flagArray = (bool[])obj2;
                            for (int num23 = 0; num23 < flagArray.Length; num23++)
                            {
                                bool flag2;
                                if (file.Read(out flag2) == 0)
                                {
                                    return false;
                                }
                                flagArray[num23] = flag2;
                            }
                            num3 += flagArray.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(DateTime[]))
                        {
                            DateTime[] timeArray = (DateTime[])obj2;
                            for (int num24 = 0; num24 < timeArray.Length; num24++)
                            {
                                string str2;
                                if (file.Read(out str2) == 0)
                                {
                                    return false;
                                }
                                timeArray[num24] = CTime.GetDateTimeFromString(str2, '-', ' ', ':');
                            }
                            num3 += timeArray.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(byte[]))
                        {
                            byte[] buffer = (byte[])obj2;
                            for (int num25 = 0; num25 < buffer.Length; num25++)
                            {
                                byte num26;
                                if (file.Read(out num26) == 0)
                                {
                                    return false;
                                }
                                buffer[num25] = num26;
                            }
                            num3 += buffer.Length - 1;
                            goto Label_0AF5;
                        }
                        if (info.FieldType == typeof(sbyte[]))
                        {
                            sbyte[] numArray9 = (sbyte[])obj2;
                            for (int num27 = 0; num27 < numArray9.Length; num27++)
                            {
                                sbyte num28;
                                if (file.Read(out num28) == 0)
                                {
                                    return false;
                                }
                                numArray9[num27] = num28;
                            }
                            num3 += numArray9.Length - 1;
                            goto Label_0AF5;
                        }
                        Log.LogErrorMsg("column's runtime type not support in table(type" + info.FieldType.ToString() + ")");
                        return false;
                    }
                    catch (Exception exception)
                    {
                        Log.LogErrorMsg("data type convertion failed in table while converting: " + info.FieldType.ToString() + "(" + exception.ToString() + ")");
                        return false;
                    }
                }
                try
                {
                    if ((this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT32) || (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_ENUM))
                    {
                        int num29;
                        if (file.Read(out num29) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num29);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT32)
                    {
                        uint num30;
                        if (file.Read(out num30) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num30);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_STRING)
                    {
                        string str3;
                        if (file.Read(out str3) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, str3);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT64)
                    {
                        long num31;
                        if (file.Read(out num31) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num31);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT64)
                    {
                        ulong num32;
                        if (file.Read(out num32) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num32);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_SINGLE)
                    {
                        float num33;
                        if (file.Read(out num33) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num33);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_DOUBLE)
                    {
                        double num34;
                        if (file.Read(out num34) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num34);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_INT16)
                    {
                        short num35;
                        if (file.Read(out num35) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num35);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_UINT16)
                    {
                        ushort num36;
                        if (file.Read(out num36) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num36);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_CHAR)
                    {
                        char ch2;
                        if (file.Read(out ch2) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, ch2);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_BOOL)
                    {
                        bool flag3;
                        if (file.Read(out flag3) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, flag3);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_DATETIME)
                    {
                        string str4;
                        if (file.Read(out str4) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, CTime.GetDateTimeFromString(str4, '-', ' ', ':'));
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_BYTE)
                    {
                        byte num37;
                        if (file.Read(out num37) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num37);
                    }
                    else if (this.m_listColumns[num3].m_eRuntimeType == CColumnInfo.ERuntimeType.TYPE_SBYTE)
                    {
                        sbyte num38;
                        if (file.Read(out num38) == 0)
                        {
                            return false;
                        }
                        info.SetValue(local2, num38);
                    }
                    else
                    {
                        Log.LogErrorMsg("column's runtime type not support in table(type:" + info.FieldType.ToString() + ")");
                        return false;
                    }
                }
                catch (Exception exception2)
                {
                    Log.LogErrorMsg("data type convertion failed in table while converting: " + info.FieldType.ToString() + "(" + exception2.ToString() + ")");
                    return false;
                }
            Label_0AF5:
                num3++;
            }
            if (iKeyColumnIdx >= length)
            {
                Log.LogErrorMsg(string.Concat(new object[] { "CTable::_LoadBinaryDataRow(): iKeyColumnIdx(", iKeyColumnIdx, ") out of available Field numbers(", length, ")" }));
                return false;
            }
            T1 key = (T1)aAvailableFields[iKeyColumnIdx].GetValue(local2);
            mapTable.Add(key, local2);
            return true;
        }

        private bool _LoadBinarySchema(CFile file)
        {
            int num;
            this.m_listColumns.Clear();
            if (file.Read(out this.m_iSchemaVersion) == 0)
            {
                return false;
            }
            if (file.Read(out num) == 0)
            {
                return false;
            }
            for (int i = 0; i < num; i++)
            {
                int num3;
                CColumnInfo item = new CColumnInfo();
                if (file.Read(out item.m_sName) == 0)
                {
                    return false;
                }
                if (file.Read(out num3) == 0)
                {
                    return false;
                }
                item.m_eRuntimeType = (CColumnInfo.ERuntimeType)num3;
                if (file.Read(out item.m_sDatatype) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_sDefault) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bPrimaryKey) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bNotNull) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bUnique) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bIndex) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bBinary) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bUnsigned) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bZeroFill) == 0)
                {
                    return false;
                }
                if (file.Read(out item.m_bAutoIncrement) == 0)
                {
                    return false;
                }
                this.m_listColumns.Add(item);
            }
            return true;
        }

        private bool _LoadData<T1, T2>(CTextParser parser, CMapT<T1, T2> mapTable, int iKeyColumnIdx) where T2 : new()
        {
            string[] asRows = new string[this.m_listColumns.Count];
            bool flag = false;
            int num = 0;
            int num2 = 0;
            bool bRowDataAssign = false;
            if (typeof(T2) == typeof(CRowData))
            {
                bRowDataAssign = true;
            }
            T2 data = (default(T2) == null) ? Activator.CreateInstance<T2>() : default(T2);
            FieldInfo[] fields = data.GetType().GetFields();
            int num3 = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsStatic && !fields[i].IsLiteral)
                {
                    num3++;
                }
            }
            FieldInfo[] aAvailableFields = new FieldInfo[num3];
            num3 = 0;
            for (int j = 0; j < fields.Length; j++)
            {
                if (!fields[j].IsStatic && !fields[j].IsLiteral)
                {
                    aAvailableFields[num3++] = fields[j];
                }
            }
            if (bRowDataAssign || this._CheckDataRowType<T2>(data, aAvailableFields, this.m_abSchemaAttrRead[0]))
            {
                string str;
            Label_02DA:
                if (parser.GetNextQuotedToken(out str, "\"", false))
                {
                    if (!flag)
                    {
                        if (str.ToUpper() == "DATABEGIN:")
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        if (str.ToUpper() == "DATAEND:")
                        {
                            goto Label_02ED;
                        }
                        switch (str)
                        {
                        case "\r":
                        case "\n":
                            if (num != 0)
                            {
                                if (num != this.m_listColumns.Count)
                                {
                                    Log.LogErrorMsg(string.Concat(new object[] { "CTable::_LoadData(): # of columns read(", num, ") are not match with the schema column(", this.m_listColumns.Count, ")" }));
                                    return false;
                                }
                                if (!this._SetDataRow<T1, T2>(asRows, aAvailableFields, mapTable, iKeyColumnIdx, bRowDataAssign))
                                {
                                    return false;
                                }
                                for (int k = 0; k < asRows.Length; k++)
                                {
                                    asRows[k] = null;
                                }
                                num2++;
                                num = 0;
                            }
                            goto Label_02DA;

                        case ",":
                            if (num == this.m_listColumns.Count)
                            {
                                if (!parser.ReadLine(out str))
                                {
                                    Log.LogErrorMsg("CTable::_LoadData(): Read line failed!");
                                    return false;
                                }
                                if (!this._SetDataRow<T1, T2>(asRows, aAvailableFields, mapTable, iKeyColumnIdx, bRowDataAssign))
                                {
                                    return false;
                                }
                                for (int m = 0; m < asRows.Length; m++)
                                {
                                    asRows[m] = null;
                                }
                                num2++;
                                num = 0;
                            }
                            else
                            {
                                num++;
                            }
                            goto Label_02DA;
                        }
                        if (num > 0)
                        {
                            if (num > asRows.Length)
                            {
                                Log.LogErrorMsg("CTable::_LoadData(): Column counts out of schema definition when parsing data section");
                                return false;
                            }
                            asRows[num - 1] = str;
                        }
                    }
                    goto Label_02DA;
                }
            }
            else
            {
                return false;
            }
        Label_02ED:
            return true;
        }

        private bool _LoadFileBinary<T1, T2>(string sFilename, CMapT<T1, T2> mapTable, int iKeyColumnIdx, bool bSuppressFailMsg) where T2 : new()
        {
            uint num;
            CFile file = new CFile();
            file.SetCryptSalt(m_byCryptSalt);
            if (!file.Open(sFilename, 0x26, true))
            {
                if (!bSuppressFailMsg)
                {
                    Log.LogWarningMsg("CTable::_LoadFileBinary(): Open file: " + sFilename + " failed!");
                }
                return false;
            }
            if (!file.ReadHeader("BCS", CPath.Name(sFilename), 1, out num, true))
            {
                Log.LogErrorMsg("CTable::_LoadFileBinary(): Read header failed!");
                return false;
            }
            if (!this._LoadBinarySchema(file))
            {
                file.Dispose();
                return false;
            }
            if (!this._LoadBinaryData<T1, T2>(file, mapTable, iKeyColumnIdx))
            {
                file.Dispose();
                return false;
            }
            file.Dispose();
            this.m_sFilename = sFilename;
            return true;
        }

        private bool _LoadFileText<T1, T2>(string sFilename, CMapT<T1, T2> mapTable, int iKeyColumnIdx, bool bSuppressFailMsg) where T2 : new()
        {
            if (m_fnCustomBindStream != null)
            {
                CTextParser parser = new CTextParser("\r\n\",", null);
                if (m_fnCustomBindStream(sFilename, ref parser))
                {
                    if (!this._LoadSchema(parser))
                    {
                        return false;
                    }
                    if (!this._LoadData<T1, T2>(parser, mapTable, iKeyColumnIdx))
                    {
                        return false;
                    }
                    return true;
                }
            }
            CFile file = new CFile();
            if (!file.Open(sFilename, 0x25, true))
            {
                if (!bSuppressFailMsg)
                {
                    Log.LogWarningMsg("CTable::_LoadFileText(): Open file: " + sFilename + " failed!");
                }
                return false;
            }
            CTextParser parser2 = new CTextParser("\r\n\",", null);
            parser2.BindStream(file);
            if (!this._LoadSchema(parser2))
            {
                file.Dispose();
                return false;
            }
            if (!this._LoadData<T1, T2>(parser2, mapTable, iKeyColumnIdx))
            {
                file.Dispose();
                return false;
            }
            file.Dispose();
            this.m_sFilename = sFilename;
            return true;
        }

        private bool _LoadSchema(CTextParser parser)
        {
            string str;
            bool flag = false;
            while (parser.GetNextQuotedToken(out str, "\"", false))
            {
                if (!flag)
                {
                    if (str.ToUpper() == "SCHEMAVERSION:")
                    {
                        if (parser.GetNextQuotedToken(out str, "\"", false) && (str != ","))
                        {
                            Log.LogErrorMsg("CTable::_LoadSchema(): Version # format error!");
                            return false;
                        }
                        if (parser.GetNextQuotedToken(out str, "\"", false))
                        {
                            this.m_iSchemaVersion = Convert.ToInt32(str);
                            flag = true;
                        }
                        else
                        {
                            Log.LogErrorMsg("CTable::_LoadSchema(): Version # not specified!");
                            return false;
                        }
                        if (!parser.ReadLine(out str))
                        {
                            Log.LogErrorMsg("CTable::_LoadSchema(): Read line failed when parsing - SCHEMAVERSION:");
                            return false;
                        }
                    }
                    continue;
                }
                if (str.ToUpper() == "NUMDATAROWS:")
                {
                    if (parser.GetNextQuotedToken(out str, "\"", false) && (str != ","))
                    {
                        Log.LogErrorMsg("CTable::_LoadSchema(): NumDataRows # format error!");
                        return false;
                    }
                    if (parser.GetNextQuotedToken(out str, "\"", false))
                    {
                        this.m_iNumDataRows = Convert.ToInt32(str);
                    }
                    else
                    {
                        Log.LogErrorMsg("CTable::_LoadSchema(): NumDataRows # not specified!");
                        return false;
                    }
                    if (!parser.ReadLine(out str))
                    {
                        Log.LogErrorMsg("CTable::_LoadSchema(): Read line failed when parsing - NUMDATAROWS:");
                        return false;
                    }
                    continue;
                }
                if (str.ToUpper() == "COLUMN:")
                {
                    string str2 = "";
                    while (parser.GetNextQuotedToken(out str, "\"", false))
                    {
                        switch (str)
                        {
                        case ",":
                            {
                                if (str2 == ",")
                                {
                                    if (!parser.ReadLine(out str))
                                    {
                                        Log.LogErrorMsg("CTable::_LoadSchema(): Read line failed when parsing - COLUMN:");
                                        return false;
                                    }
                                    goto GONEXT;
                                    //continue;
                                }
                                str2 = str;
                                continue;
                            }
                        case "\r":
                        case "\n":
                            {
                                goto GONEXT;
                                //continue;
                            }
                        }
                        CColumnInfo item = new CColumnInfo
                        {
                            m_sName = str
                        };
                        this.m_listColumns.Add(item);
                        str2 = str;
                    }
                GONEXT:
                    continue;
                }
                if ((str != "\r") && (str != "\n"))
                {
                    if (!this._ParseSchemaAttr(str.ToUpper(), parser))
                    {
                        return false;
                    }
                    if (this.m_iNumSchemaAttrRead >= 11)
                    {
                        break;
                    }
                    if (this.m_iNumSchemaAttrRead >= 10)
                    {
                        if (this.m_abSchemaAttrRead[5] && this.m_abSchemaAttrRead[0])
                        {
                            continue;
                        }
                        break;
                    }
                    if (((this.m_iNumSchemaAttrRead >= 9) && !this.m_abSchemaAttrRead[5]) && !this.m_abSchemaAttrRead[0])
                    {
                        break;
                    }
                }
            }
            return true;
        }

        private bool _ParseSchemaAttr(string sLabelUpper, CTextParser parser)
        {
            string str2;
            int index = 0;
            while (index < 11)
            {
                string str = this.m_asSchemaAttr[index];
                if (sLabelUpper == str)
                {
                    if (!this.m_abSchemaAttrRead[index])
                    {
                        this.m_iNumSchemaAttrRead++;
                    }
                    this.m_abSchemaAttrRead[index] = true;
                    break;
                }
                index++;
            }
            if (index == 11)
            {
                Log.LogErrorMsg("CTable::_ParseSchemaAttr(): Cannot handle Schema Attr - " + sLabelUpper);
                return false;
            }
            int num2 = -1;
            while (parser.GetNextQuotedToken(out str2, "\"", false))
            {
                if (str2 == ",")
                {
                    num2++;
                    if (num2 >= this.m_listColumns.Count)
                    {
                        if (!parser.ReadLine(out str2))
                        {
                            Log.LogErrorMsg("CTable::_ParseSchemaAttr(): Read line failed when parsing - " + sLabelUpper);
                            return false;
                        }
                        break;
                    }
                }
                else
                {
                    if ((str2 == "\r") || (str2 == "\n"))
                    {
                        break;
                    }
                    if ((num2 < 0) || (num2 >= this.m_listColumns.Count))
                    {
                        Log.LogErrorMsg("CTable::_ParseSchemaAttr(): Column counts out of schema definition when parsing - " + sLabelUpper);
                        return false;
                    }
                    CColumnInfo info = this.m_listColumns[num2];
                    if (sLabelUpper == "RUNTIMETYPE:")
                    {
                        string str3 = str2.ToUpper();
                        if (str3 == "INT")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_INT32;
                        }
                        else if (str3 == "UINT")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_UINT32;
                        }
                        else if (str3 == "FLOAT")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_SINGLE;
                        }
                        else if (str3 == "DOUBLE")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_DOUBLE;
                        }
                        else if (str3 == "STRING")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_STRING;
                        }
                        else if (str3 == "ENUM")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_ENUM;
                        }
                        else if (str3 == "BOOL")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_BOOL;
                        }
                        else if (str3 == "BYTE")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_BYTE;
                        }
                        else if (str3 == "SBYTE")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_SBYTE;
                        }
                        else if (str3 == "CHAR")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_CHAR;
                        }
                        else if (str3 == "SHORT")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_INT16;
                        }
                        else if (str3 == "USHORT")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_UINT16;
                        }
                        else if (str3 == "LONG")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_INT64;
                        }
                        else if (str3 == "ULONG")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_UINT64;
                        }
                        else if (str3 == "DATETIME")
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_DATETIME;
                        }
                        else
                        {
                            info.m_eRuntimeType = CColumnInfo.ERuntimeType.TYPE_UNDEFINED;
                        }
                        continue;
                    }
                    if (sLabelUpper == "DATATYPE:")
                    {
                        info.m_sDatatype = str2;
                    }
                    else
                    {
                        if (sLabelUpper == "PRIMARYKEY:")
                        {
                            bool.TryParse(str2, out info.m_bPrimaryKey);
                            continue;
                        }
                        if (sLabelUpper == "NOTNULL:")
                        {
                            bool.TryParse(str2, out info.m_bNotNull);
                            continue;
                        }
                        if (sLabelUpper == "UNIQUE:")
                        {
                            bool.TryParse(str2, out info.m_bUnique);
                            continue;
                        }
                        if (sLabelUpper == "INDEX:")
                        {
                            bool.TryParse(str2, out info.m_bIndex);
                            continue;
                        }
                        if (sLabelUpper == "BINARY:")
                        {
                            bool.TryParse(str2, out info.m_bBinary);
                            continue;
                        }
                        if (sLabelUpper == "UNSIGNED:")
                        {
                            bool.TryParse(str2, out info.m_bUnsigned);
                            continue;
                        }
                        if (sLabelUpper == "ZEROFILL:")
                        {
                            bool.TryParse(str2, out info.m_bZeroFill);
                            continue;
                        }
                        if (sLabelUpper == "AUTOINCREMENT:")
                        {
                            bool.TryParse(str2, out info.m_bAutoIncrement);
                            continue;
                        }
                        if (sLabelUpper == "DEFAULT:")
                        {
                            info.m_sDefault = str2;
                        }
                    }
                }
            }
            return true;
        }

        private bool _SetDataRow<T1, T2>(string[] asRows, FieldInfo[] aAvailableFields, CMapT<T1, T2> mapTable, int iKeyColumnIdx, bool bRowDataAssign) where T2 : new()
        {
            if (bRowDataAssign)
            {
                CRowData data = new CRowData
                {
                    m_asData = new string[asRows.Length]
                };
                for (int j = 0; j < asRows.Length; j++)
                {
                    data.m_asData[j] = asRows[j];
                }
                if (iKeyColumnIdx < asRows.Length)
                {
                    if(mapTable is CMapT<string, CRowData>)
                    {
                        var realMapTable = mapTable as CMapT<string, CRowData>;
                        realMapTable.Add(data.m_asData[iKeyColumnIdx], data);
                        return true;
                    }
                }
                return false;
            }
            int count = this.m_listColumns.Count;
            T2 local3 = (default(T2) == null) ? Activator.CreateInstance<T2>() : default(T2);
            int length = aAvailableFields.Length;
            int index = 0;
            for (int i = 0; index < count; i++)
            {
                FieldInfo info = aAvailableFields[i];
                if (info.FieldType.IsArray)
                {
                    try
                    {
                        object obj2 = info.GetValue(local3);
                        if (info.FieldType == typeof(string[]))
                        {
                            string[] strArray = (string[])obj2;
                            for (int k = 0; k < strArray.Length; k++)
                            {
                                strArray[k] = asRows[index++];
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(int[]))
                        {
                            int[] numArray = (int[])obj2;
                            for (int m = 0; m < numArray.Length; m++)
                            {
                                numArray[m] = Convert.ToInt32(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(uint[]))
                        {
                            uint[] numArray2 = (uint[])obj2;
                            for (int n = 0; n < numArray2.Length; n++)
                            {
                                numArray2[n] = Convert.ToUInt32(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(long[]))
                        {
                            long[] numArray3 = (long[])obj2;
                            for (int num9 = 0; num9 < numArray3.Length; num9++)
                            {
                                numArray3[num9] = Convert.ToInt64(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(ulong[]))
                        {
                            ulong[] numArray4 = (ulong[])obj2;
                            for (int num10 = 0; num10 < numArray4.Length; num10++)
                            {
                                numArray4[num10] = Convert.ToUInt64(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(float[]))
                        {
                            float[] numArray5 = (float[])obj2;
                            for (int num11 = 0; num11 < numArray5.Length; num11++)
                            {
                                numArray5[num11] = Convert.ToSingle(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(double[]))
                        {
                            double[] numArray6 = (double[])obj2;
                            for (int num12 = 0; num12 < numArray6.Length; num12++)
                            {
                                numArray6[num12] = Convert.ToDouble(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(short[]))
                        {
                            short[] numArray7 = (short[])obj2;
                            for (int num13 = 0; num13 < numArray7.Length; num13++)
                            {
                                numArray7[num13] = Convert.ToInt16(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(ushort[]))
                        {
                            ushort[] numArray8 = (ushort[])obj2;
                            for (int num14 = 0; num14 < numArray8.Length; num14++)
                            {
                                numArray8[num14] = Convert.ToUInt16(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(char[]))
                        {
                            char[] chArray = (char[])obj2;
                            for (int num15 = 0; num15 < chArray.Length; num15++)
                            {
                                chArray[num15] = Convert.ToChar(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(bool[]))
                        {
                            bool[] flagArray = (bool[])obj2;
                            for (int num16 = 0; num16 < flagArray.Length; num16++)
                            {
                                bool flag;
                                if (!bool.TryParse(asRows[index], out flag))
                                {
                                    flag = Convert.ToInt32(asRows[index]) != 0;
                                }
                                index++;
                                flagArray[num16] = flag;
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(DateTime[]))
                        {
                            DateTime[] timeArray = (DateTime[])obj2;
                            for (int num17 = 0; num17 < timeArray.Length; num17++)
                            {
                                timeArray[num17] = CTime.GetDateTimeFromString(asRows[index++], '-', ' ', ':');
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(byte[]))
                        {
                            byte[] buffer = (byte[])obj2;
                            for (int num18 = 0; num18 < buffer.Length; num18++)
                            {
                                buffer[num18] = Convert.ToByte(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        if (info.FieldType == typeof(sbyte[]))
                        {
                            sbyte[] numArray9 = (sbyte[])obj2;
                            for (int num19 = 0; num19 < numArray9.Length; num19++)
                            {
                                numArray9[num19] = Convert.ToSByte(asRows[index++]);
                            }
                            index--;
                            goto Label_0C8A;
                        }
                        Log.LogErrorMsg("CTable::_SetDataRow(): column type convertion not support in table while converting: " + info.FieldType.ToString());
                        return false;
                    }
                    catch (Exception exception)
                    {
                        Log.LogErrorMsg("CTable::_SetDataRow(): data type convertion failed in table while converting: " + info.FieldType.ToString() + "(" + exception.ToString() + ")");
                        return false;
                    }
                }
                try
                {
                    if (info.FieldType == typeof(string))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, "");
                        }
                        else
                        {
                            info.SetValue(local3, asRows[index]);
                        }
                    }
                    else if (info.FieldType == typeof(int))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToInt32(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(uint))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToUInt32(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(long))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToInt64(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(ulong))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToUInt64(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(float))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0f);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToSingle(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(double))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0.0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToDouble(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(short))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToInt16(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(ushort))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToUInt16(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(char))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToChar(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(bool))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, false);
                        }
                        else
                        {
                            bool flag3;
                            if (!bool.TryParse(asRows[index], out flag3))
                            {
                                flag3 = Convert.ToInt32(asRows[index]) != 0;
                            }
                            info.SetValue(local3, flag3);
                        }
                    }
                    else if (info.FieldType == typeof(DateTime))
                    {
                        if (asRows[index] == null)
                        {
                            DateTime time = new DateTime();
                            info.SetValue(local3, time);
                        }
                        else
                        {
                            info.SetValue(local3, CTime.GetDateTimeFromString(asRows[index], '-', ' ', ':'));
                        }
                    }
                    else if (info.FieldType.IsEnum)
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToInt32(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(byte))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToByte(asRows[index]));
                        }
                    }
                    else if (info.FieldType == typeof(sbyte))
                    {
                        if (asRows[index] == null)
                        {
                            info.SetValue(local3, 0);
                        }
                        else
                        {
                            info.SetValue(local3, Convert.ToSByte(asRows[index]));
                        }
                    }
                    else
                    {
                        Log.LogErrorMsg("CTable::_SetDataRow(): column type convertion not support in table while converting: " + info.FieldType.ToString());
                        return false;
                    }
                }
                catch (Exception exception2)
                {
                    Log.LogErrorMsg("CTable::_SetDataRow(): data type convertion failed in table while converting: " + info.FieldType.ToString() + "(" + exception2.ToString() + ")");
                    return false;
                }
            Label_0C8A:
                index++;
            }
            if (iKeyColumnIdx >= length)
            {
                Log.LogErrorMsg(string.Concat(new object[] { "CTable::_SetDataRow(): iKeyColumnIdx(", iKeyColumnIdx, ") out of available Field numbers(", length, ")" }));
                return false;
            }
            T1 key = (T1)aAvailableFields[iKeyColumnIdx].GetValue(local3);
            mapTable.Add(key, local3);
            return true;
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
                    this.m_listColumns.Clear();
                }
                this.m_bDisposed = true;
            }
        }

        public static bool ExportBinaryTableFile(string sTableFilename, string sBinaryTableFilename, int iKeyColumnIdx = 0)
        {
            CMapT<string, CRowData> mapTable = new CMapT<string, CRowData>();
            CTable table = new CTable();
            if (!table.LoadFile<string, CRowData>(sTableFilename, ref mapTable, iKeyColumnIdx, false))
            {
                return false;
            }
            CFile file = new CFile();
            file.SetCryptSalt(m_byCryptSalt);
            if (!file.Open(sBinaryTableFilename, 10, true))
            {
                Log.LogErrorMsg("CTable::ExportBinaryTableFile(): Open file error - " + sBinaryTableFilename);
                return false;
            }
            if (!file.WriteHeader("BCS", CPath.Name(sTableFilename), 1))
            {
                Log.LogErrorMsg("CTable::ExportBinaryTableFile(): Write header failed!");
                return false;
            }
            if (!table._ExportBinarySchema(file))
            {
                Log.LogErrorMsg("CTable::ExportBinaryTableFile(): Export schema failed!");
                return false;
            }
            if (!table._ExportBinaryData(file, mapTable))
            {
                Log.LogErrorMsg("CTable::ExportBinaryTableFile(): Export data failed!");
                return false;
            }
            file.Close(true);
            file.Dispose();
            return true;
        }

        public static bool ExportSQLCommandFile(string sTableFilename, string sSQLCommandFilename, string sSpecifyTableName = null, int iKeyColumnIdx = 0)
        {
            string str;
            if (sSpecifyTableName != null)
            {
                str = "`" + CPath.Name(sSpecifyTableName) + "`";
                string str2 = CPath.Ext(sSpecifyTableName);
                if (str2.Length > 0)
                {
                    str = (str + ".") + "`" + str2.Substring(1) + "`";
                }
            }
            else
            {
                str = "`" + CPath.Name(sTableFilename) + "`";
            }
            CMapT<string, CRowData> mapTable = new CMapT<string, CRowData>();
            CTable table = new CTable();
            if (!table.LoadFile<string, CRowData>(sTableFilename, ref mapTable, iKeyColumnIdx, false))
            {
                return false;
            }
            StringBuilder sb = new StringBuilder(0x400);
            if (!table._ExportSchema(sb, str))
            {
                Log.LogErrorMsg("CTable::ExportSQLCommandFile(): Export schema failed!");
                return false;
            }
            sb.Append("\r\n\r\n");
            if (!table._ExportData(sb, str, mapTable))
            {
                Log.LogErrorMsg("CTable::ExportSQLCommandFile(): Export data failed!");
                return false;
            }
            string s = sb.ToString();
            CFile file = new CFile();
            if (!file.Open(sSQLCommandFilename, 0x109, true))
            {
                Log.LogErrorMsg("CTable::ExportSQLCommandFile(): Open file error - " + sSQLCommandFilename);
                return false;
            }
            if (file.Write(s) <= 0)
            {
                Log.LogErrorMsg("CTable::ExportSQLCommandFile(): Write to file: " + sSQLCommandFilename + " error! - content: " + s);
                return false;
            }
            file.Close(true);
            file.Dispose();
            return true;
        }

        ~CTable()
        {
            this.Dispose(false);
        }

        public List<CColumnInfo> GetSchema()
        {
            return this.m_listColumns;
        }

        public bool LoadFile<T1, T2>(string sFilename, ref CMapT<T1, T2> mapTable, int iKeyColumnIdx = 0, bool bSuppressFailMsg = false) where T2 : new()
        {
            bool flag = false;
            try
            {
                if (iKeyColumnIdx < 0)
                {
                    iKeyColumnIdx = 0;
                }
                this.Reset();
                string str = Path.GetExtension(sFilename).ToUpper();
                if (str == this.m_sBinaryExtName.ToUpper())
                {
                    flag = this._LoadFileBinary<T1, T2>(sFilename, mapTable, iKeyColumnIdx, true);
                }
                else if (str.Length != 0)
                {
                    foreach (string str2 in m_listTextExtSequences)
                    {
                        if (str == str2.ToUpper())
                        {
                            flag = this._LoadFileText<T1, T2>(sFilename, mapTable, iKeyColumnIdx, true);
                        }
                    }
                    if (!flag)
                    {
                        flag = this._LoadFileBinary<T1, T2>(sFilename, mapTable, iKeyColumnIdx, true);
                        if (!flag)
                        {
                            this.Reset();
                            flag = this._LoadFileText<T1, T2>(sFilename, mapTable, iKeyColumnIdx, true);
                        }
                    }
                }
                else
                {
                    string str3 = sFilename;
                    str3 = str3 + this.m_sBinaryExtName;
                    flag = this._LoadFileBinary<T1, T2>(str3, mapTable, iKeyColumnIdx, true);
                    if (!flag)
                    {
                        foreach (string str4 in m_listTextExtSequences)
                        {
                            this.Reset();
                            str3 = sFilename;
                            str3 = str3 + str4;
                            if (this._LoadFileText<T1, T2>(str3, mapTable, iKeyColumnIdx, true))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (flag)
                {
                    return flag;
                }
                if (!bSuppressFailMsg)
                {
                    Log.LogWarningMsg("CTable::LoadFile(): Load file: " + sFilename + " failed!");
                }
                return false;
            }
            catch (Exception exception)
            {
                Log.LogErrorMsg("CTable::LoadFile(): Load file: " + sFilename + " exception: " + exception.ToString());
                flag = false;
            }
            return flag;
        }

        public void Reset()
        {
            for (int i = 0; i < 11; i++)
            {
                this.m_abSchemaAttrRead[i] = false;
            }
            if (this.m_listColumns != null)
            {
                this.m_listColumns.Clear();
            }
            this.m_iSchemaVersion = 0;
            this.m_iNumDataRows = 0;
            this.m_iNumSchemaAttrRead = 0;
            this.m_sFilename = "";
        }

        public static void SetCustomBindStreamFunction(CustomBindStreamFunction fn)
        {
            m_fnCustomBindStream = fn;
        }

        public static List<string> TextExtSequences
        {
            get
            {
                return m_listTextExtSequences;
            }
            set
            {
                m_listTextExtSequences = value;
                if (m_listTextExtSequences.Count == 0)
                {
                    m_listTextExtSequences.Add(".csv");
                }
            }
        }

        public class CColumnInfo
        {
            public bool m_bAutoIncrement;
            public bool m_bBinary;
            public bool m_bIndex;
            public bool m_bNotNull;
            public bool m_bPrimaryKey;
            public bool m_bUnique;
            public bool m_bUnsigned;
            public bool m_bZeroFill;
            public ERuntimeType m_eRuntimeType;
            public string m_sDatatype = "";
            public string m_sDefault = "";
            public string m_sName = "";

            public enum ERuntimeType
            {
                TYPE_UNDEFINED,
                TYPE_BOOL,
                TYPE_BYTE,
                TYPE_SBYTE,
                TYPE_CHAR,
                TYPE_ENUM,
                TYPE_INT16,
                TYPE_UINT16,
                TYPE_INT32,
                TYPE_UINT32,
                TYPE_INT64,
                TYPE_UINT64,
                TYPE_SINGLE,
                TYPE_DOUBLE,
                TYPE_STRING,
                TYPE_DATETIME
            }
        }

        public class CRowData
        {
            public string[] m_asData;
        }

        public delegate bool CustomBindStreamFunction(string sFilename, ref CTextParser parser);
    }
}
