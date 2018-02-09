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

using SD.Tools.Algorithmia;
using SD.Tools.Algorithmia.Heaps;
using SD.Tools.Algorithmia.Sorting;
using NUnit.Framework;
using System.ComponentModel;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Class with unittests for the heaps. PropertyQueuesTests also contains some tests with heaps, namely the tests which target priority queues based on
	/// a heap.
	/// </summary>
	[TestFixture]
	public class HeapTests
	{
		/// <summary>
		/// Simple class which is used to test the heaps functionality.
		/// </summary>
		public class HeapElement
		{
			private int _priority;

			/// <summary>
			/// Initializes a new instance of the <see cref="HeapElement"/> class.
			/// </summary>
			/// <param name="priority">The priority.</param>
			/// <param name="elementValue">The element value.</param>
			public HeapElement(int priority, string elementValue)
			{
				this.Priority = priority;
				this.ElementValue = elementValue;
			}

			/// <summary>
			/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
			/// </returns>
			public override string ToString()
			{
				return string.Format("Priority: {0}. {1}", this.Priority, this.ElementValue);
			}

			/// <summary>
			/// Gets or sets the priority for this element
			/// </summary>
			public int Priority 
			{
				get { return _priority; }
				set { _priority = value; }
			}
			/// <summary>
			/// value of element.
			/// </summary>
			public string ElementValue { get; set; }
		}


		/// <summary>
		/// Tests a merge between two binary max heaps.
		/// </summary>
		[Test]
		public void BinaryHeapMergeTestMaxHeaps()
		{
			// create two maxheaps based on the priority value of the elements. 
			BinaryHeap<HeapElement> heap1 = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);
			BinaryHeap<HeapElement> heap2 = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);

			FillHeap(heap1, 100);
			FillHeap(heap2, 100);
			Assert.AreEqual(100, heap1.Count);
			Assert.AreEqual(100, heap2.Count);
			// merge heap1 into heap2. heap2 will be empty afterwards
			heap1.Merge(heap2);
			Assert.AreEqual(200, heap1.Count);
			Assert.AreEqual(0, heap2.Count);

			// check if they are inserted correctly
			HeapElement previous = heap1.ExtractRoot();
			HeapElement current = heap1.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority >= current.Priority);
				previous = current;
				current = heap1.ExtractRoot();
			}
			// heap1 should be empty as well
			Assert.AreEqual(0, heap1.Count);
		}


		/// <summary>
		/// Tests a merge between two binary min heaps.
		/// </summary>
		[Test]
		public void BinaryHeapMergeTestMinHeaps()
		{
			// create two minheaps based on the priority value of the elements. 
			BinaryHeap<HeapElement> heap1 = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);
			BinaryHeap<HeapElement> heap2 = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);

			FillHeap(heap1, 100);
			FillHeap(heap2, 100);
			Assert.AreEqual(100, heap1.Count);
			Assert.AreEqual(100, heap2.Count);
			// merge heap1 into heap2. heap2 will be empty afterwards
			heap1.Merge(heap2);
			Assert.AreEqual(200, heap1.Count);
			Assert.AreEqual(0, heap2.Count);

			// check if they are inserted correctly
			HeapElement previous = heap1.ExtractRoot();
			HeapElement current = heap1.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority <= current.Priority);
				previous = current;
				current = heap1.ExtractRoot();
			}
			// heap1 should be empty as well
			Assert.AreEqual(0, heap1.Count);
		}


		/// <summary>
		/// Tests an UpdateKey action on an element in the max heap.
		/// </summary>
		[Test]
		public void BinaryHeapUpdateKeyMaxHeap()
		{
			BinaryHeap<HeapElement> heap = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);
			FillHeap(heap, 100);
			Assert.AreEqual(100, heap.Count);

			// add new element. Give it a priority of 50
			HeapElement element = new HeapElement(50, "Value: 50");
			heap.Insert(element);
			Assert.AreEqual(101, heap.Count);

			// update key to 70, using an action lambda
			heap.UpdateKey(element, (a, b) => a.Priority = b, 70);
			Assert.AreEqual(70, element.Priority);
			
			// check if the heap is still correct
			HeapElement previous = heap.ExtractRoot();
			HeapElement current = heap.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority >= current.Priority);
				previous = current;
				current = heap.ExtractRoot();
			}
			// heap should be empty 
			Assert.AreEqual(0, heap.Count);
		}



		/// <summary>
		/// Tests an UpdateKey action on an element in the min heap.
		/// </summary>
		[Test]
		public void BinaryHeapUpdateKeyMinHeap()
		{
			BinaryHeap<HeapElement> heap = new BinaryHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);

			FillHeap(heap, 100);
			Assert.AreEqual(100, heap.Count);

			// add new element. Give it a priority of 50
			HeapElement element = new HeapElement(50, "Value: 50");
			heap.Insert(element);
			Assert.AreEqual(101, heap.Count);

			// update key to 10, using an action lambda
			heap.UpdateKey(element, (a, b) => a.Priority = b, 10);
			Assert.AreEqual(10, element.Priority);

			// check if the heap is still correct
			HeapElement previous = heap.ExtractRoot();
			HeapElement current = heap.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority <= current.Priority);
				previous = current;
				current = heap.ExtractRoot();
			}
			// heap should be empty 
			Assert.AreEqual(0, heap.Count);
		}
		

		/// <summary>
		/// Tests a merge between two Fibonacci max heaps.
		/// </summary>
		[Test]
		public void FibonacciHeapMergeTestMaxHeaps()
		{
			// create two maxheaps based on the priority value of the elements. 
			FibonacciHeap<HeapElement> heap1 = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);
			FibonacciHeap<HeapElement> heap2 = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);

			FillHeap(heap1, 100);
			FillHeap(heap2, 100);
			Assert.AreEqual(100, heap1.Count);
			Assert.AreEqual(100, heap2.Count);
			// merge heap1 into heap2. heap2 will be empty afterwards
			heap1.Merge(heap2);
			Assert.AreEqual(200, heap1.Count);
			Assert.AreEqual(0, heap2.Count);

			// check if they are inserted correctly
			HeapElement previous = heap1.ExtractRoot();
			HeapElement current = heap1.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority >= current.Priority);
				previous = current;
				current = heap1.ExtractRoot();
			}
			// heap1 should be empty as well
			Assert.AreEqual(0, heap1.Count);
		}


		/// <summary>
		/// Tests a merge between two Fibonacci min heaps.
		/// </summary>
		[Test]
		public void FibonacciHeapMergeTestMinHeaps()
		{
			// create two minheaps based on the priority value of the elements. 
			FibonacciHeap<HeapElement> heap1 = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);
			FibonacciHeap<HeapElement> heap2 = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);

			FillHeap(heap1, 100);
			FillHeap(heap2, 100);
			Assert.AreEqual(100, heap1.Count);
			Assert.AreEqual(100, heap2.Count);
			// merge heap1 into heap2. heap2 will be empty afterwards
			heap1.Merge(heap2);
			Assert.AreEqual(200, heap1.Count);
			Assert.AreEqual(0, heap2.Count);

			// check if they are inserted correctly
			HeapElement previous = heap1.ExtractRoot();
			HeapElement current = heap1.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority <= current.Priority);
				previous = current;
				current = heap1.ExtractRoot();
			}
			// heap1 should be empty as well
			Assert.AreEqual(0, heap1.Count);
		}


		/// <summary>
		/// Tests an UpdateKey action on an element in the max heap.
		/// </summary>
		[Test]
		public void FibonacciHeapUpdateKeyMaxHeap()
		{
			FibonacciHeap<HeapElement> heap = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), false);
			FillHeap(heap, 100);
			Assert.AreEqual(100, heap.Count);

			// add new element. Give it a priority of 50
			HeapElement element = new HeapElement(50, "Value: 50");
			heap.Insert(element);
			Assert.AreEqual(101, heap.Count);

			// update key to 70, using an action lambda
			heap.UpdateKey(element, (a, b) => a.Priority = b, 70);
			Assert.AreEqual(70, element.Priority);

			// check if the heap is still correct
			HeapElement previous = heap.ExtractRoot();
			HeapElement current = heap.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority >= current.Priority);
				previous = current;
				current = heap.ExtractRoot();
			}
			// heap should be empty 
			Assert.AreEqual(0, heap.Count);
		}



		/// <summary>
		/// Tests an UpdateKey action on an element in the min heap.
		/// </summary>
		[Test]
		public void FibonacciHeapUpdateKeyMinHeap()
		{
			FibonacciHeap<HeapElement> heap = new FibonacciHeap<HeapElement>((a, b) => a.Priority.CompareTo(b.Priority), true);

			FillHeap(heap, 100);
			Assert.AreEqual(100, heap.Count);

			// add new element. Give it a priority of 50
			HeapElement element = new HeapElement(50, "Value: 50");
			heap.Insert(element);
			Assert.AreEqual(101, heap.Count);

			// update key to 10, using an action lambda
			heap.UpdateKey(element, (a, b) => a.Priority = b, 10);
			Assert.AreEqual(10, element.Priority);

			// check if the heap is still correct
			HeapElement previous = heap.ExtractRoot();
			HeapElement current = heap.ExtractRoot();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority <= current.Priority);
				previous = current;
				current = heap.ExtractRoot();
			}
			// heap should be empty 
			Assert.AreEqual(0, heap.Count);
		}

	
		/// <summary>
		/// Fills the heap with random elements
		/// </summary>
		/// <param name="heap">The heap.</param>
		/// <param name="numberOfElements">The number of elements.</param>
		private static void FillHeap(HeapBase<HeapElement> heap, int numberOfElements)
		{
			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));

			// fill heap with elements
			for(int i = 0; i < numberOfElements; i++)
			{
				// add a new element to the heap, with a random priority from the list and a value.
				heap.Insert(new HeapElement(rand.Next(numberOfElements), "Value: " + i));
			}
		}
	}
}
