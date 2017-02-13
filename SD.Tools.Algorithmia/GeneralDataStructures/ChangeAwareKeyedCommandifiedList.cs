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
using SD.Tools.Algorithmia.GeneralInterfaces;
using SD.Tools.Algorithmia.GeneralDataStructures.EventArguments;
using SD.Tools.BCLExtensions.SystemRelated;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Class which extends KeyedCommandifiedList so that it picks up detailed changes in elements in this list and propagates them to subscribers in a single event.
	/// Subscribers therefore don't have to subscribe to all detailed change events of all the elements in the list. 
	/// </summary>
	/// <remarks>This class can be a synchronized collection by passing true for isSynchronized in the constructor. To synchronize access to the contents of this class, 
	/// lock on the SyncRoot object. This class uses the same lock for its internal elements as the base class as these elements are related to the elements of the base class</remarks>
	public class ChangeAwareKeyedCommandifiedList<T, TKeyValue, TChangeType> : KeyedCommandifiedList<T, TKeyValue>
		where T : class, IDetailedNotifyElementChanged<TChangeType, T>
	{
		#region Events
		/// <summary>
		/// Raised when an element in this list raised its DetailedElementChanged event. The event args contain detailed information about what was changed. 
		/// </summary>
		public event EventHandler<ElementInListChangedEventArgs<TChangeType, T>> DetailedElementInListChanged;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="ChangeAwareKeyedCommandifiedList&lt;T, TKeyValue, TChangeType&gt;"/> class.
		/// </summary>
		/// <param name="keyValueProducerFunc">The key value producer func.</param>
		/// <param name="keyPropertyName">Name of the key property which is used to track changes in individual elements.</param>
		public ChangeAwareKeyedCommandifiedList(Func<T, TKeyValue> keyValueProducerFunc, string keyPropertyName)
			: this(keyValueProducerFunc, keyPropertyName, isSynchronized:false)
		{ }


		/// <summary>
		/// Initializes a new instance of the <see cref="ChangeAwareKeyedCommandifiedList&lt;T, TKeyValue, TChangeType&gt;" /> class.
		/// </summary>
		/// <param name="keyValueProducerFunc">The key value producer func.</param>
		/// <param name="keyPropertyName">Name of the key property which is used to track changes in individual elements.</param>
		/// <param name="isSynchronized">if set to <c>true</c> this list is a synchronized collection, using a lock on SyncRoot to synchronize activity in multithreading
		/// scenarios</param>
		public ChangeAwareKeyedCommandifiedList(Func<T, TKeyValue> keyValueProducerFunc, string keyPropertyName, bool isSynchronized)
			: base(keyValueProducerFunc, keyPropertyName, isSynchronized)
		{
		}

		/// <summary>
		/// Called right before the item passed in is about to be added to this list. Use this method to do event handler housekeeping on elements in this list.
		/// </summary>
		/// <param name="item">The item which is about to be added.</param>
		protected override void OnAddingItem(T item)
		{
			base.OnAddingItem(item);
			item.DetailedElementChanged += item_DetailedElementChanged;
		}


		/// <summary>
		/// Called right after the item passed in has been removed from this list.
		/// </summary>
		/// <param name="item">The item.</param>
		protected override void OnRemovingItemComplete(T item)
		{
			base.OnRemovingItemComplete(item);
			item.DetailedElementChanged -= item_DetailedElementChanged;
		}


		/// <summary>
		/// Handles the DetailedElementChanged event of the item control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void item_DetailedElementChanged(object sender, ElementChangedEventArgs<TChangeType, T> e)
		{
			this.DetailedElementInListChanged.RaiseEvent(this, new ElementInListChangedEventArgs<TChangeType, T>(e.TypeOfChange, e.InvolvedElement));
		}
	}
}
