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
using System.ComponentModel;
using System.Collections.ObjectModel;

using SD.Tools.Algorithmia.Commands;
using SD.Tools.Algorithmia.UtilityClasses;
using System.Collections.Generic;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Generic list class which is command-aware: it performs its actions through commands, so all actions on this list are undoable. 
	/// </summary>
	/// <typeparam name="T">The type of the element inside the list.</typeparam>
	/// <remarks>This class implements IBindingList and not WPF's INotifyCollectionChanged, because the latter isn't recognized
	/// by Winforms controls, hence the IBindingList interface implementation. Due to the nature of IBindingList, an extra event has been added
	/// for retrieving a removed element by an observer</remarks>
	public class CommandifiedList<T> : Collection<T>, IBindingList
	{
		#region Class Member Declarations
		private string _listDescription;
		private Dictionary<ListCommandType, string> _cachedCommandDescriptions;
		#endregion

		#region Events
		/// <summary>
		/// Occurs when the list changes or an item in the list changes.
		/// </summary>
		public event ListChangedEventHandler ListChanged;
		/// <summary>
		/// Raised when an element was removed from this list. The element removed is contained in the event arguments. This event is necessary to 
		/// be able to retrieve a removed element after it was removed from the list by an observer, as ListChanged only contains indexes, and the
		/// index of a removed element isn't valid after it's been removed from the list. 
		/// </summary>
		public event EventHandler<CollectionElementRemovedEventArgs<T>> ElementRemoved;
		#endregion

		#region Enums
		/// <summary>
		/// Enum for finding back command descriptions. These descriptions are cached because they'd otherwise overflow the GC memory with string fragments
		/// </summary>
		private enum ListCommandType
		{
			ClearItems,
			InsertItem,
			RemoveItem,
			SetItem
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandifiedList&lt;T&gt;"/> class.
		/// </summary>
		public CommandifiedList()
			: base()
		{
			_listDescription = string.Format("CommandifiedList<{0}>", typeof(T).Name);
			BuildCachedCommandDescriptions();
		}


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0}. Count: {1}", _listDescription, this.Count);
		}


		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		protected override void ClearItems()
		{
			// create a command which stores the current state into a temp collection and then clears this collection.
			Command<Collection<T>> clearCmd = new Command<Collection<T>>(() => this.PerformClearItems(), () => this.GetCurrentState(), c=>this.SetCurrentState(c), 
													_cachedCommandDescriptions[ListCommandType.ClearItems]);
			CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(clearCmd);
		}


		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void InsertItem(int index, T item)
		{
			// create a command which simply inserts the item at the given index and as undo function removes the item at the index specified.
			Command<T> insertCmd = new Command<T>(() => this.PerformInsertItem(index, item), () => this.PerformRemoveItem(index), 
													_cachedCommandDescriptions[ListCommandType.InsertItem]);
			CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(insertCmd);
		}


		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void RemoveItem(int index)
		{
			// create a command which simply removes the item at the given index and as undo function it re-inserts the item at the index specified.
			// The command created passes the current item at the index specified, but it's not really used, as there's no state to set. The command however has to
			// keep track of the item removed, so the state inside it has to be of type T.
			Command<T> removeCmd = new Command<T>(() => this.PerformRemoveItem(index), ()=>this[index], i => this.PerformInsertItem(index, i),
												_cachedCommandDescriptions[ListCommandType.RemoveItem]);
			CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(removeCmd);
		}


		/// <summary>
		/// Replaces the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to replace.</param>
		/// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void SetItem(int index, T item)
		{
			// Create a command which stores the current item at index and sets item as the new item at index. 
			Command<T> setCmd = new Command<T>(() => this.PerformSetItem(index, item), () => this[index], i => this.PerformSetItem(index, i),
												_cachedCommandDescriptions[ListCommandType.SetItem]);
			CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(setCmd);
		}


		/// <summary>
		/// Called right before the clear action starts. Use this method to do event handler housekeeping on elements in this list. 
		/// </summary>
		protected virtual void OnClearing()
		{
			// nop
		}


		/// <summary>
		/// Called right before the item passed in is about to be removed from this list. Use this method to do event handler housekeeping on elements in this list. 
		/// </summary>
		/// <param name="item">The item which is about to be removed.</param>
		protected virtual void OnRemovingItem(T item)
		{ 
			// nop
		}


		/// <summary>
		/// Called right before the item passed in is about to be added to this list. Use this method to do event handler housekeeping on elements in this list. 
		/// </summary>
		/// <param name="item">The item which is about to be added.</param>
		protected virtual void OnAddingItem(T item)
		{ 
			// nop 
		}
		

		/// <summary>
		/// Notifies a list change to observers with the parameters passed in.
		/// </summary>
		/// <param name="changeType">Type of the change.</param>
		/// <param name="index">The index of the item involved (can be -1 or 0 or higher).</param>
		protected void NotifyChange(ListChangedType changeType, int index)
		{
			if(ListChanged != null)
			{
				ListChanged(this, new ListChangedEventArgs(changeType, index));
			}
		}


		/// <summary>
		/// Builds the cached command descriptions.
		/// </summary>
		private void BuildCachedCommandDescriptions()
		{
			_cachedCommandDescriptions = new Dictionary<ListCommandType, string>();
			_cachedCommandDescriptions.Add(ListCommandType.ClearItems, string.Format("Clear the {0} instance", _listDescription));
			_cachedCommandDescriptions.Add(ListCommandType.InsertItem, string.Format("Insert a new item in the {0} instance", _listDescription));
			_cachedCommandDescriptions.Add(ListCommandType.RemoveItem, string.Format("Remove an item from the {0} instance", _listDescription));
			_cachedCommandDescriptions.Add(ListCommandType.SetItem, string.Format("Set a new item at a given index in the {0} instance", _listDescription));
		}


		/// <summary>
		/// Gets the current state of this collection, which is simply a copy of the items into another collection.
		/// </summary>
		/// <returns>a collection with all items in this collection in the same order. </returns>
		private Collection<T> GetCurrentState()
		{
			// can't use the wrapping CTor as that would reflect the changes we're going to make to this collection.
			Collection<T> toReturn = new Collection<T>();
			foreach(T t in this)
			{
				toReturn.Add(t);
			}
			return toReturn;
		}


		/// <summary>
		/// Sets the state of this collection to the passed in state. This means: removing all elements and then setting it back to the state passed in.
		/// </summary>
		/// <param name="state">The state.</param>
		private void SetCurrentState(Collection<T> state)
		{
			ArgumentVerifier.CantBeNull(state, "state");

			base.ClearItems();

			// from back to front, insert the items, in the same order.
			for(int i = state.Count-1; i>=0; i--)
			{
				T itemToAdd = state[i];
				OnAddingItem(itemToAdd);
				base.InsertItem(0, itemToAdd);
			}
			NotifyChange(ListChangedType.Reset, -1);
		}
		

		/// <summary>
		/// Verifies the index.
		/// </summary>
		/// <param name="index">The index.</param>
		private void VerifyIndex(int index)
		{
			if((index < 0) || (index >= this.Count))
			{
				throw new ArgumentOutOfRangeException("index", string.Format("index is out of range: the value '{0}' isn't in the range of valid values.", index));
			}
		}


		#region Methods which perform the actual actions
		/// <summary>
		/// Performs the ClearItems call.
		/// </summary>
		private void PerformClearItems()
		{
			OnClearing();
			base.ClearItems();
			NotifyChange(ListChangedType.Reset, -1);
		}


		/// <summary>
		/// Performs the InsertItem call
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The item.</param>
		private void PerformInsertItem(int index, T item)
		{
			OnAddingItem(item);
			base.InsertItem(index, item);
			NotifyChange(ListChangedType.ItemAdded, index);
		}


		/// <summary>
		/// Performs the RemoveItem call
		/// </summary>
		/// <param name="index">The index.</param>
		private void PerformRemoveItem(int index)
		{
			VerifyIndex(index);
			T itemToRemove = this[index];
			OnRemovingItem(itemToRemove);
			base.RemoveItem(index);
			this.ElementRemoved.RaiseEvent(this, new CollectionElementRemovedEventArgs<T>(itemToRemove));
			NotifyChange(ListChangedType.ItemDeleted, index);
		}

		/// <summary>
		/// Performs the SetItem call
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The item.</param>
		private void PerformSetItem(int index, T item)
		{
			VerifyIndex(index);
			OnRemovingItem(this[index]);
			OnAddingItem(item);
			base.SetItem(index, item);
			NotifyChange(ListChangedType.ItemChanged, index);
		}
		#endregion


		#region IBindingList Members
		/// <summary>
		/// Adds the <see cref="T:System.ComponentModel.PropertyDescriptor"/> to the indexes used for searching.
		/// </summary>
		/// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to add to the indexes used for searching.</param>
		public virtual void AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException("AddIndex isn't supported.");
		}


		/// <summary>
		/// Adds a new item to the list.
		/// </summary>
		/// <returns>The item added to the list.</returns>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.AllowNew"/> is false. </exception>
		public virtual object AddNew()
		{
			throw new NotSupportedException("AddNew isn't supported");
		}


		/// <summary>
		/// Gets whether you can update items in the list.
		/// </summary>
		/// <value></value>
		/// <returns>true if you can update the items in the list; otherwise, false.</returns>
		public virtual bool AllowEdit
		{
			get { return true; }
		}

		/// <summary>
		/// Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
		/// </summary>
		/// <value></value>
		/// <returns>true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>; otherwise, false.</returns>
		public virtual bool AllowNew
		{
			get { return false; }
		}

		/// <summary>
		/// Gets whether you can remove items from the list, using <see cref="M:System.Collections.IList.Remove(System.Object)"/> or <see cref="M:System.Collections.IList.RemoveAt(System.Int32)"/>.
		/// </summary>
		/// <value></value>
		/// <returns>true if you can remove items from the list; otherwise, false.</returns>
		public virtual bool AllowRemove
		{
			get { return true; }
		}


		/// <summary>
		/// Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor"/> and a <see cref="T:System.ComponentModel.ListSortDirection"/>.
		/// </summary>
		/// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to sort by.</param>
		/// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
		public virtual void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException("Sorting through ApplySort isn't supported. Use the Sort extension method instead.");
		}


		/// <summary>
		/// Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
		/// </summary>
		/// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to search on.</param>
		/// <param name="key">The value of the <paramref name="property"/> parameter to search for.</param>
		/// <returns>
		/// The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSearching"/> is false. </exception>
		public virtual int Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException("Find isn't supported");
		}


		/// <summary>
		/// Gets whether the items in the list are sorted.
		/// </summary>
		/// <value></value>
		/// <returns>true if <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort"/> has not been called; otherwise, false.</returns>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
		public virtual bool IsSorted
		{
			get { return false; }
		}


		/// <summary>
		/// Removes the <see cref="T:System.ComponentModel.PropertyDescriptor"/> from the indexes used for searching.
		/// </summary>
		/// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to remove from the indexes used for searching.</param>
		public void RemoveIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException("RemoveIndex isn't supported");
		}


		/// <summary>
		/// Removes any sort applied using <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
		public virtual void RemoveSort()
		{
			throw new NotSupportedException("RemoveSort isn't supported");
		}


		/// <summary>
		/// Gets the direction of the sort.
		/// </summary>
		/// <value></value>
		/// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</returns>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
		public virtual ListSortDirection SortDirection
		{
			get { return ListSortDirection.Ascending;}
		}


		/// <summary>
		/// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
		/// </summary>
		/// <value></value>
		/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.</returns>
		/// <exception cref="T:System.NotSupportedException">
		/// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
		public virtual PropertyDescriptor SortProperty
		{
			get { return null; }
		}


		/// <summary>
		/// Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or an item in the list changes.
		/// </summary>
		/// <value></value>
		/// <returns>true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or when an item changes; otherwise, false.</returns>
		public virtual bool SupportsChangeNotification
		{
			get { return true; }
		}


		/// <summary>
		/// Gets whether the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method.
		/// </summary>
		/// <value></value>
		/// <returns>true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method; otherwise, false.</returns>
		public virtual bool SupportsSearching
		{
			get { return false; }
		}


		/// <summary>
		/// Gets whether the list supports sorting.
		/// </summary>
		/// <value></value>
		/// <returns>true if the list supports sorting; otherwise, false.</returns>
		public virtual bool SupportsSorting
		{
			get { return false; }
		}
		#endregion
	}
}
