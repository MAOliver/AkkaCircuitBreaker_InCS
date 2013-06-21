using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AkkaCircuitBreaker_InCS.Exceptions;
using AkkaCircuitBreaker_InCS.States;

namespace AkkaCircuitBreaker_InCS
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
    public class CircuitBreaker
    {

        private AtomicState _currentState;

        public int MaxFailures { get; private set; }

        public TimeSpan CallTimeout { get; private set; }
        public TimeSpan ResetTimeout { get; private set; }

        private AtomicState Closed { get; set; }
        private AtomicState Open { get; set; }
        private AtomicState HalfOpen { get; set; }

        private bool SwapState( AtomicState oldState, AtomicState newState )
        {
            return Interlocked.CompareExchange( ref _currentState, newState, oldState ) == oldState;
        }

        public static CircuitBreaker Create( int maxFailures, TimeSpan callTimeout, TimeSpan resetTimeout )
        {
            return new CircuitBreaker( maxFailures, callTimeout, resetTimeout );
        }

        public CircuitBreaker( int maxFailures, TimeSpan callTimeout, TimeSpan resetTimeout )
        {
            MaxFailures = maxFailures;
            CallTimeout = callTimeout;
            ResetTimeout = resetTimeout;
            Closed = new Closed( this );
            Open = new Open( this );
            HalfOpen = new HalfOpen( this );
            _currentState = Closed;
            //_failures = new AtomicInteger();
        }

        private AtomicState CurrentState
        {
            get
            {
                Interlocked.MemoryBarrier( );
                var currentValue = _currentState;
                Interlocked.MemoryBarrier( );
                return currentValue;
            }
        }

        public int CurrentFailureCount
        {
            get { return Closed.CurrentValue; }
        }

        public async Task<T> WithCircuitBreaker<T>( Task<T> body )
        {
            return await CurrentState.Invoke( body );
        }

        public async Task WithCircuitBreaker( Task body )
        {
            await CurrentState.Invoke( body );
        }
        /// <summary>
        /// The failure will be recorded farther down.
        /// </summary>
        /// <param name="body"></param>
        public void WithSyncCircuitBreaker( Action body )
        {
            var cbTask = WithCircuitBreaker( Task.Factory.StartNew( body ) );
            if ( !cbTask.Wait( CallTimeout ) )
            {
                //throw new TimeoutException( string.Format( "Execution did not complete within the time alotted {0} ms", CallTimeout.TotalMilliseconds ) );
            }
            if ( cbTask.Exception != null )
            {
                throw cbTask.Exception;
            }
        }

        /// <summary>
        /// https://github.com/scala/scala/blob/v2.10.1/src/actors/scala/actors/Future.scala#L1
        /// 
        /// A Future that does not complete within the time alotted should return None
        /// 
        /// Await.result(
        //      withCircuitBreaker(try Future.successful(body) catch { case NonFatal(t) ⇒ Future.failed(t) }),
        //      callTimeout)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <returns><typeparam name="T"></typeparam> or default(<typeparam name="T"></typeparam>) -- '
        /// NOTE: I have this implemented as Maybe<typeparam name="T"></typeparam> internally, but my Option implementation is currently too flawed to make a part of this </returns>
        public T WithSyncCircuitBreaker<T>( Func<T> body )
        {
            var cbTask = WithCircuitBreaker( Task.Factory.StartNew( body ) );
            return cbTask.Wait( CallTimeout ) ? cbTask.Result : default(T);
        }

        public CircuitBreaker OnOpen( Action callback )
        {
            Open.AddListener( callback );
            return this;
        }

        public CircuitBreaker OnHalfOpen( Action callback )
        {
            HalfOpen.AddListener( callback );
            return this;
        }


        public CircuitBreaker OnClose( Action callback )
        {
            Closed.AddListener( callback );
            return this;
        }

        private void Transition( AtomicState fromState, AtomicState toState )
        {
            if ( SwapState( fromState, toState ) )
            {
                Debug.WriteLine( "Successful transition from {0} to {1}", fromState, toState );
                toState.Enter( );
            }
            else
            {
                throw new IllegalStateException( string.Format( "Illegal transition attempted from {0} to {1}", fromState, toState ) );
            }
        }

        internal void TripBreaker( AtomicState fromState )
        {
            Transition( fromState, Open );
        }

        internal void ResetBreaker( )
        {
            Transition( HalfOpen, Closed );
        }

        internal void AttemptReset( )
        {
            Transition( Open, HalfOpen );
        }
    }
}
