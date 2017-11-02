using System;

namespace ggc.Foundation
{
    public sealed class CTimer : IDisposable
    {
        private bool m_bDisposed;
        private bool m_bEnable;
        private float m_fInterval;
        private DateTime m_time;

        public CTimer(float fTimeInterval = -1f)
        {
            this.m_fInterval = fTimeInterval;
            this.m_bEnable = true;
            this.Reset();
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
                this.m_bDisposed = true;
            }
        }

        public bool Enable()
        {
            return this.m_bEnable;
        }

        public void Enable(bool b)
        {
            this.m_bEnable = b;
        }

        ~CTimer()
        {
            this.Dispose(false);
        }

        public float Interval()
        {
            return this.m_fInterval;
        }

        public void Interval(float fInv)
        {
            this.m_fInterval = fInv;
        }

        public bool OnTime()
        {
            if (!this.m_bEnable)
            {
                return false;
            }
            if (this.m_fInterval < 0f)
            {
                return false;
            }
            if ((this.m_fInterval != 0f) && (this.Seconds() < this.m_fInterval))
            {
                return false;
            }
            return true;
        }

        public float Percent()
        {
            if (this.m_fInterval <= 0f)
            {
                return 0f;
            }
            return (this.Seconds() / this.m_fInterval);
        }

        public void Reset()
        {
            this.m_time = DateTime.Now;
        }

        public float Seconds()
        {
            return (float)(((double)DateTime.Now.Subtract(this.m_time).Ticks) / 10000000.0);
        }
    }
}
