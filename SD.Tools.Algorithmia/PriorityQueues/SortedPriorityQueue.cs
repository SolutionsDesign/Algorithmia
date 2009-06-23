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

using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.Sorting;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.PriorityQueues
{
	/// <summary>
	/// Simple priority queue which uses a list for storing the elements which is always kept sorted. 
	/// Inserts can be slow, as worst case inserts (insert at the front) requires a move of all elements, so the order is somewhere around the order of
	/// insertion sort, O((n^2)/4). Lookups and removals are very fast: O(1).
	/// </summary>
	/// <typeparam name="TElement">The type of the elements in this queue</typeparam>
	/// <remarks>If you store value types in this queue, be aware that Peek() and Remove() on an empty queue will return the default value for
	/// the value type used, e.g. 0 for a priority queue with Int32 values. Therefore, consult Count to check whether there are any values in the queue.
	/// </remarks>
	public class SortedPriorityQueue<TElement> : PriorityQueueBase<TElement>
	{
		#region Class Member Declarations
		private List<TElement> _elements;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="SortedPriorityQueue&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="priorityComparison">The priority comparison func.</param>
		public SortedPriorityQueue(Comparison<TElement> priorityComparison) :
			base(priorityComparison)
		{
			InitDataStructures();
		}


		/// <summary>
		/// Peeks at the first element of the queue (the one with the highest priority, according to the priority comparison) and returns that element
		/// without removing it
		/// </summary>
		/// <returns>
		/// the first element in the queue or the default value for TElement if the queue is empty
		/// </returns>
		public override TElement Peek()
		{
			TElement toReturn = default(TElement);

			if(this.Count>0)
			{
				toReturn = _elements[0];
			}
			return toReturn;
		}


		/// <summary>
		/// Adds the specified element to the queue
		/// </summary>
		/// <param name="element">The element to add.</param>
		public override void Add(TElement element)
		{
			ArgumentVerifier.CantBeNull(element, "element");

			// use binary search to find the spot where to insert the element, then insert the element using List<T>'s Insert() method which uses
			// native block-copy code internally for performance. We'll use .NET's binarySearch implementation with our own comparer.
			if(this.Count == 0)
			{
				_elements.Add(element);
				return;
			}
			ComparisonBasedComparer<TElement> comparer = new ComparisonBasedComparer<TElement>(this.PriorityComparison) { FlipCompareResult = true };
			// as the list has to be sorted descending, we have to flip the comparer's result
			int indexToUse = _elements.BinarySearch(element, comparer);
			if(indexToUse < 0)
			{
				// use bitwise complement of index. See List<T>.BinarySearch documentation in MSDN for details.
				_elements.Insert(~indexToUse, element);
			}
			else
			{
				_elements.Insert(indexToUse, element);
			}
		}


		/// <summary>
		/// Removes the first element from the queue, if the queue isn't empty.
		/// </summary>
		/// <returns>
		/// the first element of the queue or the default value for TElement if the queue is empty
		/// </returns>
		public override TElement Remove()
		{
			TElement toReturn = default(TElement);
			if(this.Count > 0)
			{
				toReturn = this.Peek();
				_elements.RemoveAt(0);
			}
			return toReturn;
		}


		/// <summary>
		/// Clears the queue.
		/// </summary>
		public override void Clear()
		{
			_elements.Clear();
		}


		/// <summary>
		/// Determines whether the queue contains the element specified
		/// </summary>
		/// <param name="element">The element to check.</param>
		/// <returns>
		/// true if elementToCheck is in the queue, false otherwise
		/// </returns>
		public override bool Contains(TElement element)
		{
			return _elements.Contains(element);
		}


		/// <summary>
		/// Gets the enumerator for this queue
		/// </summary>
		/// <returns>the enumerator to use over this queue</returns>
		protected override IEnumerator<TElement> GetEnumerator()
		{
			return ((IEnumerable<TElement>)_elements).GetEnumerator();
		}


		/// <summary>
		/// Inits the data structures of this priority queue
		/// </summary>
		private void InitDataStructures()
		{
			_elements = new List<TElement>();
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the number of elements in the queue
		/// </summary>
		/// <value></value>
		public override int Count
		{
			get { return _elements.Count; }
		}
		#endregion
	}
}
