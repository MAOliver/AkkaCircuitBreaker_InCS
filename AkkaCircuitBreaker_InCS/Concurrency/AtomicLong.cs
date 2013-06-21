using System.Threading;

namespace AkkaCircuitBreaker_InCS.Concurrency
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
    public class AtomicLong : IAtomicValue<long>
    {
        private long _currentValue;

        public AtomicLong( )
        {
        }

        public AtomicLong( long currentValue )
        {
            _currentValue = currentValue;
        }

        public long Swap( long newValue )
        {
            return Interlocked.Exchange( ref _currentValue, newValue );
        }

        public long CurrentValue
        {
            get { return Interlocked.Add( ref _currentValue, 0 ); }
        }

        public bool CompareAndSwap( long oldValue, long newValue )
        {
            return Interlocked.CompareExchange( ref _currentValue, newValue, oldValue ) == oldValue;
        }

        public long Increment( )
        {
            return Interlocked.Increment( ref _currentValue );
        }

        public long Decrement( )
        {
            return Interlocked.Decrement( ref _currentValue );
        }

        public long Add( long byValue )
        {
            return Interlocked.Add( ref _currentValue, byValue );
        }
    }
}