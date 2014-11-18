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
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SD.Tools.Algorithmia.UtilityClasses
{
	/// <summary>
	/// General set of utility routines
	/// </summary>
	public static class GeneralUtils
	{
		/// <summary>
		/// Gets a usable comparison for the comparer passed in.
		/// </summary>
		/// <typeparam name="T">the type comparer works on</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <returns>
		/// usable comparison
		/// </returns>
		public static Comparison<T> GetUsableComparison<T>(IComparer<T> comparer)
		{
			IComparer<T> comparerToUse = comparer;
			if(comparer == null)
			{
				comparerToUse = Comparer<T>.Default;
			}
			return (a, b) => (comparerToUse.Compare(a, b));
		}


		/// <summary>
		/// Compares the two values passed in and checks if they're value-wise the same. This extends 'Equals' in the sense that if the values are
		/// arrays it considers them the same if the values of the arrays are the same as well and the length is the same. 
		/// </summary>
		/// <remarks>It assumes the types of value1 and value2 are the same</remarks>
		/// <param name="value1">The first value to compare</param>
		/// <param name="value2">The second value to compare</param>
		/// <returns>true if the values should be considered equal. If value1 or value2 are null and the other isn't, false is returned. If both are null,
		/// true is returned.</returns>
		public static bool ValuesAreEqual(object value1, object value2)
		{
			if(((value1 == null) && (value2 != null)) || ((value1 != null) && (value2 == null)))
			{
				return false;
			}
			if(value1 == null)		// test on value1, if value1 is null at this spot, value2 also has to be null, otherwise we'd have been caught by the previous expression.
			{
				return true;
			}

			// not null, proceed with checks on values.
			Type value1Type = value1.GetType();
			Type value2Type = value2.GetType();

			if(value1Type != value2Type)
			{
				return false;
			}

			if(value1Type.IsArray)
			{
				return CheckArraysAreEqual((Array)value1, (Array)value2);
			}
			return value1.Equals(value2);
		}
		

		/// <summary>
		/// Performs a per-value compare on the arrays passed in and returns true if the arrays are of the same length and contain the same values.
		/// </summary>
		/// <param name="arr1"></param>
		/// <param name="arr2"></param>
		/// <returns>true if the arrays contain the same values and are of the same length</returns>
		public static bool CheckArraysAreEqual(Array arr1, Array arr2)
		{
			if(((arr1 == null) && (arr2 != null)) || ((arr1 != null) && (arr2 == null)))
			{
				return false;
			}

			if(arr1 == null)	// test on arr1, if arr1 is null at this spot, arr2 also has to be null, otherwise we'd have been caught by the previous expression.
			{
				return true;
			}

			// non-null arrays.
			if(arr1.Length != arr2.Length)
			{
				return false;
			}

			bool areEqual = true;
			for(int i = 0; i < arr1.Length; i++)
			{
				areEqual &= arr1.GetValue(i).Equals(arr2.GetValue(i));
				if(!areEqual)
				{
					break;
				}
			}

			return areEqual;
		}
	}
}
