using System.Threading;

namespace EnhancedThreadPool
{
    internal class InnerTask
    {
        public InnerTask(WaitCallback waitCallBack, object state)
        {
            WaitCallBack = waitCallBack;
            State = state;

        }
        public WaitCallback WaitCallBack { get; set; }
        public object State { get; set; }
    }
}
