#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolytopeSolutions.Toolset.Threading {
    public class ConcurentTaskController<T>{
        // TODO: Evaluate perforrmance on mobile devices.

        private string source;
        private Task concurentLoop;

        private float minDelay;
        private int concurentCount;

        private Action<T, Action<T>> InTaskAction;
        private Queue<KeyValuePair<T,Action<T>>> requestData;

        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private SemaphoreSlim concurrencySemaphore;

        public ConcurentTaskController(string _source, int _concurentCount, float _minDelay, 
            Action<T, Action<T>> _InTaskAction) {
            this.source = _source;
            this.minDelay = _minDelay;
            this.concurentCount = _concurentCount;
            this.InTaskAction = _InTaskAction;

            this.requestData = new Queue<KeyValuePair<T, Action<T>>>();
            this.tokenSource = new CancellationTokenSource();

            Start();
        }
        ~ConcurentTaskController() {
            Stop();
        }
        public void Start() {
            #if DEBUG
            Debug.Log("ConcurrentTaskController ["+this.source+"]: Strating.");
            #endif
            if (this.concurentLoop != null)
                Stop();

            this.requestData.Clear();
            this.concurrencySemaphore = new SemaphoreSlim(this.concurentCount);
            this.token = this.tokenSource.Token;
            this.concurentLoop = Task.Run(RequestingLoop, this.token);
        }
        public void Stop() {
            #if DEBUG
            Debug.Log("ConcurrentTaskController ["+this.source+"]: Stopping.");
            #endif
            if (this.tokenSource != null) { 
                this.tokenSource.Cancel();
                //this.tokenSource.Dispose();
            }
            //this.concurrencySemaphore.Dispose();
            //this.concurentLoop.Dispose();
        }

        // Infinite loop that runs actions on separate tasks.
        private void RequestingLoop() {
            while (true) {
                // If no requests, wait a bit and check again.
                if (this.requestData.Count == 0) {
                    Task.Delay(TimeSpan.FromSeconds(this.minDelay));
                    continue;
                }

                // Loop through them and schedule them, respecting concurency limit.
                List<Task> currentTasks = new List<Task>();
                while(this.requestData.Count > 0) {
                    // Get the current item.
                    KeyValuePair<T, Action<T>> currentItem;
                    lock (this.requestData)
                        currentItem = this.requestData.Dequeue();
                    #if DEBUG2
                    Debug.Log("ConcurrentTaskController ["+this.source+"]: Waiting for concurency semaphore: " + this.concurrencySemaphore.CurrentCount);
                    #endif
                    this.concurrencySemaphore.Wait();
                    // Launch the task.
                    Task currentTask = Task.Factory.StartNew(() => {
                        try {
                            #if DEBUG2
                            Debug.Log("ConcurrentTaskController ["+this.source+"]: Running action in task");
                            #endif
                            InTaskAction(currentItem.Key, currentItem.Value);
                        }
                        finally {
                            this.concurrencySemaphore.Release();
                        }
                    }, this.token);
                    // Add it to the list of current tasks.
                    currentTasks.Add(currentTask);
                }
                // Wait for all tasks to finish.
                Task.WaitAll(currentTasks.ToArray());
                #if DEBUG
                Debug.Log("ConcurrentTaskController ["+this.source+"]: Finished current tasks: " + currentTasks.Count);
                #endif
            }
        }

        public void AddRequest(T request, Action<T> _OnCompletionAction=null) { 
            #if DEBUG
            Debug.Log("ConcurrentTaskController ["+this.source+"]: Adding a task");
            #endif
            lock (this.requestData) {
                this.requestData.Enqueue(new KeyValuePair<T,Action<T>>(request, _OnCompletionAction));
            }
        }
    }
}
