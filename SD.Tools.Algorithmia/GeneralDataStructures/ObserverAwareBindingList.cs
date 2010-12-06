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
using System.ComponentModel;
using SD.Tools.Algorithmia.GeneralInterfaces;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Special version of the bindinglist, where the generic elements have to implement IEventBasedObserver to get their events managed.
	/// </summary>
	/// <typeparam name="T">Type of the element contained in this special binding list.</typeparam>
	public class ObserverAwareBindingList<T> : BindingList<T>
		where T : class, IEventBasedObserver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObserverAwareBindingList&lt;T&gt;"/> class.
		/// </summary>
		public ObserverAwareBindingList() : base()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ObserverAwareBindingList&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="elements">The elements.</param>
		public ObserverAwareBindingList(IList<T> elements) : base(elements)
		{
		}


		/// <summary>
		/// Adds the range.
		/// </summary>
		/// <param name="rangeToAdd">The range to add.</param>
		public void AddRange(IEnumerable<T> rangeToAdd)
		{
			foreach(T item in rangeToAdd)
			{
				this.Add(item);
			}
		}


		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param name="index">The zero-based index of the item to replace.</param>
		/// <param name="item">The new value for the item at the specified index. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than zero.
		/// -or-
		/// <paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.
		/// </exception>
		protected override void SetItem(int index, T item)
		{
			T existingItem = this[index];
			if(existingItem!=null)
			{
				existingItem.UnbindEvents();
				UnbindFromNotifyElementRemoved(existingItem);
			}
			if(item!=null)
			{
				item.BindEvents();
				BindToNotifyElementRemoved(item);
			}
			base.SetItem(index, item);
		}


		/// <summary>
		/// Inserts the specified item in the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index where the item is to be inserted.</param>
		/// <param name="item">The item to insert in the list.</param>
		protected override void InsertItem(int index, T item)
		{
			item.BindEvents();
			BindToNotifyElementRemoved(item);
			base.InsertItem(index, item);
		}


		/// <summary>
		/// Removes all elements from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach(T item in this)
			{
				if(item!=null)
				{
					item.UnbindEvents();
					UnbindFromNotifyElementRemoved(item);
				}
			}
			base.ClearItems();
		}


		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// You are removing a newly added item and <see cref="P:System.ComponentModel.IBindingList.AllowRemove"/> is set to false.
		/// </exception>
		protected override void RemoveItem(int index)
		{
			T item = this[index];
			if(item!=null)
			{
				item.UnbindEvents();
				UnbindFromNotifyElementRemoved(item);
			}
			base.RemoveItem(index);
		}

		
		/// <summary>
		/// Unbinds the eventhandler from item.ElementRemoved if item implements INotifyElementRemoved
		/// </summary>
		/// <param name="item">The item.</param>
		private void UnbindFromNotifyElementRemoved(T item)
		{
			INotifyAsRemoved itemAsINotifyElementRemoved = item as INotifyAsRemoved;
			if(itemAsINotifyElementRemoved != null)
			{
				itemAsINotifyElementRemoved.HasBeenRemoved -= new EventHandler(element_ElementRemoved);
			}
		}


		/// <summary>
		/// Binds the eventhandler to item.ElementRemoved if item implements INotifyElementRemoved
		/// </summary>
		/// <param name="item">The item.</param>
		private void BindToNotifyElementRemoved(T item)
		{
			INotifyAsRemoved itemAsINotifyElementRemoved = item as INotifyAsRemoved;
			if(itemAsINotifyElementRemoved != null)
			{
				itemAsINotifyElementRemoved.HasBeenRemoved += new EventHandler(element_ElementRemoved);
			}
		}


		/// <summary>
		/// Handles the ElementRemoved event of the element control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void element_ElementRemoved(object sender, EventArgs e)
		{
			T senderToRemove = sender as T;
			if(senderToRemove!=null)
			{
				this.Remove(senderToRemove);
			}
		}
	}
}
