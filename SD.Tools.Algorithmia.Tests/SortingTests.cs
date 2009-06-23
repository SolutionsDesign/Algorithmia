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

using SD.Tools.Algorithmia;
using SD.Tools.Algorithmia.Sorting;
using NUnit.Framework;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Tests for the sorting algorithms
	/// </summary>
	[TestFixture]
	public class SortingTests
	{
		[Test]
		public void SelectionSortSortListOfIntegersAscending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.SelectionSort, SortDirection.Ascending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize-1); i++)
			{
				Assert.IsTrue((toSort[i] <= toSort[i + 1]));
			}
		}


		[Test]
		public void SelectionSortSortListOfIntegersDescending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.SelectionSort, SortDirection.Descending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize - 1); i++)
			{
				Assert.IsTrue((toSort[i] >= toSort[i + 1]));
			}
		}


		[Test]
		public void ShellSortSortListOfIntegersAscending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.ShellSort, SortDirection.Ascending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize - 1); i++)
			{
				Assert.IsTrue((toSort[i] <= toSort[i + 1]));
			}
		}


		[Test]
		public void ShellSortSortListOfIntegersDescending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.ShellSort, SortDirection.Descending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize - 1); i++)
			{
				Assert.IsTrue((toSort[i] >= toSort[i + 1]));
			}
		}


		[Test]
		public void QuickSortSortListOfIntegersAscending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.QuickSort, SortDirection.Ascending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize - 1); i++)
			{
				Assert.IsTrue((toSort[i] <= toSort[i + 1]));
			}
		}


		[Test]
		public void QuickSortSortListOfIntegersDescending()
		{
			int listSize = 1000;
			List<int> toSort = CreateListOfRandomNumbers(listSize);

			toSort.Sort(SortAlgorithm.QuickSort, SortDirection.Descending);

			// check if all elements are in the right order
			for(int i = 0; i < (listSize - 1); i++)
			{
				Assert.IsTrue((toSort[i] >= toSort[i + 1]));
			}
		}


		#region Helpers
		/// <summary>
		/// Displays the list.
		/// </summary>
		/// <param name="toSort">To sort.</param>
		private void DisplayList<T>(List<T> toSort)
		{
			for(int i = 0; i < toSort.Count; i++)
			{
				if(i > 0)
				{
					Console.Write(", ");
				}
				Console.Write(toSort[i]);
			}
			Console.WriteLine();
		}



		/// <summary>
		/// Creates the list of random numbers.
		/// </summary>
		/// <param name="listSize">Size of the list.</param>
		/// <returns></returns>
		private static List<int> CreateListOfRandomNumbers(int listSize)
		{
			List<int> toSort = new List<int>();
			// add random ints
			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));

			for(int i = 0; i < listSize; i++)
			{
				toSort.Add(rand.Next(listSize*1313));
			}
			return toSort;
		}
		#endregion
	}
}
