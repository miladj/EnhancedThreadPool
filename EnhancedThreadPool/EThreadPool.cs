using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnhancedThreadPool
{
    public class EThreadPool : IThreadPool
    {
        private const string defaultGroupName = "__DefaultGroupName__";
        private uint _maxThread;
        private uint _minThread;
        private ConcurrentDictionary<string, TaskGroup> groups = new ConcurrentDictionary<string, TaskGroup>();
        private LinkedList<TaskGroup> taskGroups = new LinkedList<TaskGroup>();
        private List<ThreadContainer> threadList;
        private bool _isDisposed;
        private object _lockObject = new object();
        private ManualResetEventSlim manualReset=new ManualResetEventSlim(false);
        public string UniqueName { get; private set; } = Guid.NewGuid().ToString();
        private LinkedListNode<TaskGroup> _current=null;
        private int _threadInUseCounter=0;
        public EThreadPool(uint workerThread):this(workerThread,workerThread)
        {

        }
        private EThreadPool(uint minThread,uint maxThread)
        {
            if (minThread <= 0)
                throw new ArgumentOutOfRangeException(nameof(minThread));
            if (maxThread <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxThread));
            threadList = new List<ThreadContainer>((int)minThread);
            _maxThread = maxThread;
            _minThread = minThread;
            for (int i = 0; i < minThread; i++)
            {                
                threadList.Add(CreateThread());                
            }
            Console.WriteLine("Thread count "+threadList.Count);
        }
        private ThreadContainer CreateThread()
        {
            var tc = new ThreadContainer(new Thread(WorkerTask) { IsBackground = true, Name = UniqueName + "_WorkerThread_"+Guid.NewGuid().ToString() });
            tc.Thread.Start(tc);
            
            return tc;
        }
        private void CheckForDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EThreadPool)+" "+UniqueName);
        }       
        

        public uint GetAvailableThreads()
        {
            
            return (uint)threadList.Count-(uint)_threadInUseCounter;
        }

        public uint GetMaxThreads()
        {

            return _maxThread;
        }

        public uint GetMinThreads()
        {
            return _minThread;
        }

        public bool QueueUserWorkItem(WaitCallback callBack)
        {
            return this.QueueUserWorkItem(defaultGroupName, callBack);
        }
        public bool QueueUserWorkItem(string groupName, WaitCallback callBack)
        {
            return this.QueueUserWorkItem(groupName, callBack, null);
        }

        public bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return QueueUserWorkItem(defaultGroupName, callBack, state);
        }
        public bool QueueUserWorkItem(string groupName, WaitCallback callBack, object state)
        {
            CheckForDisposed();
            TaskGroup tg = groups.GetOrAdd(groupName, key => new TaskGroup(groupName));
            tg.Enqueue(new InnerTask(callBack,state));
            
            if(!tg.IsQueued)
            {
                lock(tg)
                {
                    if(!tg.IsQueued)
                    {

                        lock (taskGroups)
                        {
                            tg.IsQueued=true;
                            taskGroups.AddLast(tg);
                        }                        
                    }
                }
            }
            manualReset.Set();
            return true;
        }

        private bool SetMaxThreads(uint workerThreads)
        {
            CheckForDisposed();
            if (workerThreads == 0) return false;
            if (_maxThread== workerThreads) return false;
            _maxThread = workerThreads;
            return true;
        }

        public bool SetMinThreads(uint workerThreads)
        {
            CheckForDisposed();
            if (workerThreads == 0) return false;
            if (_minThread == workerThreads) return false;

            lock (_lockObject)
            {
                if (_minThread < workerThreads)
                {
                    for (uint i = 0; i < workerThreads - _minThread; i++)
                    {
                        threadList.Add(CreateThread());
                    }
                }
                else
                {
                    for (uint i = 0; i < _minThread-workerThreads; i++)
                    {
                        ThreadContainer tc = threadList[0];
                        threadList.RemoveAt(0);
                        tc.MarkForDispose();
                    }
                }
                _minThread = workerThreads;
            }
            return true;
        }        
        
        //public bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        //{
        //    return this.UnsafeQueueUserWorkItem(defaultGroupName, callBack, state);
        //}
        //public bool UnsafeQueueUserWorkItem(string groupName, WaitCallback callBack, object state)
        //{
        //    throw new NotImplementedException();
        //}

        private void WorkerTask(object state)
        {
            ThreadContainer tc = state as ThreadContainer;

            while (!tc.ShouldDispose)
            {      
                if(_current==null)
                {
                    manualReset.Wait();
                    lock(taskGroups)
                    {
                        if(taskGroups.First==null)
                        {
                            manualReset.Reset();
                            continue;
                        }
                        if(_current==null)
                        {
                            _current=taskGroups.First;
                        }
                    }
                    continue;
                }
                LinkedListNode<TaskGroup> item=null;
                lock(taskGroups)
                {
                    if(_current==null)continue;
                    item=_current;
                    _current=_current.Next;
                }
                
                Interlocked.Increment(ref _threadInUseCounter);
                
                //foreach (TaskGroup item in taskGroups.GetConsumingEnumerable())
                try
                {
                    if (item.Value.Dequeue(out InnerTask innerTask))
                    {
                        try
                        {
                            innerTask.WaitCallBack(innerTask.State);
                        }
                        catch
                        {
                            //TODO: Handle Exception
                        }
                    }
                    if (item.Value.Count == 0)
                    {
                        lock (item)
                        {
                            if (item.Value.Count == 0 && item.Value.IsQueued)
                            {             
                                item.Value.IsQueued=false;
                                lock (taskGroups)
                                {
                                    taskGroups.Remove(item);
                                }
                            }
                        }
                    }
                }
                finally{
                    
                    Interlocked.Decrement(ref _threadInUseCounter);
                    
                }
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            //taskGroups.CompleteAdding();
            foreach (var item in threadList)
            {
                item.MarkForDispose();
                item.Thread.Abort();
            }
            GC.SuppressFinalize(this);
        }
        ~EThreadPool()
        {
            Dispose();
        }
    }
}
