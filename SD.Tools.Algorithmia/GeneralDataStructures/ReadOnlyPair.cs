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
//		- Frans  Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Simple class which represents a pair of values which can be of different types, and which is readonly. 
	/// </summary>
	/// <typeparam name="TVal1">The type of value 1.</typeparam>
	/// <typeparam name="TVal2">The type of value 2.</typeparam>
	[Serializable]
	public class ReadOnlyPair<TVal1, TVal2>
	{
		#region Class Member Declarations
		private readonly TVal1 _value1;
		private readonly TVal2 _value2;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Pair&lt;TVal1, TVal2&gt;"/> class.
		/// </summary>
		/// <param name="value1">The value for Value1.</param>
		/// <param name="value2">The value for Value2.</param>
		public ReadOnlyPair(TVal1 value1, TVal2 value2)
		{
			_value1 = value1;
			_value2 = value2;
		}


		/// <summary>
		/// Gets Value1.
		/// </summary>
		public TVal1 Value1
		{
			get { return _value1; }
		}

		/// <summary>
		/// Gets Value2.
		/// </summary>
		public TVal2 Value2 
		{
			get { return _value2; }
		}
	}
}
