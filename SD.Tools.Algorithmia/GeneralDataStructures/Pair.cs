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
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Simple class which represents a pair of values which can be of different types. It's not a struct so it can be modified in-place inside
	/// other constructs.
	/// </summary>
	/// <typeparam name="TVal1">The type of value 1.</typeparam>
	/// <typeparam name="TVal2">The type of value 2.</typeparam>
	/// <remarks>Pair implements Equals and GetHashCode. The implementation is oriented towards C# null behavior (null==null is true), so if you're using 
	/// VB.NET, you've to take this into account.</remarks>
	[Serializable]
	public class Pair<TVal1, TVal2>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Pair&lt;TVal1, TVal2&gt;"/> class.
		/// </summary>
		public Pair()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Pair&lt;TVal1, TVal2&gt;"/> class.
		/// </summary>
		/// <param name="value1">The value for Value1.</param>
		/// <param name="value2">The value for Value2.</param>
		public Pair(TVal1 value1, TVal2 value2)
		{
			this.Value1 = value1;
			this.Value2 = value2;
		}


		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		/// <remarks>null == null is considered true. If TVal1 or TVal2 is an array type, the values are considered equal if the values in the array are
		/// equal</remarks>
		public override bool Equals(object obj)
		{
			Pair<TVal1, TVal2> toCompareWith = obj as Pair<TVal1, TVal2>;
			if(toCompareWith == null)
			{
				return false;
			}
			return (GeneralUtils.ValuesAreEqual(this.Value1, toCompareWith.Value1) && GeneralUtils.ValuesAreEqual(this.Value2, toCompareWith.Value2));
		}


		/// <summary>
		/// Returns the hashcode of this instance. 
		/// </summary>
		/// <returns>
		/// The XOR result of the hashcode of Value1 and Value2. If one of the values is null, that value is ignored. If both are null, the hashcode of this instance
		/// is returned. 
		/// </returns>
		public override int GetHashCode()
		{
			bool value1IsNull = ((object)this.Value1 == null);
			bool value2IsNull = ((object)this.Value2 == null);
			if(value1IsNull && value2IsNull)
			{
				return base.GetHashCode();
			}
			if(value1IsNull)
			{
				return this.Value2.GetHashCode();
			}
			if(value2IsNull)
			{
				return this.Value1.GetHashCode();
			}
			return this.Value1.GetHashCode() ^ this.Value2.GetHashCode();
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets or sets Value1.
		/// </summary>
		public TVal1 Value1 { get; set; }

		/// <summary>
		/// Gets or sets Value2.
		/// </summary>
		public TVal2 Value2 { get; set; }
		#endregion
	}
}
