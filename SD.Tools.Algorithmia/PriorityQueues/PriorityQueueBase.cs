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
using System.Linq;
using System.Text;
using System.Collections;

namespace SD.Tools.Algorithmia.PriorityQueues
{
	/// <summary>
	/// Base class for all priority queues. A priority queue is a special queue: the elements added to the queue are stored in the order of the priority
	/// they have. Grabbing the first element from a queue is therefore grabbing the element with the highest priority.
	/// See: http://en.wikipedia.org/wiki/Priority_queue
	/// </summary>
	/// <typeparam name="TElement">the type of the element in the queue</typeparam>
	/// <remarks>If you store value types in this queue, be aware that Peek() and Remove() on an empty queue will return the default value for
	/// the value type used, e.g. 0 for a priority queue with Int32 values.</remarks>
	public abstract class PriorityQueueBase<TElement> : IEnumerable<TElement>
	{
		#region Class Member Declarations
		private readonly Comparison<TElement> _priorityComparison;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="PriorityQueueBase&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="priorityComparison">The priority comparison.</param>
		protected PriorityQueueBase(Comparison<TElement> priorityComparison)
		{
			_priorityComparison = priorityComparison;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}


		/// <summary>
		/// Peeks at the first element of the queue (the one with the highest priority, according to the priority comparison) and returns that element
		/// without removing it
		/// </summary>
		/// the first element in the queue or the default value for TElement if the queue is empty
		public abstract TElement Peek();
		/// <summary>
		/// Adds the specified element to the queue
		/// </summary>
		/// <param name="element">The element to add.</param>
		public abstract void Add(TElement element);
		/// <summary>
		/// Removes the first element from the queue, if the queue isn't empty. 
		/// </summary>
		/// <returns>the first element of the queue or the default value for TElement if the queue is empty</returns>
		public abstract TElement Remove();
		/// <summary>
		/// Clears the queue.
		/// </summary>
		public abstract void Clear();
		/// <summary>
		/// Determines whether the queue contains the element specified
		/// </summary>
		/// <param name="element">The element to check.</param>
		/// <returns>true if elementToCheck is in the queue, false otherwise</returns>
		public abstract bool Contains(TElement element);

		/// <summary>
		/// Gets the enumerator for this queue
		/// </summary>
		/// <returns>the enumerator to use over this queue</returns>
		protected abstract IEnumerator<TElement> GetEnumerator();


		#region Class Property Declarations
		/// <summary>
		/// Gets the priority comparison.
		/// </summary>
		protected Comparison<TElement> PriorityComparison
		{
			get { return _priorityComparison; }
		}

		/// <summary>
		/// Gets the number of elements in the queue
		/// </summary>
		public abstract int Count { get; }
		#endregion
	}
}
