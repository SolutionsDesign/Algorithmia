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

using SD.Tools.Algorithmia.Sorting;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.PriorityQueues
{
	/// <summary>
	/// Simple priority queue which uses a simple list for storing the elements. Inserts are very fast, O(1), but lookups are done in linear time, O(n).
	/// </summary>
	/// <typeparam name="TElement">The type of the elements in this queue</typeparam>
	/// <remarks>If you store value types in this queue, be aware that Peek() and Remove() on an empty queue will return the default value for
	/// the value type used, e.g. 0 for a priority queue with Int32 values.</remarks>
	public class SimplePriorityQueue<TElement> : PriorityQueueBase<TElement>
	{
		#region Class Member Declarations
		private List<TElement> _elements;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="SimplePriorityQueue&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="priorityComparison">The priority comparison.</param>
		public SimplePriorityQueue(Comparison<TElement> priorityComparison) :
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
				// find the element which is the root of the queue. This is O(n) worse case. It can be optimized by caching the element
				// with the highest priority when it's added, however removing the element from the queue (which is the typical behavior where Peek is used for)
				// again will require to find the then highest priority, which in short means that the optimization is mitigated by the fact that the search for
				// the new highest priority is still O(n).

				// use the first element as the start value.
				TElement elementWithHighestPriority = _elements[0];

				for(int i=1;i<_elements.Count;i++)
				{
					TElement element = _elements[i];
					if(this.PriorityComparison(element, elementWithHighestPriority)>0)
					{
						// found new bigger value
						elementWithHighestPriority = element;
					}
				}

				toReturn = elementWithHighestPriority;
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
			_elements.Add(element);
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
				_elements.Remove(toReturn);
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
			// we simply sort the list in a new construct using ShellSort and enumerate over that sorted list.
			// This is faster than finding the new biggest value over and over again.
			List<TElement> elementsAsList = new List<TElement>(_elements);
			elementsAsList.Sort(SortAlgorithm.ShellSort, SortDirection.Descending, this.PriorityComparison);
			return ((IEnumerable<TElement>)elementsAsList).GetEnumerator();
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
