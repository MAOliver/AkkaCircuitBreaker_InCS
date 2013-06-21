using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AkkaCircuitBreaker_InCS;

namespace AkkaCircuitBreaker_InCS_Tests
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
    /// Adapted from <a href="https://github.com/akka/akka/blob/master/akka-actor-tests/src/test/scala/akka/pattern/CircuitBreakerSpec.scala">CircuitBreakerSpec.scala</a>
    /// </summary>
    public class CircuitBreakerTestKit
    {
        private readonly TimeSpan _awaitTimeout = TimeSpan.FromSeconds( 2 );
        public TimeSpan AwaitTimeout { get { return _awaitTimeout; } }

        protected Breaker ShortCallTimeoutCb( )
        {
            return new Breaker( new CircuitBreaker( 1, TimeSpan.FromMilliseconds( 50 ), TimeSpan.FromMilliseconds( 500 ) ) );
        }
        protected Breaker ShortResetTimeoutCb( )
        {
            return new Breaker( new CircuitBreaker( 1, TimeSpan.FromMilliseconds( 1000 ), TimeSpan.FromMilliseconds( 50 ) ) );
        }
        protected Breaker LongCallTimeoutCb( )
        {
            return new Breaker( new CircuitBreaker( 1, TimeSpan.FromMilliseconds( 5000 ), TimeSpan.FromMilliseconds( 500 ) ) );
        }
        protected Breaker LongResetTimeoutCb( )
        {
            return new Breaker( new CircuitBreaker( 1, TimeSpan.FromMilliseconds( 100 ), TimeSpan.FromMilliseconds( 5000 ) ) );
        }
        protected Breaker MultiFailureCb( )
        {
            return new Breaker( new CircuitBreaker( 5, TimeSpan.FromMilliseconds( 200 ), TimeSpan.FromMilliseconds( 500 ) ) );
        }

        protected bool CheckLatch( CountdownEvent latch )
        {
            return latch.Wait( _awaitTimeout );
        }

        protected Task Delay( TimeSpan toDelay, CancellationToken? token )
        {
            return token.HasValue ? Task.Delay( toDelay, token.Value ) : Task.Delay( toDelay );
        }

        protected void ThrowException( )
        {
            throw new TestException( "Test Exception" );
        }

        protected string SayTest( )
        {
            return "Test";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
        protected bool Intercept<T>( Action action ) where T : class
        {
            try
            {
                action.Invoke( );
                return false;
            }
            catch ( Exception ex )
            {
                var aggregate = ex as AggregateException;
                if ( aggregate != null )
                {
                    foreach ( var temp in aggregate.InnerExceptions.Select( innerException => innerException as T ).Where( temp => temp == null ) )
                    {
                        throw;
                    }
                }
                else
                {
                    var temp = ex as T;

                    if ( temp == null )
                    {
                        throw;
                    }
                }

            }
            return true;
        }

    }
}