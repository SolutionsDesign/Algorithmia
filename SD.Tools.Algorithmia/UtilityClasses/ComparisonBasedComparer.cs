//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2018 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2018 Solutions Design. All rights reserved.
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

namespace SD.Tools.Algorithmia.UtilityClasses
{
	/// <summary>
	/// Generic comparer which uses a comparison func to implement the compare method
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ComparisonBasedComparer<T> : Comparer<T>
	{
		#region Class Member Declarations
		private readonly Comparison<T> _compareFunc;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="ComparisonBasedComparer&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="compareFunc">The compare func.</param>
		public ComparisonBasedComparer(Comparison<T> compareFunc)
		{
			ArgumentVerifier.CantBeNull(compareFunc, "compareFunc");
			_compareFunc = compareFunc;
		}


		/// <summary>
		/// Compares the two passed in elements using the set compareFunc.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// the result of the compareFunc applied to the two input parameters. If FlipCompareResult is set to true, the result is negated.</returns>
		public override int Compare(T x, T y)
		{
			int toReturn = _compareFunc(x, y);
			if(this.FlipCompareResult)
			{
				toReturn = -toReturn;
			}
			return toReturn;
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets or sets the flag which, if set to true, makes the Compare method flip its result: -1 becomes 1 and 1 becomes -1. This flag can be used to
		/// use the comparer in descending sorted lists.
		/// </summary>
		public bool FlipCompareResult { get; set; }
		#endregion
	}
}
