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
// 
// Originally written by Frans Bouma for LLBLGen Pro's Linq provider.
// http://www.llblgen.com
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Class which implements the IGrouping interface to return grouped results in a query
	/// </summary>
	/// <typeparam name="TKey">type of the grouping key</typeparam>
	/// <typeparam name="TElement">type of the elements grouped</typeparam>
	public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
	{
		#region Class Member Declarations
		private readonly TKey _key;
		private readonly IEnumerable<TElement> _elements;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="Grouping&lt;TKey, TElement&gt;"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="elements">The grouped elements.</param>
		public Grouping(TKey key, IEnumerable<TElement> elements)
		{
			_key = key;
			_elements = elements;
		}


		/// <summary>
		/// Gets the key of the <see cref="T:System.Linq.IGrouping`2"/>.
		/// </summary>
		/// <returns>The key of the <see cref="T:System.Linq.IGrouping`2"/>.</returns>
		TKey IGrouping<TKey, TElement>.Key
		{
			get { return _key; }
		}
		
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TElement> GetEnumerator()
		{
			if(_elements==null)
			{
				return null;
			}
			return _elements.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
