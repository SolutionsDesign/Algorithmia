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
using System.Linq;
using System.Text;

namespace SD.Tools.Algorithmia.Heaps
{
	/// <summary>
	/// General base class for all Heap classes. A heap is a specialized tree-based datastructure which has its elements ordered in such a way that
	/// every parent is larger / equal than its children, if the heap is a max-heap and in the case of a min-heap every parent is smaller/equal than its
	/// children. See: http://en.wikipedia.org/wiki/Heap_%28data_structure%29<br/>
	/// The heap classes implement the basic operations on a heap: delete max/min, increase/decrease key, insert and merge. 
	/// </summary>
	/// <remarks>
	/// As this class can both handle min heaps as well as max heaps, the delete max/min is called 'ExtractRoot'. The increase/decrease key method is called
	/// 'UpdateKey'.
	/// <para>
	/// Heaps can't store value types. This is because changing a value type really makes it a different value, while changing the contents of an object doesn't
	/// make it a different object.
	/// </para>
	/// </remarks>
	/// <typeparam name="TElement">The type of the elements in this heap</typeparam>
	public abstract class HeapBase<TElement>
		where TElement : class
	{
		#region Class Member Declarations
		private readonly Comparison<TElement> _keyCompareFunc;
		private readonly bool _isMinHeap;
		private readonly Func<TElement, TElement, bool> _elementCompareFunc;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="HeapBase&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="keyCompareFunc">The key compare func, which is used to compare two elements based on their key. Based on the type of the heap, min heap
		/// or max heap, the first element is placed as parent or as child: if this heap is a min-heap, and the first element, according to keyCompareFunc is
		/// bigger than the second element, the second element will be the parent of the first element. In a max-heap, the first element will be the parent of
		/// the second element in that situation.</param>
		/// <param name="isMinHeap">Flag to signal if this heap is a min heap or a max heap</param>
		protected HeapBase(Comparison<TElement> keyCompareFunc, bool isMinHeap)
		{
			_keyCompareFunc = keyCompareFunc;
			_isMinHeap = isMinHeap;

			// create element compare func to compare elements based on the type of the heap. 
			if(_isMinHeap)
			{
				// a min heap has the element with the lower key value as the parent of the element with the higher key value. 
				_elementCompareFunc = (a, b) => (_keyCompareFunc(a, b) <= 0);
			}
			else
			{
				// a max heap has the element with the higher key value as the parent of the element with the lower key value. 
				_elementCompareFunc = (a, b) => (_keyCompareFunc(a, b) >= 0);
			}
		}


		/// <summary>
		/// Extracts and removes the root of the heap.
		/// </summary>
		/// <returns>the root element deleted, or null/default if the heap is empty</returns>
		public abstract TElement ExtractRoot();
		/// <summary>
		/// Updates the key of the element passed in. Only use this method for elements where the key is a property of the element, not the element itself.
		/// This means that if you have a heap with value typed elements (e.g. integers), updating the key is updating the value of the element itself, and because
		/// a heap might contain elements with the same value, this could lead to undefined results.
		/// </summary>
		/// <typeparam name="TKeyType">The type of the key type.</typeparam>
		/// <param name="element">The element which key has to be updated</param>
		/// <param name="keyUpdateFunc">The key update func, which takes 2 parameters: the element to update and the new key value.</param>
		/// <param name="newValue">The new value for the key.</param>
		/// <remarks>The element to update is first looked up in the heap. If the new key violates the heap property (e.g. the key is bigger than the
		/// key of the parent in a max-heap) the elements in the heap are restructured in such a way that the heap again obeys the heap property. </remarks>
		public abstract void UpdateKey<TKeyType>(TElement element, Action<TElement, TKeyType> keyUpdateFunc, TKeyType newValue);
		/// <summary>
		/// Inserts the specified element into the heap at the right spot
		/// </summary>
		/// <param name="element">The element to insert.</param>
		public abstract void Insert(TElement element);
		/// <summary>
		/// Clears all elements from the heap
		/// </summary>
		public abstract void Clear();
		/// <summary>
		/// Removes the element specified
		/// </summary>
		/// <param name="element">The element to remove.</param>
		public abstract void Remove(TElement element);
		/// <summary>
		/// Determines whether this heap contains the element specified
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>true if the heap contains the element specified, false otherwise</returns>
		public abstract bool Contains(TElement element);


		#region Class Property Declarations
		/// <summary>
		/// Gets the root of the heap. Depending on the fact if this is a min or max heap, it returns the element with the minimum key (min heap) or the element
		/// with the maximum key (max heap). If the heap is empty, null / default is returned
		/// </summary>
		public abstract TElement Root { get; }
		/// <summary>
		/// Gets the number of elements in the heap.
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// Gets the key compare func to compare elements based on key.
		/// </summary>
		protected Comparison<TElement> KeyCompareFunc
		{
			get { return _keyCompareFunc; }
		}

		/// <summary>
		/// Gets the element compare func, which is the func to compare two elements based on the heap type: the function returns true if the first element
		/// is indeed the correct parent of the second element or false if not.
		/// </summary>
		protected Func<TElement, TElement, bool> ElementCompareFunc
		{
			get { return _elementCompareFunc; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is a min heap (true) or a max heap (false)
		/// </summary>
		public bool IsMinHeap
		{
			get { return _isMinHeap; }
		}
		#endregion
	}
}
