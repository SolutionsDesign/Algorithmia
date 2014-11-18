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

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Simple class which can be used as a bucket in a linked list. The .NET LinkedList class has a downside that you can't concatenate two
	/// linked lists in O(1) time: a LinkedListNode is part of a LinkedList object and to connect two lists, one has to traverse one of them in full.
	/// </summary>
	/// <typeparam name="T">Type of the element contained in this bucket</typeparam>
	public class ListBucket<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ListBucket&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="contents">The contents.</param>
		public ListBucket(T contents)
		{
			this.Contents = contents;
		}


		/// <summary>
		/// Appends the passed in bucket after this bucket in the list.
		/// </summary>
		/// <param name="toAppend">To append.</param>
		public void AppendAfter(ListBucket<T> toAppend)
		{
			if(toAppend == null)
			{
				return;
			}
			toAppend.Next = this.Next;
			toAppend.Previous = this;
			this.Next = toAppend;
			if(toAppend.Next != null)
			{
				toAppend.Next.Previous = toAppend;
			}
		}


		/// <summary>
		/// Appends the passed in contents in a new bucket after this bucket in the list.
		/// </summary>
		/// <param name="toAppendContents">The contents of the bucket to append.</param>
		public void AppendAfter(T toAppendContents)
		{
			AppendAfter(new ListBucket<T>(toAppendContents));
		}


		/// <summary>
		/// Inserts the passed in bucket before this bucket in the list
		/// </summary>
		/// <param name="toInsert">To insert.</param>
		public void InsertBefore(ListBucket<T> toInsert)
		{
			if(toInsert == null)
			{
				return;
			}

			toInsert.Previous = this.Previous;
			toInsert.Next = this;
			this.Previous = toInsert;
			if(toInsert.Previous != null)
			{
				toInsert.Previous.Next = toInsert;
			}
		}


		/// <summary>
		/// Inserts the passed in contents in a new bucket before this bucket in the list
		/// </summary>
		/// <param name="toInsertContents">The contents of the new bucket to insert.</param>
		public void InsertBefore(T toInsertContents)
		{
			InsertBefore(new ListBucket<T>(toInsertContents));
		}


		/// <summary>
		/// Removes this bucket from the list, connecting both ends together.
		/// </summary>
		public void RemoveFromList()
		{
			if(this.Previous != null)
			{
				this.Previous.Next = this.Next;
			}
			if(this.Next != null)
			{
				this.Next.Previous = this.Previous;
			}

			this.Previous = null;
			this.Next = null;
		}


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			string toReturn = (object)this.Contents == null ? "<null>" : this.Contents.ToString();
			return "Contents: " + toReturn;
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets or sets the contents of this bucket
		/// </summary>
		public T Contents { get; set; }

		/// <summary>
		/// Gets or sets the next bucket in the list
		/// </summary>
		public ListBucket<T> Next { get; set; }

		/// <summary>
		/// Gets or sets the previous bucket in the list.
		/// </summary>
		public ListBucket<T> Previous { get; set; }
		#endregion
	}
}
