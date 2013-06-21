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
    internal class Closed : AtomicState
    {
        private readonly CircuitBreaker _breaker;

        public Closed( CircuitBreaker breaker )
            : base( breaker.CallTimeout )
        {
            _breaker = breaker;
        }

        public override async Task<T> Invoke<T>( Task<T> body )
        {
            return await CallThrough( body );
        }

        public override async Task Invoke( Task body )
        {
            await CallThrough( body );
        }

        protected override void CallFails( )
        {
            if ( Increment( ) == _breaker.MaxFailures )
            {
                _breaker.TripBreaker( this );
            }
        }

        protected override void CallSucceeds( )
        {
            Swap( 0 );
        }

        protected override void EnterInternal( )
        {
            Swap( 0 );
        }

        public override string ToString( )
        {
            return string.Format( "Closed with failure count = {0}", CurrentValue );
        }
    }
}