using System.Threading;

namespace EnhancedThreadPool
{
    internal class ThreadContainer
    {
        public Thread Thread { get; }

        public ThreadContainer(Thread thread)
        {
            Thread = thread;
        }
        public void MarkForDispose()
        {
            ShouldDispose = true;
        }
        public bool ShouldDispose { get; set; }
    }
}
