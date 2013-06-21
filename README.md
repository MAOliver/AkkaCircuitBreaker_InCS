AkkaCircuitBreaker_InCS
=======================

    Copyright 2013 Matthew Oliver https://github.com/MAOliver/AkkaCircuitBreaker_InCS
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
              
			  http://www.apache.org/licenses/LICENSE-2.0
    
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
     
    Adapted from https://github.com/akka/akka/blob/master/akka-actor/src/main/scala/akka/pattern/CircuitBreaker.scala
	(Licensed under Apache LICENSE-2.0)
    

Implementation of Akka's CircuitBreaker in CS

I had difficulty finding a satisfactory threadsafe circuit breaker in CSharp
for my current project. Ultimately, I wound up translating Akka's CircuitBreaker
into CSharp. It satisifies the unit tests, though I have not used Akka's CircuitBreaker
extensively, so there may be issues. Let me know/fix them if you find them.


