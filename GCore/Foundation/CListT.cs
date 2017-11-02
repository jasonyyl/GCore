using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ggc.Foundation
{
    public sealed class CListT<T> : List<T>
    {
        private List<_Action<T>> m_listAction;

        public CListT()
        {
            this.InTheLoop = 0;
        }

        public new void Add(T theValue)
        {
            if (this.InTheLoop > 0)
            {
                if (this.m_listAction == null)
                {
                    this.m_listAction = new List<_Action<T>>();
                }
                this.m_listAction.Add(new _Action<T>(_Action<T>.EOperation.ADD, theValue, -1));
            }
            else
            {
                base.Add(theValue);
            }
        }

        public bool Erase(T theValue)
        {
            return this.Remove(theValue);
        }

        public bool Find(T theValue)
        {
            return base.Contains(theValue);
        }

        public void Flush()
        {
            if ((this.InTheLoop <= 0) && (this.m_listAction != null))
            {
                int num = 0;
                foreach (_Action<T> action in this.m_listAction)
                {
                    if (action.m_eOperation == _Action<T>.EOperation.ADD)
                    {
                        base.Add(action.m_Value);
                    }
                    else if (action.m_eOperation == _Action<T>.EOperation.REMOVE)
                    {
                        base.Remove(action.m_Value);
                    }
                    else if (action.m_iIdx >= 0)
                    {
                        base.RemoveAt(action.m_iIdx);
                        int iIdx = action.m_iIdx;
                        int count = this.m_listAction.Count;
                        for (int i = num + 1; i < count; i++)
                        {
                            _Action<T> action2 = this.m_listAction[i];
                            if (action2.m_eOperation == _Action<T>.EOperation.REMOVEAT)
                            {
                                if (action2.m_iIdx > iIdx)
                                {
                                    action2.m_iIdx--;
                                }
                                else if (action2.m_iIdx == iIdx)
                                {
                                    action2.m_iIdx = -1;
                                }
                            }
                        }
                    }
                    num++;
                }
                this.m_listAction.Clear();
            }
        }

        public new ListEnumerator<T> GetEnumerator()
        {
            return new ListEnumerator<T>((CListT<T>)this);
        }

        public void Insert(T theValue)
        {
            this.Add(theValue);
        }

        public new bool Remove(T theValue)
        {
            if (this.InTheLoop <= 0)
            {
                return base.Remove(theValue);
            }
            if (this.m_listAction == null)
            {
                this.m_listAction = new List<_Action<T>>();
            }
            this.m_listAction.Add(new _Action<T>(_Action<T>.EOperation.REMOVE, theValue, -1));
            return true;
        }

        public new void RemoveAt(int iIdx)
        {
            if (this.InTheLoop > 0)
            {
                if (this.m_listAction == null)
                {
                    this.m_listAction = new List<_Action<T>>();
                }
                this.m_listAction.Add(new _Action<T>(_Action<T>.EOperation.REMOVEAT, default(T), iIdx));
            }
            else
            {
                base.RemoveAt(iIdx);
            }
        }

        public int InTheLoop { get; set; }

        private class _Action<T1>
        {
            public EOperation m_eOperation;
            public int m_iIdx;
            public T1 m_Value;

            public _Action(EOperation eOperation, T1 theValue, int iIdx = -1)
            {
                this.m_eOperation = eOperation;
                this.m_Value = theValue;
                this.m_iIdx = iIdx;
            }

            public enum EOperation
            {
                ADD,
                REMOVE,
                REMOVEAT,
            }
        }

        public class ListEnumerator<T1> : IEnumerator<T1>, IDisposable, IEnumerator
        {
            private List<T1>.Enumerator m_orgEnumerator;
            private CListT<T1> m_theList;

            public ListEnumerator(CListT<T1> theList)
            {
                this.m_theList = theList;
                this.m_orgEnumerator = ((List<T1>)theList).GetEnumerator();
                this.m_theList.InTheLoop++;
            }

            public void Dispose()
            {
                this.m_theList.InTheLoop--;
                this.m_theList.Flush();
                this.m_orgEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return this.m_orgEnumerator.MoveNext();
            }

            public void Reset()
            {
            }

            public T1 Current
            {
                get
                {
                    return this.m_orgEnumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}