using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AkkaCircuitBreaker_InCS.Concurrency;

namespace AkkaCircuitBreaker_InCS.States
{
    /// <summary>
    /// Copyright 2013 Matthew Oliver <a href="https://github.com/MAOliver/AkkaCircuitBreaker_InCS">AkkaCircuitBreaker_InCS</a>
    ///
    /// Licensed under the Apache License, Version 2.0 (the "License");
    /// you may not use this file except in compliance with the License.
    /// You may obtain a copy of the License at
    ///
    ///       <a href="http://www.apache.org/licenses/LICENSE-2.0">Apache License-2.0</a>
    ///
    /// Unless required by applicable law or agreed to in writing, software
    /// distributed under the License is distributed on an "AS IS" BASIS,
    /// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    /// See the License for the specific language governing permissions and
    /// limitations under the License.
    /// 
    /// Adapted from <a href="https://github.com/akka/akka/blob/master/akka-actor/src/main/scala/akka/pattern/CircuitBreaker.scala">CircuitBreaker.scala</a>
    /// </summary>
    public abstract class AtomicState : AtomicInteger, IState
    {
        private readonly ConcurrentQueue<Action> _listeners;
        private readonly TimeSpan _callTimeout;

        protected AtomicState( TimeSpan callTimeout )
        {
            _listeners = new ConcurrentQueue<Action>( );
            _callTimeout = callTimeout;
        }

        public void AddListener( Action listener )
        {
            _listeners.Enqueue( listener );
        }

        public bool HasListeners
        {
            get { return !_listeners.IsEmpty; }
        }
        //todo what if this throws an exception?
        public void NotifyTransitionListeners( )
        {
            if ( !HasListeners )
                return;
            Task
                .Factory
                .StartNew
                (
                    ( ) =>
                        {
                            foreach ( var listener in _listeners )
                            {
                                listener.Invoke( );
                            }
                        }
                );
        }

        /// <summary>
        /// see http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<T> CallThrough<T>( Task<T> task )
        {
            var deadline = DateTime.Now.Add( _callTimeout );
            try
            {
                return await task;
            }
            finally
            {
                switch ( task.Status )
                {
                    case TaskStatus.RanToCompletion:
                        if ( DateTime.Now.CompareTo( deadline ) < 0 )
                        {
                            CallSucceeds( );
                            break;
                        }
                        goto default;
                    default:
                        CallFails( );
                        break;
                }
            }
        }

        /// <summary>
        /// see http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx, added cancellation token to handle cancelling "zombie" job 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task CallThrough( Task task )
        {
            var deadline = DateTime.Now.Add( _callTimeout );
            try
            {
                await task;
            }
            finally
            {
                switch ( task.Status )
                {
                    case TaskStatus.RanToCompletion:
                        if ( DateTime.Now.CompareTo( deadline ) < 0 )
                        {
                            CallSucceeds( );
                            break;
                        }
                        goto default;
                    default:
                        CallFails( );
                        break;
                }
            }
        }

        public abstract Task<T> Invoke<T>( Task<T> body );
        public abstract Task Invoke( Task body );

        protected abstract void CallFails( );

        protected abstract void CallSucceeds( );

        protected abstract void EnterInternal( );

        public void Enter( )
        {
            EnterInternal( );
            NotifyTransitionListeners( );
        }

    }
}