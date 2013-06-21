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
    public class Given_An_Asynchronous_Circuit_Breaker_That_Is_HalfOpen : CircuitBreakerTestKit
    {
        [TestMethod]
        public void When_Next_Called_Then_Pass_Call_And_Transition_To_Close_On_Success( )
        {
            var breaker = ShortResetTimeoutCb( );
            Intercept<TestException>( ( ) => breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ThrowException ) ) );
            Assert.IsTrue( CheckLatch( breaker.HalfOpenLatch ) );

            var result = breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ( ) => SayTest( ) ) );

            Assert.IsTrue( CheckLatch( breaker.ClosedLatch ) );
            Assert.AreEqual( SayTest( ), result.Result );
        }

        [TestMethod]
        public void When_Next_Called_Then_Pass_Call_And_Transition_To_Open_On_Exception( )
        {
            var breaker = ShortResetTimeoutCb( );


            Assert.IsTrue( Intercept<TestException>( ( ) => breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ThrowException ) ).Wait( ) ) );
            Assert.IsTrue( CheckLatch( breaker.HalfOpenLatch ) );

            Assert.IsTrue( Intercept<TestException>( ( ) => breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ThrowException ) ).Wait( ) ) );
            Assert.IsTrue( CheckLatch( breaker.OpenLatch ) );
        }

        [TestMethod]
        public void When_Next_Called_Then_Pass_Call_And_Transition_To_Open_On_Async_Failure( )
        {
            var breaker = ShortResetTimeoutCb( );

            breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ThrowException ) );
            Assert.IsTrue( CheckLatch( breaker.HalfOpenLatch ) );

            breaker.Instance.WithCircuitBreaker( Task.Factory.StartNew( ThrowException ) );
            Assert.IsTrue( CheckLatch( breaker.OpenLatch ) );
        }
    }
}