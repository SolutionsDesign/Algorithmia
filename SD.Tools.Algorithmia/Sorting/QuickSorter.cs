//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2009 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2009 Solutions Design. All rights reserved.
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
	/// Implements the quick sort algorithm. See: http://en.wikipedia.org/wiki/Quick_sort
	/// The class implements Quicksort again, even though .NET already implements QuickSort, because the sort should be done
	/// in-place. Using .NET's quicksort algorithm would require a copy to an array and back into the list to sort, which could be an expensive operation with
	/// large collections to sort.
	/// Algorithm is based on Wikipedia's algorithm.
	/// </summary>
	internal class QuickSorter : ISortAlgorithm
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
			// create lambda which will produce the proper boolean value to use for the partition routine
			Func<T, T, bool> valueComparerTest;
			switch(direction)
			{
				case SortDirection.Ascending:
					valueComparerTest = (a, b) => (compareFunc(a, b) < 0);
					break;
				case SortDirection.Descending:
					valueComparerTest = (a, b) => (compareFunc(a, b) > 0);
					break;
				default:
					throw new ArgumentOutOfRangeException("direction", "Invalid direction specified, can't craete value comparer func");
			}

			// start the sort by calling the recursive routine using the initial values.
			PerformSort(toSort, startIndex, endIndex, valueComparerTest);
		}


		/// <summary>
		/// Performs the sort of a partition in the list. This routine is called recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="toSort">To sort.</param>
		/// <param name="left">The left index.</param>
		/// <param name="right">The right index.</param>
		/// <param name="valueComparerTest">The value comparer test.</param>
		private static void PerformSort<T>(IList<T> toSort, int left, int right, Func<T, T, bool> valueComparerTest)
		{
			if(right <= left)
			{
				return;
			}
			int pivotIndex = Partition(toSort, left, right, left, valueComparerTest);
			PerformSort(toSort, left, pivotIndex - 1, valueComparerTest);
			PerformSort(toSort, pivotIndex + 1, right, valueComparerTest);
		}


		/// <summary>
		/// Partitions the specified list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="toSort">To sort.</param>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <param name="pivotIndex">Index of the pivot.</param>
		/// <param name="valueComparerTest">The value comparer test.</param>
		/// <returns>index for new pivot point</returns>
		private static int Partition<T>(IList<T> toSort, int left, int right, int pivotIndex, Func<T, T, bool> valueComparerTest)
		{
			T pivotValue = toSort[pivotIndex];
			// move pivot to the end of the partition
			toSort.SwapValues(pivotIndex, right);	
			int storeIndex = left;
			for(int i = left; i < right; i++)
			{
				if(!valueComparerTest(toSort[i], pivotValue))
				{
					continue;
				}
				toSort.SwapValues(i, storeIndex);
				storeIndex++;
			}
			toSort.SwapValues(storeIndex, right);
			return storeIndex;
		}
	}
}
