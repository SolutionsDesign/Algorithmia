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
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.Heaps
{
	/// <summary>
	/// Simple heap which is based on a binary tree: the binary heap. See: http://en.wikipedia.org/wiki/Binary_heap
	/// Binary heaps are quite efficient, however if you're doing a lot of merging between heaps, the Binary heap is inefficient. Consider a Fibonacci heap
	/// instead in these situations.
	/// </summary>
	/// <remarks>Heaps can't store value types. This is because changing a value type really makes it a different value, while changing the contents of an object doesn't
	/// make it a different object.
	/// </remarks>
	/// <typeparam name="TElement">The type of the elements in this heap</typeparam>
	public class BinaryHeap<TElement> : HeapBase<TElement>
		where TElement: class
	{
		#region Class Member Declarations
		private List<TElement> _elements;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryHeap&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="keyCompareFunc">The key compare func, which is used to compare two elements based on their key. Based on the type of the heap, min heap
		/// or max heap, the first element is placed as parent or as child: if this heap is a min-heap, and the first element, according to keyCompareFunc is
		/// bigger than the second element, the second element will be the parent of the first element. In a max-heap, the first element will be the parent of
		/// the second element in that situation.</param>
		/// <param name="isMinHeap">Flag to signal if this heap is a min heap or a max heap</param>
		public BinaryHeap(Comparison<TElement> keyCompareFunc, bool isMinHeap) : base(keyCompareFunc, isMinHeap)
		{
			InitDataStructures();
		}


		/// <summary>
		/// Extracts and removes the root of the heap.
		/// </summary>
		/// <returns>
		/// the root element deleted, or null/default if the heap is empty
		/// </returns>
		public override TElement ExtractRoot()
		{
			TElement toReturn = this.Root;

			if(_elements.Count > 0)
			{
				RemoveAt(0);
			}
			return toReturn;
		}


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
		public override void UpdateKey<TKeyType>(TElement element, Action<TElement, TKeyType> keyUpdateFunc, TKeyType newValue)
		{
			int currentIndex = FindIndexOfElement(element);
			if(currentIndex < 0)
			{
				return;
			}

			// set the new key value. This could invalidate the heap.
			keyUpdateFunc(element, newValue);
			
			// execute an Upheap to see if the parent should be the child of the element based on the new key. 
			// If the index of the element changed, we're done, if not, execute a downheap. 
			Upheap(currentIndex);
			int newIndex = FindIndexOfElement(element);
			if(newIndex == currentIndex)
			{
				// Upheap did not change its position, check using downheap
				Downheap(currentIndex);
			}
		}


		/// <summary>
		/// Inserts the specified element into the heap at the right spot
		/// </summary>
		/// <param name="element">The element to insert.</param>
		public override void Insert(TElement element)
		{
			ArgumentVerifier.CantBeNull(element, "element");
			_elements.Add(element);
			Upheap(_elements.Count - 1);
		}


		/// <summary>
		/// Merges the specified heap into this heap.
		/// </summary>
		/// <param name="toMerge">The heap to merge into this heap. This heap will be empty after the merge</param>
		/// <remarks>Merging is expensive with this heap implementation. Consider another heap class if you are doing a lot of merging</remarks>
		public void Merge(BinaryHeap<TElement> toMerge)
		{
			if(toMerge == null)
			{
				return;
			}
			if(toMerge.Count > 0)
			{
				// walk the heap to merge by simply extract the root till there are no elements left. toMerge will be empty after this operation.
				do
				{
					TElement toAdd = toMerge.ExtractRoot();
					Insert(toAdd);
				}
				while(toMerge.Count > 0);
			}
		}


		/// <summary>
		/// Clears all elements from the heap
		/// </summary>
		public override void Clear()
		{
			_elements.Clear();
		}


		/// <summary>
		/// Removes the element specified
		/// </summary>
		/// <param name="element">The element to remove.</param>
		public override void Remove(TElement element)
		{
			int indexOfElement = FindIndexOfElement(element);
			if(indexOfElement < 0)
			{
				return;
			}

			RemoveAt(indexOfElement);
		}


		/// <summary>
		/// Determines whether this heap contains the element specified
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		/// true if the heap contains the element specified, false otherwise
		/// </returns>
		public override bool Contains(TElement element)
		{
			return (FindIndexOfElement(element) >= 0);
		}


		/// <summary>
		/// Inits the data structures for this heap.
		/// </summary>
		private void InitDataStructures()
		{
			_elements = new List<TElement>();
		}


		/// <summary>
		/// Finds the index of the element passed in. If element is a valuetyped object, e.g. an integer, it means that there can be more elements with the same
		/// value in the heap. In that case, the index of the first element found is returned.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>the index of the element passed in in the _elements array, or -1 if not found</returns>
		private int FindIndexOfElement(TElement element)
		{
			return element == null ? -1 : FindIndexOfElement(element, 0);
		}


		/// <summary>
		/// Finds the index of the element passed in. If element is a valuetyped object, e.g. an integer, it means that there can be more elements with the same
		/// value in the heap. In that case, the index of the first element found is returned.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="index">The current index to consider.</param>
		/// <returns>
		/// the index of the element passed in in the _elements array, or -1 if not found
		/// </returns>
		private int FindIndexOfElement(TElement element, int index)
		{
			if(element == null)
			{
				return -1;
			}

			int toReturn = -1;
			if(index < _elements.Count)
			{
				// check if this element is equal to the passed in element
				bool areEqual = element.Equals(_elements[index]);
				if(!areEqual)
				{
					// check children
					int indexLeftChild = (2 * index) + 1;
					if(indexLeftChild < _elements.Count)
					{
						// has left child. Check if the left child would be a parent of this element. If not, skip it, otherwise, dig into that tree.
						if(this.ElementCompareFunc( _elements[indexLeftChild], element))
						{
							// dig into left child's tree
							int leftResult = FindIndexOfElement(element, indexLeftChild);
							if(leftResult >= 0)
							{
								// found it
								toReturn = leftResult;
							}
						}
						if(toReturn<0)
						{
							// consider right branch, as element isn't in left child's tree. 
							int indexRightChild = (2 * index) + 2;
							if(indexRightChild < _elements.Count)
							{
								// has right child. Check if the right child would be a parent of this element. If not, skip it, otherwise, dig into that tree.
								if(this.ElementCompareFunc(_elements[indexRightChild], element))
								{
									// dig into right child's tree
									int rightResult = FindIndexOfElement(element, indexRightChild);
									if(rightResult >= 0)
									{
										// found it
										toReturn = rightResult;
									}
								}
							}
						}
					}
				}
				else
				{
					toReturn = index;
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Checks the element at the index specified to see if it is at the wrong spot. If so, it is exchanged with its parent till the root is reached or the heap
		/// is already stable.
		/// </summary>
		/// <param name="index">The index.</param>
		private void Upheap(int index)
		{
			if(index==0)
			{
				// root, done
				return;
			}

			int parentIndex = (index - 1) / 2;
			if(!this.ElementCompareFunc(_elements[parentIndex], _elements[index]))
			{
				// parent shouldn't be the parent of this element. Swap
				_elements.SwapValues(parentIndex, index);
				Upheap(parentIndex);
			}
		}


		/// <summary>
		/// Checks the element at the index specified to see if it is at the wrong spot. If so, it is exchanged with it's larger child till the last element is
		/// reached or the heap is stable
		/// </summary>
		/// <param name="index">The index.</param>
		private void Downheap(int index)
		{
			int indexLeftChild = (2 * index) + 1;
			int indexRightChild = (2 * index) + 2;
			if(indexLeftChild >= _elements.Count)
			{
				// element with passed in index doesn't have children. Done
				return;
			}

			// by default pick the left child (as it is always there IF there are children). If there is a right child, we check which one of the two would be
			// the parent of the other. That element is then used.
			bool selectRightChild = false;
			if(indexRightChild < _elements.Count)
			{
				// there is a right child, check which of the two children would be the parent of one another. We'll check if the right one would be the parent.
				selectRightChild = this.ElementCompareFunc(_elements[indexRightChild], _elements[indexLeftChild]);
			}

			int indexToCompareWith = indexLeftChild;
			if(selectRightChild)
			{
				indexToCompareWith = indexRightChild;
			}

			// we now have two indexes. We've to check if the passed in index is correctly the parent of the indexToCompareWith element. If not, swap and proceed
			// otherwise, we're done
			if(!this.ElementCompareFunc(_elements[index], _elements[indexToCompareWith]))
			{
				// elements have to be swapped
				_elements.SwapValues(index, indexToCompareWith);
				// dig deeper
				Downheap(indexToCompareWith);
			}
		}


		/// <summary>
		/// Removes the element at the index passed in from the heap and restructures the elements to fulfill the heap property again.
		/// </summary>
		private void RemoveAt(int index)
		{
			// replace element to remove with last element in the heap, then remove last element from the list of elements and then downheap the element at index
			switch(_elements.Count)
			{
				case 0:
					return;
				case 1:
				case 2:
					// simply remove the element
					_elements.RemoveAt(index);
					return;
				default:
					// swap element at index and last element, then remove last element (which is the element to remove)
					_elements.SwapValues(index, _elements.Count - 1);
					_elements.RemoveAt(_elements.Count - 1);
					// now simply downheap the element at index specified. Downheap will take care of the rest. We need a downheap because the element
					// we swapped into the 'index' location is always a child of the elements which are potentially higher up the tree. We now only have
					// to take care of the fact if the element now located at 'index' is at the right place or should be a child of one of its new children, 
					// something which is been taken care of by Downheap(index).
					Downheap(index);
					break;
			}
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the root of the heap. Depending on the fact if this is a min or max heap, it returns the element with the minimum key (min heap) or the element
		/// with the maximum key (max heap). If the heap is empty, null / default is returned
		/// </summary>
		public override TElement Root
		{
			get 
			{ 
				TElement toReturn = default(TElement);
				if(_elements.Count > 0)
				{
					toReturn = _elements[0];
				}
				return toReturn;
			}
		}


		/// <summary>
		/// Gets the number of elements in the heap.
		/// </summary>
		public override int Count
		{
			get { return _elements.Count; }
		}
		#endregion
	}
}
