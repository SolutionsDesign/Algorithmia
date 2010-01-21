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
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.Sorting
{
	/// <summary>
	/// Static class which contains the extension method for IList(Of T) implementations to sort these implementations using various sorting algorithms.
	/// </summary>
	public static class SortingExtensionMethods
	{
		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction)
		{
			toSort.Sort(algorithm, direction, -1, -1, GeneralUtils.GetUsableComparison<T>(null));
		}

		
		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		/// <param name="compareFunc">The compare func to compare two elements in the list to sort.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction, Comparison<T> compareFunc)
		{
			toSort.Sort(algorithm, direction, -1, -1, compareFunc);
		}


		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		/// <param name="comparer">The comparer to use. If set to null, the default comparer for T is used.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction, IComparer<T> comparer)
		{
			toSort.Sort(algorithm, direction, -1, -1, GeneralUtils.GetUsableComparison(comparer));
		}


		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endIndex">The end index.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction, int startIndex, int endIndex)
		{
			toSort.Sort(algorithm, direction, startIndex, endIndex, GeneralUtils.GetUsableComparison<T>(null));
		}


		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endIndex">The end index.</param>
		/// <param name="comparer">The comparer to use. If set to null, the default comparer for T is used.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction, int startIndex, int endIndex, IComparer<T> comparer)
		{
			toSort.Sort(algorithm, direction, startIndex, endIndex, GeneralUtils.GetUsableComparison(comparer));
		}


		/// <summary>
		/// Sorts the specified IList(Of T) using the algorithm specified in the order specified.
		/// </summary>
		/// <typeparam name="T">type of the elements in toSort</typeparam>
		/// <param name="toSort">the list to sort.</param>
		/// <param name="algorithm">The algorithm to use.</param>
		/// <param name="direction">The direction to sort the elements in.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endIndex">The end index.</param>
		/// <param name="compareFunc">The compare func to compare two elements in the list to sort.</param>
		public static void Sort<T>(this IList<T> toSort, SortAlgorithm algorithm, SortDirection direction, int startIndex, int endIndex, Comparison<T> compareFunc)
		{
			if(toSort.IsNullOrEmpty() || (toSort.Count == 1))
			{
				return;
			}

			int startIndexToUse = startIndex;
			int endIndexToUse = endIndex;
			if(startIndexToUse < 0)
			{
				startIndexToUse = 0;
			}
			if(startIndexToUse >= toSort.Count)
			{
				throw new IndexOutOfRangeException(string.Format("startIndex's value {0} is outside the list to sort, which has {1} values.", startIndexToUse, toSort.Count));
			}
			if(endIndexToUse < 0)
			{
				endIndexToUse = toSort.Count - 1;
			}
			if(endIndexToUse >= toSort.Count)
			{
				throw new IndexOutOfRangeException(string.Format("endIndex's value {0} is outside the list to sort, which has {1} values.", endIndexToUse, toSort.Count));
			}

			Comparison<T> comparisonToUse = compareFunc;
			if(compareFunc == null)
			{
				comparisonToUse = GeneralUtils.GetUsableComparison<T>(null);
			}

			ISortAlgorithm sorter = SortAlgorithmFactory.CreateSortAlgorithmImplementation(algorithm);
			if(sorter != null)
			{
				sorter.Sort(toSort, direction, startIndexToUse, endIndexToUse, comparisonToUse);
			}
		}
	}
}
