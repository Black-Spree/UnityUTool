using System;

namespace BetaTool.StaticUnityEvent
{
    [Serializable]
    public class StaticEvent<T> : StaticEvent
    {
        public void Invoke(T p1)
        {
            RealInvoke(p1);
        }
    }

    public class StaticEvent<T1, T2> : StaticEvent
    {
        public void Invoke(T1 p1, T2 p2)
        {
            RealInvoke(p1, p2);
        }
    }

    public class StaticEvent<T1, T2, T3> : StaticEvent
    {
        public void Invoke(T1 p1, T2 p2, T3 p3)
        {
            RealInvoke(p1, p2, p3);
        }
    }
}