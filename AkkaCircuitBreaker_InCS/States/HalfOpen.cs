using System.Globalization;
using System.Threading.Tasks;
using AkkaCircuitBreaker_InCS.Concurrency;
using AkkaCircuitBreaker_InCS.Exceptions;

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
    internal class HalfOpen : AtomicState
    {
        private readonly CircuitBreaker _breaker;
        private readonly AtomicInteger _lock;
        private const int True = 1;
        private const int False = 0;

        public HalfOpen( CircuitBreaker breaker )
            : base( breaker.CallTimeout )
        {
            _breaker = breaker;
            _lock = new AtomicInteger( True );
        }

        public override async Task<T> Invoke<T>( Task<T> body )
        {
            if ( !_lock.CompareAndSwap( True, False ) )
            {
                throw new OpenCircuitException( );
            }
            return await CallThrough( body );
        }

        public override async Task Invoke( Task body )
        {
            if ( !_lock.CompareAndSwap( True, False ) )
            {
                throw new OpenCircuitException( );
            }
            await CallThrough( body );
        }

        protected override void CallFails( )
        {
            _breaker.TripBreaker( this );
        }

        protected override void CallSucceeds( )
        {
            _breaker.ResetBreaker( );
        }

        protected override void EnterInternal( )
        {
            _lock.Swap( True );
        }

        public override string ToString( )
        {
            return string.Format( CultureInfo.InvariantCulture, "Half-Open currently testing call for success = {0}", ( CurrentValue == True ) );
        }
    }
}