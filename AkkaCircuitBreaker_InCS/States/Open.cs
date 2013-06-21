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
    internal class Open : AtomicState
    {
        private readonly CircuitBreaker _breaker;

        public Open( CircuitBreaker breaker )
            : base( breaker.CallTimeout )
        {
            _breaker = breaker;
        }

        public override Task<T> Invoke<T>( Task<T> body )
        {
            throw new OpenCircuitException( );
        }

        public override Task Invoke( Task body )
        {
            throw new OpenCircuitException( );
        }

        protected override void CallFails( )
        {
            //throw new NotImplementedException();
        }

        protected override void CallSucceeds( )
        {
            //throw new NotImplementedException();
        }

        protected override void EnterInternal( )
        {
            Task.Delay( _breaker.ResetTimeout ).ContinueWith( task => _breaker.AttemptReset( ) );
        }
    }
}