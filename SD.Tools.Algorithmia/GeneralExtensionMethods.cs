//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2008 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2008 Solutions Design. All rights reserved.
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

namespace SD.Tools.Algorithmia
{
	/// <summary>
	/// General set of extension methods.
	/// </summary>
	public static class GeneralExtensionMethods
	{
		/// <summary>
		/// Determines whether the passed in list is null or empty.
		/// </summary>
		/// <typeparam name="T">the type of the elements in the list to check</typeparam>
		/// <param name="toCheck">the list to check.</param>
		/// <returns>true if the passed in list is null or empty, false otherwise</returns>
		public static bool IsNullOrEmpty<T>(this IList<T> toCheck)
		{
			return ((toCheck == null) || (toCheck.Count <= 0));
		}


		/// <summary>
		/// Swaps the values at indexA and indexB.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="indexA">The index for A.</param>
		/// <param name="indexB">The index for B.</param>
		public static void SwapValues<T>(this IList<T> source, int indexA, int indexB)
		{
			if((indexA < 0) || (indexA >= source.Count))
			{
				throw new IndexOutOfRangeException("indexA is out of range");
			}
			if((indexB < 0) || (indexB >= source.Count))
			{
				throw new IndexOutOfRangeException("indexB is out of range");
			}

			if(indexA == indexB)
			{
				return;
			}

			T tempValue = source[indexA];
			source[indexA] = source[indexB];
			source[indexB] = tempValue;
		}


		/// <summary>
		/// Raises the event which is represented by the handler specified. 
		/// </summary>
		/// <typeparam name="T">type of the event args</typeparam>
		/// <param name="handler">The handler of the event to raise.</param>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="arguments">The arguments to pass to the handler.</param>
		public static void RaiseEvent<T>(this EventHandler<T> handler, object sender, T arguments)
			where T : System.EventArgs
		{
			if(handler != null)
			{
				handler(sender, arguments);
			}
		}


		/// <summary>
		/// Determines whether the type this method is called on is a nullable type of type Nullable(Of T)
		/// </summary>
		/// <param name="toCheck">The type to check.</param>
		/// <returns>true if toCheck is a Nullable(Of T) type, otherwise false
		/// </returns>
		public static bool IsNullableValueType(this Type toCheck)
		{
			if((toCheck == null) || !toCheck.IsValueType)
			{
				return false;
			}
			return (toCheck.IsGenericType && toCheck.GetGenericTypeDefinition() == (typeof(Nullable<>)));
		}


		/// <summary>
		/// Adds the range defined by source to the destination.
		/// </summary>
		/// <param name="destination">The destination.</param>
		/// <param name="source">The source.</param>
		public static void AddRange<T>(this HashSet<T> destination, IEnumerable<T> source)
		{
			foreach(T element in source) 
			{
				destination.Add(element);
			}
		}


		/// <summary>
		/// Delimits the specified strings in the enumerable passed in with the delimiter specified
		/// </summary>
		/// <param name="toDelimit">To delimit.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns>
		/// the strings in toDelimit delimited with the delimited specified without a dangling delimiter at the end.
		/// </returns>
		/// <example>"foo" and "bar" with delimiter "," becomes "foo, bar"</example>
		/// <remarks>Inserts a space after each delimiter</remarks>
		public static string Delimit(this IEnumerable<string> toDelimit, string delimiter)
		{
			return toDelimit.Delimit(delimiter, true);
		}


		/// <summary>
		/// Delimits the specified strings in the enumerable passed in with the delimiter specified
		/// </summary>
		/// <param name="toDelimit">To delimit.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <param name="insertSpaceAfterDelimiter">if true, a space is appended after each delimiter.</param>
		/// <returns>
		/// the strings in toDelimit delimited with the delimited specified without a dangling delimiter at the end.
		/// </returns>
		/// <example>"foo" and "bar" with delimiter "," becomes "foo, bar"</example>
		public static string Delimit(this IEnumerable<string> toDelimit, string delimiter, bool insertSpaceAfterDelimiter)
		{
			StringBuilder builder = new StringBuilder();
			bool first = true;
			foreach(string toAppend in toDelimit)
			{
				if(!first)
				{
					builder.Append(delimiter);
					if(insertSpaceAfterDelimiter)
					{
						builder.Append(" ");
					}
				}
				builder.Append(toAppend);
				first = false;
			}
			return builder.ToString();
		}
	}
}
