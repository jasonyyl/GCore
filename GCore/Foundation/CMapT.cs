using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GCore.Foundation
{
    public sealed class CMapT<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new MapKeyCollection<TKey, TValue> Keys;
        private List<_Action<TKey, TValue>> m_listAction;
        public new MapValueCollection<TKey, TValue> Values;

        public CMapT()
        {
            this.InTheLoop = 0;
            this.Keys = new MapKeyCollection<TKey, TValue>((CMapT<TKey, TValue>)this, base.Keys);
            this.Values = new MapValueCollection<TKey, TValue>((CMapT<TKey, TValue>)this, base.Values);
        }

        public new void Add(TKey key, TValue value)
        {
            if (this.InTheLoop > 0)
            {
                if (this.m_listAction == null)
                {
                    this.m_listAction = new List<_Action<TKey, TValue>>();
                }
                this.m_listAction.Add(new _Action<TKey, TValue>(true, key, value));
            }
            else
            {
                base.Add(key, value);
            }
        }

        public bool Erase(TKey key)
        {
            return this.Remove(key);
        }

        public TValue Find(TKey key)
        {
            TValue local;
            if (base.TryGetValue(key, out local))
            {
                return local;
            }
            return default(TValue);
        }

        public TValue FirstValue()
        {
            if (this.Values.Count != 0)
            {
                return this.Values.First<TValue>();
            }
            return default(TValue);
        }

        public void Flush()
        {
            if ((this.InTheLoop <= 0) && (this.m_listAction != null))
            {
                foreach (_Action<TKey, TValue> action in this.m_listAction)
                {
                    if (action.m_bAdd)
                    {
                        base.Add(action.m_Key, action.m_Value);
                    }
                    else
                    {
                        base.Remove(action.m_Key);
                    }
                }
                this.m_listAction.Clear();
            }
        }

        public void Insert(TKey key, TValue value)
        {
            this.Add(key, value);
        }

        public TValue LastValue()
        {
            if (this.Values.Count != 0)
            {
                return this.Values.Last<TValue>();
            }
            return default(TValue);
        }

        public new bool Remove(TKey key)
        {
            if (this.InTheLoop <= 0)
            {
                return base.Remove(key);
            }
            if (this.m_listAction == null)
            {
                this.m_listAction = new List<_Action<TKey, TValue>>();
            }
            this.m_listAction.Add(new _Action<TKey, TValue>(false, key));
            return true;
        }

        public int InTheLoop { get; set; }

        private class _Action<TKey1, TValue1>
        {
            public bool m_bAdd;
            public TKey1 m_Key;
            public TValue1 m_Value;

            public _Action(bool bAdd, TKey1 theKey)
            {
                this.m_bAdd = bAdd;
                this.m_Key = theKey;
            }

            public _Action(bool bAdd, TKey1 theKey, TValue1 theValue)
            {
                this.m_bAdd = bAdd;
                this.m_Key = theKey;
                this.m_Value = theValue;
            }
        }

        public class MapKeyCollection<TKey1, TValue1> : IEnumerable<TKey1>, IEnumerable
        {
            private Dictionary<TKey1, TValue1>.KeyCollection m_orgKeys;
            private CMapT<TKey1, TValue1> m_theMap;

            public MapKeyCollection(CMapT<TKey1, TValue1> theMap, Dictionary<TKey1, TValue1>.KeyCollection orgKeys)
            {
                this.m_theMap = theMap;
                this.m_orgKeys = orgKeys;
            }

            public bool Contains(TKey1 key)
            {
                return this.m_orgKeys.Contains<TKey1>(key);
            }

            public void CopyTo(TKey1[] array, int index)
            {
                this.m_orgKeys.CopyTo(array, index);
            }

            protected Enumerator<TKey1, TValue1> GetEnumerator()
            {
                return new Enumerator<TKey1, TValue1>(this.m_theMap, this.m_orgKeys);
            }

            IEnumerator<TKey1> IEnumerable<TKey1>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.m_orgKeys.Count;
                }
            }

            public class Enumerator<TKey2, TValue2> : IEnumerator<TKey2>, IDisposable, IEnumerator
            {
                private Dictionary<TKey2, TValue2>.KeyCollection.Enumerator m_orgEnumerator;
                private Dictionary<TKey2, TValue2>.KeyCollection m_orgKeys;
                private CMapT<TKey2, TValue2> m_theMap;

                public Enumerator(CMapT<TKey2, TValue2> theMap, Dictionary<TKey2, TValue2>.KeyCollection orgKeys)
                {
                    this.m_theMap = theMap;
                    this.m_orgKeys = orgKeys;
                    this.m_orgEnumerator = this.m_orgKeys.GetEnumerator();
                    this.m_theMap.InTheLoop++;
                }

                public void Dispose()
                {
                    this.m_theMap.InTheLoop--;
                    this.m_theMap.Flush();
                    this.m_orgEnumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return this.m_orgEnumerator.MoveNext();
                }

                public void Reset()
                {
                }

                public TKey2 Current
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

        public class MapValueCollection<TKey1, TValue1> : IEnumerable<TValue1>, IEnumerable
        {
            private Dictionary<TKey1, TValue1>.ValueCollection m_orgValues;
            private CMapT<TKey1, TValue1> m_theMap;

            public MapValueCollection(CMapT<TKey1, TValue1> theMap, Dictionary<TKey1, TValue1>.ValueCollection orgValues)
            {
                this.m_theMap = theMap;
                this.m_orgValues = orgValues;
            }

            public bool Contains(TValue1 value)
            {
                return this.m_orgValues.Contains<TValue1>(value);
            }

            public void CopyTo(TValue1[] array, int index)
            {
                this.m_orgValues.CopyTo(array, index);
            }

            protected Enumerator<TKey1, TValue1> GetEnumerator()
            {
                return new Enumerator<TKey1, TValue1>(this.m_theMap, this.m_orgValues);
            }

            IEnumerator<TValue1> IEnumerable<TValue1>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.m_orgValues.Count;
                }
            }

            public class Enumerator<TKey2, TValue2> : IEnumerator<TValue2>, IDisposable, IEnumerator
            {
                private Dictionary<TKey2, TValue2>.ValueCollection.Enumerator m_orgEnumerator;
                private Dictionary<TKey2, TValue2>.ValueCollection m_orgValues;
                private CMapT<TKey2, TValue2> m_theMap;

                public Enumerator(CMapT<TKey2, TValue2> theMap, Dictionary<TKey2, TValue2>.ValueCollection orgValues)
                {
                    this.m_theMap = theMap;
                    this.m_orgValues = orgValues;
                    this.m_orgEnumerator = this.m_orgValues.GetEnumerator();
                    this.m_theMap.InTheLoop++;
                }

                public void Dispose()
                {
                    this.m_theMap.InTheLoop--;
                    this.m_theMap.Flush();
                    this.m_orgEnumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return this.m_orgEnumerator.MoveNext();
                }

                public void Reset()
                {
                    Debug.Assert(false, "Not implement!");
                }

                public TValue2 Current
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
}
