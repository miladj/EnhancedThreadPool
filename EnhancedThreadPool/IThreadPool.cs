using System;
using System.Threading;

namespace EnhancedThreadPool
{
    public interface IThreadPool:IDisposable
    {
        uint GetAvailableThreads();
        uint GetMaxThreads();
        uint GetMinThreads();        
        bool QueueUserWorkItem(WaitCallback callBack);        
        bool QueueUserWorkItem(string groupName,WaitCallback callBack);        
        bool QueueUserWorkItem(WaitCallback callBack, object state);
        bool QueueUserWorkItem(string groupName, WaitCallback callBack, object state);
        
        //bool SetMaxThreads(int workerThreads);
        bool SetMinThreads(uint workerThreads);
        //bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state);
        //bool UnsafeQueueUserWorkItem(string groupName, WaitCallback callBack, object state);


    }
}
