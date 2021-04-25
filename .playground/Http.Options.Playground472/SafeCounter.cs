using System.Threading;

namespace Http.Options.Playground472
{
    public class SafeCounter
    {
        public int Counter = 0;

        public SafeCounter Increment()
        {
            Interlocked.Increment(ref Counter);
            return this;
        }

        public SafeCounter Decrement()
        {
            Interlocked.Decrement(ref Counter);
            return this;
        }

        public static implicit operator int(SafeCounter me) => me.Counter;
    }

}