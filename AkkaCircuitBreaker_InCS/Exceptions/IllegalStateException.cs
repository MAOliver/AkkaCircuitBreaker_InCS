﻿using System;
using System.Runtime.Serialization;

namespace AkkaCircuitBreaker_InCS.Exceptions
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
    [Serializable]
    public class IllegalStateException : ApplicationException
    {
        public IllegalStateException( string message )
            : base( message )
        {
        }

        public IllegalStateException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        public IllegalStateException( )
        {
        }

        protected IllegalStateException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}