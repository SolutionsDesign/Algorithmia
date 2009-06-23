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
using SD.Tools.Algorithmia.Heaps;

namespace SD.Tools.Algorithmia.PriorityQueues
{
	/// <summary>
	/// Priority queue class based on a binary heap which is set as a max heap, as a priority queue always has the element with the highest value (key) as the first
	/// element
	/// </summary>
	/// <typeparam name="TElement">The type of the elements in the queue</typeparam>
	/// <remarks>Heaps don't allow enumeration (as enumeration destroys the heap as the elements have to be extracted), so enumerating this
	/// priority queue will throw an exception.
	/// <para>Heaps can't store value types. This is because changing a value type really makes it a different value, while changing the contents of an object doesn't
	/// make it a different object. This priority queue, based on a heap, can't store value types either.
	/// </para>
	/// </remarks>
	public class BinaryHeapPriorityQueue<TElement> : PriorityQueueBase<TElement>
		where TElement : class
	{
		#region Class Member Declarations
		private BinaryHeap<TElement>	_heap;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryHeapPriorityQueue&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="priorityComparison">The priority comparison.</param>
		public BinaryHeapPriorityQueue(Comparison<TElement> priorityComparison) : base(priorityComparison)
		{
		}


		/// <summary>
		/// Peeks at the first element of the queue (the one with the highest priority, according to the priority comparison) and returns that element
		/// without removing it
		/// </summary>
		/// <returns></returns>
		/// the first element in the queue or the default value for TElement if the queue is empty
		public override TElement Peek()
		{
			return _heap.Root;
		}


		/// <summary>
		/// Adds the specified element to the queue
		/// </summary>
		/// <param name="element">The element to add.</param>
		public override void Add(TElement element)
		{
			_heap.Insert(element);
		}


		/// <summary>
		/// Removes the first element from the queue, if the queue isn't empty.
		/// </summary>
		/// <returns>
		/// the first element of the queue or the default value for TElement if the queue is empty
		/// </returns>
		public override TElement Remove()
		{
			return _heap.ExtractRoot();
		}


		/// <summary>
		/// Clears the queue.
		/// </summary>
		public override void Clear()
		{
			_heap.Clear();
		}


		/// <summary>
		/// Determines whether the queue contains the element specified
		/// </summary>
		/// <param name="elementToCheck">The element to check.</param>
		/// <returns>
		/// true if elementToCheck is in the queue, false otherwise
		/// </returns>
		public override bool Contains(TElement elementToCheck)
		{
			return _heap.Contains(elementToCheck);
		}


		/// <summary>
		/// Inits the data structures of this priority queue
		/// </summary>
		protected override void InitDataStructures()
		{
			_heap = new BinaryHeap<TElement>(this.PriorityComparison, false);		
		}


		/// <summary>
		/// Throws a NotSupportException, as binary heaps can't be enumerated.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerator<TElement> GetEnumerator()
		{
			throw new NotSupportedException("A binary heap can't be enumerated");
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the number of elements in the queue
		/// </summary>
		public override int Count
		{
			get { return _heap.Count; }
		}
		#endregion
	}
}
