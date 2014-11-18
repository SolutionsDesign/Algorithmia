//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2014 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2014 Solutions Design. All rights reserved.
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
using SD.Tools.Algorithmia.GeneralDataStructures.EventArguments;

namespace SD.Tools.Algorithmia.GeneralDataStructures.EqualityComparers
{
	/// <summary>
	/// An IEqualityComparer usable to compare ElementChangedEventArgs objects.
	/// </summary>
	/// <typeparam name="TChangeType">The type of the change type. Assumed to be an enum type</typeparam>
	/// <typeparam name="TElement">The type of the element.</typeparam>
	public class ElementChangedEventArgsComparer<TChangeType, TElement> : IEqualityComparer<ElementChangedEventArgs<TChangeType, TElement>>
	{
		/// <summary>
		/// Compares x with y and returns true if they're equal. This is a value-based comparison (so InvolvedElement and changetype are equal)
		/// </summary>
		/// <param name="x">first element to compare</param>
		/// <param name="y">second element to compare</param>
		/// <returns>true if x and y represent the same event arguments, false otherwise</returns>
		public bool Equals(ElementChangedEventArgs<TChangeType, TElement> x, ElementChangedEventArgs<TChangeType, TElement> y)
		{
			if((x == null) || (y == null))
			{
				return false;
			}
			if((x.InvolvedElement == null) || (y.InvolvedElement == null))
			{
				return false;
			}
			return (x.InvolvedElement.Equals(y.InvolvedElement) && y.TypeOfChange.Equals(x.TypeOfChange));
		}


		/// <summary>
		/// Returns a hash code for the specified object. 
		/// </summary>
		/// <param name="obj">The instance to return the hascode for.</param>
		/// <returns>A hash code for the specified object</returns>
		public int GetHashCode(ElementChangedEventArgs<TChangeType, TElement> obj)
		{
			if(obj == null)
			{
				return this.GetHashCode();
			}
			if(obj.InvolvedElement == null)
			{
				return obj.GetHashCode();
			}
			return obj.InvolvedElement.GetHashCode() ^ obj.TypeOfChange.GetHashCode();
		}
	}
}
