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

namespace SD.Tools.Algorithmia.Sorting
{
	/// <summary>
	/// Implements the shell sort algorithm. See: http://en.wikipedia.org/wiki/Shell_sort
	/// The algorithm implemented below is from Sedgewick: http://www.cs.princeton.edu/~rs/shell/
	/// It uses Sedgewick's increments values, as described in this paper: http://www.cs.princeton.edu/~rs/shell/paperF.pdf
	/// </summary>
	internal class ShellSorter : ISortAlgorithm
	{
		/// <summary>
		/// Sorts the specified list in the direction specified.
		/// </summary>
		/// <typeparam name="T">type of element to sort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="direction">The direction to sort the elements in toSort.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endIndex">The end index.</param>
		/// <param name="compareFunc">The compare func.</param>
		void ISortAlgorithm.Sort<T>(IList<T> toSort, SortDirection direction, int startIndex, int endIndex, Comparison<T> compareFunc)
		{
			// create a lambda which will produce the proper boolean value to use in our if statement based on the direction specified.
			Func<T, T, bool> valueComparerTest;
			switch(direction)
			{
				case SortDirection.Ascending:
					valueComparerTest = (a, b) => (compareFunc(a, b) > 0);
					break;
				case SortDirection.Descending:
					valueComparerTest = (a, b) => (compareFunc(a, b) < 0);
					break;
				default:
					throw new ArgumentOutOfRangeException("direction", "Invalid direction specified, can't craete value comparer func");
			}

			int[] increments = new[]{ 1391376, 463792, 198768, 86961, 33936, 13776, 4592, 1968, 861, 336,	112, 48, 21, 7, 3, 1 };
			for(int incrementIndex = 0; incrementIndex < increments.Length; incrementIndex++)
			{
				for(int intervalIndex = increments[incrementIndex], i = startIndex + intervalIndex; i <= endIndex; i++)
				{
					T currentValue = toSort[i]; 
					int j = i;
					while((j >= intervalIndex) && valueComparerTest(toSort[j - intervalIndex], currentValue))
					{ 
						toSort[j] = toSort[j - intervalIndex]; 
						j -= intervalIndex; 
					}
					toSort[j] = currentValue;
				}
			}
		}
	}
}
