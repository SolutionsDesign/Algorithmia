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
	/// Extension methods for classes defined in the GeneralDataStructures namespace
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Converts an enumerable into a MultiValueDictionary. Similar to ToDictionary however this time the returned type is a MultiValueDictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelectorFunc">The key selector func.</param>
		/// <returns>MultiValueDictionary with the values of source stored under the keys retrieved by the keySelectorFunc which is applied to each
		/// value in source, or null if source is null</returns>
		public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelectorFunc)
		{
			if(source==null)
			{
				return null;
			}

			ArgumentVerifier.CantBeNull(keySelectorFunc, "keySelectorFunc");
			MultiValueDictionary<TKey, TValue> toReturn = new MultiValueDictionary<TKey, TValue>();
			foreach(TValue v in source)
			{
				toReturn.Add(keySelectorFunc(v), v);
			}
			return toReturn;
		}


		/// <summary>
		/// Splits the pairs in the enumerable into two lists of values where the pair at position x in source equals to values1[x], values2[x]
		/// </summary>
		/// <typeparam name="TValue1">The type of the Value1 property.</typeparam>
		/// <typeparam name="TValue2">The type of the Value2 property.</typeparam>
		/// <typeparam name="TValue3">The type of the elements in values1. TValue1 has to be casteable to TValue3.</typeparam>
		/// <typeparam name="TValue4">The type of the eleemnts in values2. TValue2 has to be casteable to TValue4.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="values1">The list of all Value1 values in the pairs in source.</param>
		/// <param name="values2">The list of all Value2 values in the pairs in source.</param>
		public static void SplitPairs<TValue1, TValue2, TValue3, TValue4>(this IEnumerable<Pair<TValue1, TValue2>> source, 
											List<TValue3> values1, List<TValue4> values2)
			where TValue3 : TValue1
			where TValue4: TValue2
		{
			ArgumentVerifier.CantBeNull(values1, "values1");
			ArgumentVerifier.CantBeNull(values2, "values2");

			if(source==null)
			{
				return;
			}
			foreach(Pair<TValue1, TValue2> pair in source)
			{
				values1.Add((TValue3)pair.Value1);
				values2.Add((TValue4)pair.Value2);
			}
		}

		/// <summary>
		/// Raises the event on the handler passed in with default empty arguments
		/// </summary>
		/// <param name="handler">The handler.</param>
		/// <param name="sender">The sender.</param>
		public static void RaiseEvent(this MemberValueElementChangedHandler handler, object sender)
		{
			if(handler != null)
			{
				handler(sender, EventArgs.Empty);
			}
		}
		
		
		/// <summary>
		/// Raises the event which is represented by the handler specified. 
		/// </summary>
		/// <typeparam name="T">type of the event args</typeparam>
		/// <param name="handler">The handler of the event to raise.</param>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="arguments">The arguments to pass to the handler.</param>
		public static void RaiseEvent<T>(this MemberValueElementChangedHandler handler, object sender, T arguments)
			where T : System.EventArgs
		{
			if(handler != null)
			{
				handler(sender, arguments);
			}
		}
		
		
		/// <summary>
		/// Raises the event on the handler passed in with default empty arguments
		/// </summary>
		/// <param name="handler">The handler.</param>
		/// <param name="sender">The sender.</param>
		public static void RaiseEvent(this MemberValueElementRemovedHandler handler, object sender)
		{
			if(handler != null)
			{
				handler(sender, EventArgs.Empty);
			}
		}
		
		
		/// <summary>
		/// Raises the event which is represented by the handler specified. 
		/// </summary>
		/// <typeparam name="T">type of the event args</typeparam>
		/// <param name="handler">The handler of the event to raise.</param>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="arguments">The arguments to pass to the handler.</param>
		public static void RaiseEvent<T>(this MemberValueElementRemovedHandler handler, object sender, T arguments)
			where T : System.EventArgs
		{
			if(handler != null)
			{
				handler(sender, arguments);
			}
		}
	}
}
