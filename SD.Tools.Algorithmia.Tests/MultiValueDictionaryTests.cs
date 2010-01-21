//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2010 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2010 Solutions Design. All rights reserved.
// 
// The Algorithmia library sourcecode and its accompanying tools, tests and support code
// are released under the following license: (BSD2)
// ----------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met: 
//
// 1) Redistributions of source code must retain the above copyright notice, this list of 
//    conditions and the following disclaimer. 
// 2) Redistributions in binary form must reproduce the above copyright notice, this list of 
//    conditions and the following disclaimer in the documentation and/or other materials 
//    provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY SOLUTIONS DESIGN ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL SOLUTIONS DESIGN OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
//
// The views and conclusions contained in the software and documentation are those of the authors 
// and should not be interpreted as representing official policies, either expressed or implied, 
// of Solutions Design. 
//
//////////////////////////////////////////////////////////////////////
// Contributers to the code:
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SD.Tools.Algorithmia.GeneralDataStructures;

namespace SD.Tools.Algorithmia.Tests
{
	[TestFixture]
	public class MultiValueDictionaryTests
	{
		[Test]
		public void SimpleEnumerationTest()
		{
			MultiValueDictionary<int, string> container = new MultiValueDictionary<int, string> { { 1, "value1" }, { 2, "value2" }, { 2, "value3" } };

			foreach(KeyValuePair<int, HashSet<string>> pair in container)
			{
				switch(pair.Key)
				{
					case 1:
						Assert.AreEqual("value1", pair.Value.First());
						break;
					case 2:
						Assert.AreEqual(2, pair.Value.Count);
						Assert.IsTrue(pair.Value.Contains("value2"));
						Assert.IsTrue(pair.Value.Contains("value3"));
						break;
				}
			}
		}


		[Test]
		public void SimpleEnumerationTestWithLinqOperator()
		{
			MultiValueDictionary<int, string> container = new MultiValueDictionary<int, string> { { 1, "value1" }, { 2, "value2" }, { 2, "value3" } };

			foreach(KeyValuePair<int, HashSet<string>> pair in container.OrderBy(p=>p.Key))
			{
				switch(pair.Key)
				{
					case 1:
						Assert.AreEqual("value1", pair.Value.First());
						break;
					case 2:
						Assert.AreEqual(2, pair.Value.Count);
						Assert.IsTrue(pair.Value.Contains("value2"));
						Assert.IsTrue(pair.Value.Contains("value3"));
						break;
				}
			}
		}
	}
}
