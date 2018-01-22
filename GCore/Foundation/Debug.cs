using System.Diagnostics;

namespace GCore.Foundation
{
    public sealed class Debug
    {
        private static bool m_bDumpAssertionCallstacksOnly;

        public static void Assert(bool bCondition)
        {
            if (m_bDumpAssertionCallstacksOnly && !bCondition)
            {
                Log.LogErrorMsg("Assertion failed! \nCallStack:\n" + new StackTrace(1, true).ToString());
            }
        }

        public static void Assert(bool bCondition, string sReason)
        {
            if (m_bDumpAssertionCallstacksOnly && !bCondition)
            {
                Log.LogErrorMsg("Assertion failed: " + sReason + "\nCallStack:\n" + new StackTrace(1, true).ToString());
            }
        }

        public static void Assert(bool bCondition, string sReason, string sDetail)
        {
            if (m_bDumpAssertionCallstacksOnly && !bCondition)
            {
                StackTrace trace = new StackTrace(1, true);
                Log.LogErrorMsg("Assertion failed: " + sReason + "\nDetail: " + sDetail + "\nCallStack:\n" + trace.ToString());
            }
        }

        public static bool DumpAssertionCallstackOnly()
        {
            return m_bDumpAssertionCallstacksOnly;
        }

        public static void DumpAssertionCallstackOnly(bool bDump)
        {
            m_bDumpAssertionCallstacksOnly = bDump;
        }
    }
}
