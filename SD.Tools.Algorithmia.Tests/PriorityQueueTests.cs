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

using SD.Tools.Algorithmia;
using SD.Tools.Algorithmia.PriorityQueues;
using SD.Tools.Algorithmia.Sorting;
using NUnit.Framework;
using System.ComponentModel;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Class with unittests for the priority queues.
	/// </summary>
	[TestFixture]
	public class PriorityQueueTests
	{
		#region Local classes
		/// <summary>
		/// Simple class which is used to test the priority queues functionality.
		/// </summary>
		public class QueueElement
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="QueueElement"/> class.
			/// </summary>
			/// <param name="priority">The priority.</param>
			/// <param name="elementValue">The element value.</param>
			public QueueElement(int priority, string elementValue)
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
			public int Priority { get; set; }
			/// <summary>
			/// value of element.
			/// </summary>
			public string ElementValue { get; set; }
		}
		#endregion


		/// <summary>
		/// Test which checks the functionality of the SimplePriorityQueue. 
		/// </summary>
		[Test]
		public void SimplePriorityQueueFunctionalityTest()
		{
			// create the queue and specify the comparison func. This is a simple lambda which returns the comparison on priorities
			SimplePriorityQueue<QueueElement> queue = new SimplePriorityQueue<QueueElement>((a, b) => a.Priority.CompareTo(b.Priority));

			// use the generic method for all queues.
			PriorityQueueFunctionalityTest(queue, 100, true);
		}


		/// <summary>
		/// Test which checks the functionality of the SortedPriorityQueue. 
		/// </summary>
		[Test]
		public void SortedPriorityQueueFunctionalityTest()
		{
			// create the queue and specify the comparison func. This is a simple lambda which returns the comparison on priorities
			SortedPriorityQueue<QueueElement> queue = new SortedPriorityQueue<QueueElement>((a, b) => a.Priority.CompareTo(b.Priority));

			// use the generic method for all queues.
			PriorityQueueFunctionalityTest(queue, 100, true);
		}


		/// <summary>
		/// Test which checks the functionality of the BinaryHeapPriorityQueue. No enumeration is tested, as BinaryHeapPriorityQueue doesn't support
		/// enumeration.
		/// </summary>
		[Test]
		public void BinaryHeapPriorityQueueFunctionalityTest()
		{
			// create the queue and specify the comparison func. This is a simple lambda which returns the comparison on priorities
			BinaryHeapPriorityQueue<QueueElement> queue = new BinaryHeapPriorityQueue<QueueElement>((a, b) => a.Priority.CompareTo(b.Priority));

			// use the generic method for all queues.
			PriorityQueueFunctionalityTest(queue, 100, false);
		}


		/// <summary>
		/// Tests the functionality of the queue passed in.
		/// </summary>
		/// <param name="queue">The queue.</param>
		/// <param name="numberOfElements">The number of elements.</param>
		/// <param name="performEnumerateTest">if set to <c>true</c> [perform enumerate test].</param>
		private static void PriorityQueueFunctionalityTest(PriorityQueueBase<QueueElement> queue, int numberOfElements, bool performEnumerateTest)
		{
			// create list of random priorities.
			List<int> priorities = new List<int>();
			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			for(int i = 0; i < numberOfElements; i++)
			{
				priorities.Add(rand.Next(numberOfElements));
			}

			// fill queue with elements
			for(int i = 0; i < numberOfElements; i++)
			{
				// add a new element to the queue, with a random priority from the list and a value.
				queue.Add(new QueueElement(priorities[i], "Value: " + i));
			}

			Assert.AreEqual(numberOfElements, queue.Count);

			// sort the priorities, descending, so we know the highest priority
			priorities.Sort(SortAlgorithm.ShellSort, SortDirection.Descending);

			// check if peek reveals the highest priority
			QueueElement highestPriorityElement = queue.Peek();
			Assert.IsNotNull(highestPriorityElement);
			Assert.AreEqual(priorities[0], highestPriorityElement.Priority);

			QueueElement current;
			QueueElement previous;
			if(performEnumerateTest)
			{
				// test the enumerator to see if it gives back the elements in the right order
				current = highestPriorityElement;
				foreach(QueueElement element in queue)
				{
					previous = current;
					current = element;
					Assert.IsTrue(previous.Priority >= current.Priority);
				}
			}

			// now remove all elements one by one. Each element has to have a lower (or equal) priority than the one before it.
			previous = queue.Remove();
			current = queue.Remove();
			while(current != null)
			{
				Assert.IsTrue(previous.Priority >= current.Priority);
				previous = current;
				current = queue.Remove();
			}

			// queue is now empty
			Assert.AreEqual(0, queue.Count);

			Assert.IsNull(queue.Peek());
		}
	}
}
