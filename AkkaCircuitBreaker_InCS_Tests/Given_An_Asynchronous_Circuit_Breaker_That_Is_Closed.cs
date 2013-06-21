using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    [TestClass]
    public class Given_An_Asynchronous_Circuit_Breaker_That_Is_Closed : CircuitBreakerTestKit
    {
        [TestMethod]
        public void When_Called_Then_Allow_Call_Through( )
        {
            var breaker = LongCallTimeoutCb( );
            var result = breaker.Instance.WithCircuitBreaker( Task.Run( ( ) => SayTest( ) ) );

            Assert.AreEqual( SayTest( ), result.Result );
        }

        [TestMethod]
        public void When_Call_Fails_Then_Increment_Failure_Count( )
        {
            var breaker = LongCallTimeoutCb( );

            Assert.AreEqual( breaker.Instance.CurrentFailureCount, 0 );
            Assert.IsTrue( Intercept<TestException>( ( ) => breaker.Instance.WithCircuitBreaker( Task.Run( ( ) => ThrowException( ) ) ).Wait( AwaitTimeout ) ) );
            Assert.IsTrue( CheckLatch( breaker.OpenLatch ) );
            Assert.AreEqual( 1, breaker.Instance.CurrentFailureCount );
        }

        [TestMethod]
        public void When_Call_Succeeds_After_Failure_Then_Reset_Failure_Count( )
        {
            var breaker = MultiFailureCb( );

            Assert.AreEqual( breaker.Instance.CurrentFailureCount, 0 );

            var whenall = Task.WhenAll
                (
                    new[ ]
                        {
                            breaker.Instance.WithCircuitBreaker(Task.Factory.StartNew(ThrowException))
                            , breaker.Instance.WithCircuitBreaker(Task.Factory.StartNew(ThrowException))
                            , breaker.Instance.WithCircuitBreaker(Task.Factory.StartNew(ThrowException))
                            , breaker.Instance.WithCircuitBreaker(Task.Factory.StartNew(ThrowException))
                        }
                );

            Assert.IsTrue( Intercept<TestException>( ( ) => whenall.Wait( AwaitTimeout ) ) );

            Assert.AreEqual( breaker.Instance.CurrentFailureCount, 4 );

            var result = breaker.Instance.WithCircuitBreaker( Task.Run( ( ) => SayTest( ) ) ).Result;

            Assert.AreEqual( SayTest( ), result );
            Assert.AreEqual( 0, breaker.Instance.CurrentFailureCount );
        }

        [TestMethod]
        public void When_Call_Times_Out_Then_Increment_Failure_Count( )
        {
            var breaker = ShortCallTimeoutCb( );

            breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ( ) =>
                                                                            {
                                                                                Thread.Sleep( 500 );
                                                                                return SayTest( );
                                                                            } ) );

            Assert.IsTrue( CheckLatch( breaker.OpenLatch ) );
            Assert.AreEqual( 1, breaker.Instance.CurrentFailureCount );
        }
    }
}