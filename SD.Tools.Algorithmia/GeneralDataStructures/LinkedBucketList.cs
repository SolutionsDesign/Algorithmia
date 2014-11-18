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

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Simple doubly linked list which doesn't suffer from the problem that one can't concat two linked lists in O(1). The .NET LinkedList class
	/// can't be used to fast connect two LinkedLists together without traversing all nodes of one of them (as each node has a reference to its containing list)
	/// </summary>
	/// <typeparam name="T">Type of contents in the buckets in this list</typeparam>
	/// <remarks>One could use the ListBucket class on its own as a raw linked list, however this class provides more utility code one will need
	/// anyway to use ListBucket instances in practise. If nodes are added to the linked list of buckets by using the ListBucket nodes themselves
	/// the count doesn't match, however doing so will also mess up the head/tail housekeeping references. So either use solely the buckets as a 
	/// linked list, or use this instance to manage the linked list of buckets</remarks>
	public class LinkedBucketList<T> : IEnumerable<T>
	{
		#region Class Member Declarations
		private ListBucket<T>	_head, _tail;
		private int _count;
		#endregion

		/// <summary>
		/// Appends the specified bucket to the list, after the last element as a new tail.
		/// </summary>
		/// <param name="toAppend">To append.</param>
		public void AppendTail(ListBucket<T> toAppend)
		{
			if(toAppend == null)
			{
				return;
			}

			if(_head == null)
			{
				_head = toAppend;
			}
			else
			{
				_tail.AppendAfter(toAppend);
			}
			_tail = toAppend;
			_count++;
		}


		/// <summary>
		/// Appends the specified contents in a new bucket after the last element in the list as a new tail.
		/// </summary>
		/// <param name="contentsToAppend">The contents to append.</param>
		/// <returns>the bucket appended</returns>
		public ListBucket<T> AppendTail(T contentsToAppend)
		{
			ListBucket<T> toAppend = new ListBucket<T>(contentsToAppend);
			AppendTail(toAppend);
			return toAppend;
		}


		/// <summary>
		/// Inserts the specified bucket as the new head in the list.
		/// </summary>
		/// <param name="toInsert">To insert.</param>
		public void InsertHead(ListBucket<T> toInsert)
		{
			if(toInsert == null)
			{
				return;
			}

			if(_tail == null)
			{
				_tail = toInsert;
			}
			else
			{
				_head.InsertBefore(toInsert);
			}
			_head = toInsert;
			_count++;
		}


		/// <summary>
		/// Inserts the specified contents in a new bucket as the new head in the list.
		/// </summary>
		/// <param name="contentsToInsert">The contents to insert.</param>
		/// <returns>the bucket inserted</returns>
		public ListBucket<T> InsertHead(T contentsToInsert)
		{
			ListBucket<T> toInsert = new ListBucket<T>(contentsToInsert);
			InsertHead(toInsert);
			return toInsert;
		}


		/// <summary>
		/// Removes all buckets after the node specified. It assumes the specified node is in the ListBucketList. If it's not, all nodes in this list will
		/// be removed and the list will be empty.
		/// </summary>
		/// <param name="newTail">The new tail. Has to be in the list.</param>
		public void RemoveAfter(ListBucket<T> newTail)
		{
			if(newTail == null)
			{
				return;
			}
			
			while(_tail != newTail)
			{
				Remove(_tail);
			}
		}


		/// <summary>
		/// Removes all buckets before the node specified. It assumes the specified node is in the ListBucketList. If it's not, all nodes in this list will
		/// be removed and the list will be empty.
		/// </summary>
		/// <param name="newHead">The new head. Has to be in the list.</param>
		public void RemoveBefore(ListBucket<T> newHead)
		{
			if(newHead == null)
			{
				return;
			}

			while(_head != newHead)
			{
				Remove(_head);
			}
		}


		/// <summary>
		/// Inserts the specified node toInsert after the node toInsertAfter.
		/// </summary>
		/// <param name="toInsert">To insert into the list.</param>
		/// <param name="toInsertAfter">the node to insert after. Has to be in the list</param>
		public void InsertAfter(ListBucket<T> toInsert, ListBucket<T> toInsertAfter)
		{
			if(toInsert == null)
			{
				return;
			}
			if(toInsertAfter == null)
			{
				if(_count > 0)
				{
					throw new ArgumentException("toInsertAfter can't be null when this LinkedBucketList has 1 or more elements");
				}
				InsertHead(toInsert);
			}
			else
			{
				toInsertAfter.AppendAfter(toInsert);
				_count++;
				if(toInsertAfter == _tail)
				{
					_tail = toInsert;
				}
			}
		}


		/// <summary>
		/// Inserts the specified node toInsert before the node toInsertBefore.
		/// </summary>
		/// <param name="toInsert">To insert into the list.</param>
		/// <param name="toInsertBefore">the node to insert before. Has to be in the list</param>
		public void InsertBefore(ListBucket<T> toInsert, ListBucket<T> toInsertBefore)
		{
			if(toInsert == null) 
			{
				return;
			}
			if(toInsertBefore == null)
			{
				if(_count > 0)
				{
					throw new ArgumentException("toInsertBefore can't be null when this LinkedBucketList has 1 or more elements");
				}
				InsertHead(toInsert);
			}
			else
			{
				toInsertBefore.InsertBefore(toInsert);
				_count++;
				if(toInsertBefore == _head)
				{
					_head = toInsert;
				}
			}
		}


		/// <summary>
		/// Removes the specified bucket from the list.
		/// </summary>
		/// <param name="toRemove">To remove.</param>
		/// <remarks>Routine assumes the passed in bucket is in the linked bucket list managed by this instance</remarks>
		/// <returns>true if remove took place, otherwise false</returns>
		public bool Remove(ListBucket<T> toRemove)
		{
			if(toRemove==null)
			{
				return false;
			}

			if(_head == toRemove)
			{
				_head = toRemove.Next;
			}
			if(_tail == toRemove)
			{
				_tail = toRemove.Previous;
			}

			toRemove.RemoveFromList();
			_count--;
			return true;
		}


		/// <summary>
		/// Removes the bucket with the specified contents from the list
		/// </summary>
		/// <param name="contentsToRemove">The contents to remove.</param>
		/// <returns>true if remove took place, otherwise false</returns>
		public bool Remove(T contentsToRemove)
		{
			return Remove(Find(contentsToRemove));
		}


		/// <summary>
		/// Removes the bucket with the specified contents from the list
		/// </summary>
		/// <param name="contentsToRemove">The contents to remove.</param>
		/// <param name="compareFunc">The compare func.</param>
		/// <returns>
		/// true if remove took place, otherwise false
		/// </returns>
		public bool Remove(T contentsToRemove, Func<T, T, bool> compareFunc)
		{
			return Remove(Find(contentsToRemove, (compareFunc)));
		}


		/// <summary>
		/// Concats the specified list after this list in an O(1) operation.
		/// </summary>
		/// <param name="toConcat">To concat.</param>
		/// <remarks>After the concat operation, the buckets in toConcat are referenced by this list. It's not recommended to keep on
		/// working with toConcat. Instead use this instance, as all data of toConcat is now part of this instance</remarks>
		public void Concat(LinkedBucketList<T> toConcat)
		{
			if(toConcat == null)
			{
				return;
			}
			if(_head == null)
			{
				_head = toConcat.Head;
				_tail = toConcat.Tail;
			}
			else
			{
				_tail.AppendAfter(toConcat.Head);
			}
			_count += toConcat.Count;
		}


		/// <summary>
		/// Finds the bucket with the specified contents. 
		/// </summary>
		/// <param name="contents">The contents.</param>
		/// <returns>the bucket with the contents or null if not found.</returns>
		/// <remarks>Uses a linear search</remarks>
		public ListBucket<T> Find(T contents)
		{
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			Func<T, T, bool> comparerFunc = (a, b) => comparer.Equals(a, b);
			return Find(contents, comparerFunc);
		}


		/// <summary>
		/// Finds the bucket with the specified contents using the comparer func specified.
		/// </summary>
		/// <param name="contents">The contents.</param>
		/// <param name="compareFunc">The compare func.</param>
		/// <returns>
		/// the bucket with the contents or null if not found.
		/// </returns>
		/// <remarks>Uses a linear search</remarks>
		public ListBucket<T> Find(T contents, Func<T, T, bool> compareFunc)
		{
			ListBucket<T> toReturn = null;
			ListBucket<T> currentBucket = _head;
			while(currentBucket!=null)
			{
				if(compareFunc(contents, currentBucket.Contents))
				{
					toReturn = currentBucket;
					break;
				}
				currentBucket = currentBucket.Next;
			}
			return toReturn;
		}


		/// <summary>
		/// Clears this instance. It doesn't reset the individual nodes, it just cuts off references to head and tail so the list contents goes
		/// out of scope.
		/// </summary>
		public void Clear()
		{
			if(_head != null)
			{
				// just cut off head and tail (it's a linked list, people, not a living being ;)). The rest will go out of scope. 
				_head.RemoveFromList();
				_head = null;
				_tail.RemoveFromList();
				_tail = null;
				_count = 0;
			}
		}


		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			ListBucket<T> current = _head;

			while(current != null)
			{
				yield return current.Contents;
				current = current.Next;
			}
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


		#region Class Property Declarations
		/// <summary>
		/// Gets the head of the list
		/// </summary>
		public ListBucket<T> Head
		{
			get { return _head; }
		}

		/// <summary>
		/// Gets the tail of the list.
		/// </summary>
		public ListBucket<T> Tail
		{
			get { return _tail; }
		}

		/// <summary>
		/// Gets the number of elements in this list. 
		/// </summary>
		/// <remarks>This is the number calculated from append/insert/remove actions.</remarks>
		public int Count
		{
			get { return _count; }
		}
		#endregion
	}
}
