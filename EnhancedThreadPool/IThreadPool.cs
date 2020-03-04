using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EnhancedThreadPool
{
    public interface IThreadPool
    {
        int GetAvailableThreads();
        int GetMaxThreads();
        int GetMinThreads();        
        bool QueueUserWorkItem(WaitCallback callBack);        
        bool QueueUserWorkItem(WaitCallback callBack, object state);
        bool SetMaxThreads(int workerThreads);
        bool SetMinThreads(int workerThreads);
        bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state);

        void CreateGroup(string name);


    }
    public class EnhancedThreadPool : IThreadPool
    {
        private class InnerTask
        {
            public WaitCallback WaitCallBack { get; set; }
            public object State { get; set; }
        }
        private class TaskGroup
        {
            private readonly string name;

            public TaskGroup(string name)
            {
                this.name = name;
            }
            private ConcurrentQueue<InnerTask> tasks= new ConcurrentQueue<InnerTask>();
            public void Enqueue(InnerTask task)
            {
                tasks.Enqueue(task);
            }
            public bool Denqueue(out InnerTask innerTask)
            {
                return tasks.TryDequeue(out innerTask);
                
            }
        }
        private int _maxThread;
        private int _minThread;
        private ConcurrentDictionary<string, TaskGroup> groups = new ConcurrentDictionary<string, TaskGroup>();
        private BlockingCollection<TaskGroup> taskGroups = new BlockingCollection<TaskGroup>();

        public void CreateGroup(string name)
        {
            groups.TryAdd(name, new TaskGroup(name));
            sem
        }

        public int GetAvailableThreads()
        {
            throw new NotImplementedException();
        }

        public int GetMaxThreads()
        {
            return _maxThread;
        }

        public int GetMinThreads()
        {
            return _minThread;
        }

        public bool QueueUserWorkItem(WaitCallback callBack)
        {
            throw new NotImplementedException();
        }

        public bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            throw new NotImplementedException();
        }

        public bool SetMaxThreads(int workerThreads)
        {
            throw new NotImplementedException();
        }

        public bool SetMinThreads(int workerThreads)
        {
            throw new NotImplementedException();
        }

        public bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        {
            throw new NotImplementedException();
        }
    }
}
