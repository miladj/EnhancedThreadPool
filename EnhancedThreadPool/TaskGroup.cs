using System;
using System.Collections.Concurrent;

namespace EnhancedThreadPool
{
    internal class TaskGroup : IDisposable
    {
        public  string Name{get; private set;}
        private ConcurrentQueue<InnerTask> tasks = new ConcurrentQueue<InnerTask>();
        public bool Disposed { get; private set; } = false;
        public bool IsQueued { get; set; }
        public int Count => tasks.Count;
        public TaskGroup(string name)
        {
            Name = name;
        }
        
        public void Enqueue(InnerTask task)
        {
            tasks.Enqueue(task);
        }
        public bool Dequeue(out InnerTask innerTask)
        {
            return tasks.TryDequeue(out innerTask);
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
