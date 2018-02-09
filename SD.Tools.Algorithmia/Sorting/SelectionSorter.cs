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
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.Sorting
{
	/// <summary>
	/// Implements the selection sort algorithm. See: http://en.wikipedia.org/wiki/Selection_sort
	/// </summary>
	internal class SelectionSorter : ISortAlgorithm
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

			for(int i = startIndex; i < endIndex; i++)
			{
				int indexValueToSwap = i;
				for(int j = i+1; j <= endIndex; j++)
				{
					if(valueComparerTest(toSort[indexValueToSwap], toSort[j]))
					{
						// found new value to swap
						indexValueToSwap = j;
					}
				}
				// swap value at position i with position valueToSwap
				toSort.SwapValues(i, indexValueToSwap);
			}
		}
	}
}
