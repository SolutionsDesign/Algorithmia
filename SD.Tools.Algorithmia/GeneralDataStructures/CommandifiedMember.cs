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
using SD.Tools.Algorithmia.Commands;
using SD.Tools.BCLExtensions.SystemRelated;
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.Algorithmia.GeneralDataStructures.EventArguments;
using SD.Tools.Algorithmia.GeneralInterfaces;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Class which represents a member variable which is commandified, so setting the value occurs through commands.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <typeparam name="TChangeType">The type of the change type enum.</typeparam>
	/// <remarks>Resets error messages on successful set of a value, or if the field is set to the same value.
	/// Binds to the HasBeenChanged event of TValue, if it implements INotifyAsChanged, and raises ValueChanged if HasBeenChanged is raised on an element.
	/// Binds to the HasBeenRemoved event of TValue, if it implements INotifyAsRemoved, and raises ValueElementRemoved if HasBeenRemoved is raised
	/// on an element.</remarks>
	public class CommandifiedMember<TValue, TChangeType>
	{
		#region Class Member Declarations
		private TValue _memberValue;
		private readonly TChangeType _changeTypeValueToUse;
		private readonly string _memberName;
		private readonly ErrorContainer _loggedErrors;
		private bool _elementChangedBound, _elementRemovedBound;
		private MemberValueElementChangedHandler _sharedValueChangedHandler;
		private MemberValueElementRemovedHandler _sharedValueRemovedHandler;
		#endregion

		#region Events
		/// <summary>
		/// Event which is raised explicitly to signal subscribers that the error has been reset. This event is only raised if there's a necessity for it:
		/// If there's an error set and this object was set to the same value, resetting the error, subscribers don't know the error has been reset as the
		/// member value itself won't be changed. This event will notify them that the error has been reset and they should raise a Propertychanged event to
		/// notify UI elements that they can clear ErrorInfo providers. 
		/// </summary>
		public event EventHandler ErrorReset;
		/// <summary>
		/// Event which is raised when MemberValue is set to a different value. If MemberValue's value is a mutable object and implements INotifyElementChanged, the 
		/// event is also raised when some values inside the value object change. In that case, the original value and the new value in the event args
		/// are the same. 
		/// </summary>
		public event EventHandler<MemberChangedEventArgs<TChangeType, TValue>> ValueChanged;
		/// <summary>
		/// Event which is raised when the value implements INotifyElementRemoved and it got removed from its container. This event
		/// is used in that situation to notify observers that the value of this CommandifiedMember is no longer usable. 
		/// </summary>
		public event EventHandler ValueElementRemoved;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="CommandifiedMember&lt;TValue, TChangeType&gt;"/> class.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="changeTypeValueToUse">The change type value to use.</param>
		public CommandifiedMember(string memberName, TChangeType changeTypeValueToUse)
			: this(memberName, changeTypeValueToUse, null, default(TValue))
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CommandifiedMember&lt;TValue, TChangeType&gt;"/> class.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="changeTypeValueToUse">The change type value to use.</param>
		/// <param name="loggedErrors">container for error messages. Can be null.</param>
		public CommandifiedMember(string memberName, TChangeType changeTypeValueToUse, ErrorContainer loggedErrors) 
			: this(memberName, changeTypeValueToUse, loggedErrors, default(TValue))
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CommandifiedMember&lt;TValue, TChangeType&gt;"/> class.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="changeTypeValueToUse">The change type value to use.</param>
		/// <param name="initialValue">The initial value.</param>
		public CommandifiedMember(string memberName, TChangeType changeTypeValueToUse, TValue initialValue)
			: this(memberName, changeTypeValueToUse, null, initialValue)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CommandifiedMember&lt;TValue, TChangeType&gt;"/> class.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="changeTypeValueToUse">The change type value to use.</param>
		/// <param name="loggedErrors">The logged errors container.</param>
		/// <param name="initialValue">The initial value.</param>
		public CommandifiedMember(string memberName, TChangeType changeTypeValueToUse, ErrorContainer loggedErrors, TValue initialValue)
		{
			if(string.IsNullOrEmpty(memberName))
			{
				throw new ArgumentException("memberName has to have a value");
			}
			_loggedErrors = loggedErrors;
			_changeTypeValueToUse = changeTypeValueToUse;
			_memberName = memberName;
			_memberValue = initialValue;
			this.ThrowExceptionOnValidationError=true;
			BindToElementChanged();
			BindToElementRemoved();
		}


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return ((object)_memberValue) == null ? base.ToString() : this.MemberValue.ToString();
		}


		/// <summary>
		/// Unbinds from the ElementChanged event
		/// </summary>
		public void UnbindFromElementChanged()
		{
			if(_elementChangedBound && _memberValue is INotifyAsChanged changeAwareValue)
			{
				changeAwareValue.HasBeenChanged -= this.SharedValueChangedHandler;
				_elementChangedBound = false;
			}
		}


		/// <summary>
		/// Unbinds from the ElementRemoved event
		/// </summary>
		public void UnbindFromElementRemoved()
		{
			if(_elementRemovedBound && _memberValue is INotifyAsRemoved removeAwareValue)
			{
				removeAwareValue.HasBeenRemoved -= this.SharedValueRemovedHandler;
				_elementRemovedBound = false;
			}
		}


		/// <summary>
		/// Binds to INotifyElementChanged.ElementChanged on the value set in memberValue, if supported.
		/// </summary>
		public void BindToElementChanged()
		{
			if(!_elementChangedBound && _memberValue is INotifyAsChanged changeAwareValue)
			{
				changeAwareValue.HasBeenChanged += this.SharedValueChangedHandler;
				_elementChangedBound = true;
			}
		}


		/// <summary>
		/// Binds to the ElementRemoved event
		/// </summary>
		public void BindToElementRemoved()
		{
			if(!_elementRemovedBound && _memberValue is INotifyAsRemoved removeAwareValue)
			{
				removeAwareValue.HasBeenRemoved += this.SharedValueRemovedHandler;
				_elementRemovedBound = true;
			}
		}


		/// <summary>
		/// Sets the initial value of this commandified member. It does this by bypassing the actual member set function.
		/// Use this method to avoid events being raised while setting the value and also when you want to avoid using commands to set the value, as the
		/// memberValue is set without using commands. No validation occurs as well, so only use this if you want to set the initial value after the ctor 
		/// has already been called. 
		/// </summary>
		/// <param name="value">The value.</param>
		public void SetInitialValue(TValue value)
		{
			_memberValue = value;
		}


		/// <summary>
		/// Called when the member is set to the same value it already has
		/// </summary>
		protected virtual void OnMemberSetToSameValue()
		{
			this.ErrorReset.RaiseEvent(this);
		}


		/// <summary>
		/// Validates the member value.
		/// </summary>
		/// <param name="valueToValidate">The value to validate.</param>
		/// <param name="checkForDuplicates">if set to true, it will check for duplicate names</param>
		/// <param name="throwOnError">if set to true, it will throw a ValidationException if validation failed</param>
		/// <param name="correctValue">The correct value.</param>
		/// <returns>
		/// true if the member is valid, false otherwise
		/// </returns>
		protected virtual bool ValidateMemberValue(TValue valueToValidate, bool checkForDuplicates, bool throwOnError, out TValue correctValue)
		{
			correctValue = valueToValidate;
			return true;
		}


		/// <summary>
		/// Called before the membervalue is about to be set to the value specified. 
		/// </summary>
		/// <param name="value">The value.</param>
		protected virtual void OnBeforeMemberValueSet(TValue value)
		{
			UnbindFromElementChanged();
			UnbindFromElementRemoved();
		}


		/// <summary>
		/// Called after the membervalue has been set to a new value.
		/// </summary>
		protected virtual void OnAfterMemberValueSet()
		{
			if(_loggedErrors != null)
			{
				_loggedErrors.SetPropertyError(_memberName, string.Empty);
			}
			BindToElementChanged();
			BindToElementRemoved();
		}


		/// <summary>
		/// Resets the error for member.
		/// </summary>
		protected void ResetErrorForMember()
		{
			if(_loggedErrors != null)
			{
				_loggedErrors.SetPropertyError(_memberName, string.Empty);
			}
		}


		/// <summary>
		/// Sets the member value.
		/// </summary>
		/// <param name="value">The value.</param>
		private void SetMemberValue(TValue value)
		{
			OnBeforeMemberValueSet(value);
			if(!GeneralUtils.ValuesAreEqual(_memberValue, value))
			{
				TValue originalValue = _memberValue;
				_memberValue = value;
				OnAfterMemberValueSet();
				this.ValueChanged.RaiseEvent(this, new MemberChangedEventArgs<TChangeType, TValue>(_changeTypeValueToUse, originalValue, _memberValue));
			}
		}


		/// <summary>
		/// Handles the ElementChanged event of the _memberValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _memberValue_ElementChanged(object sender, EventArgs e)
		{
			this.ValueChanged.RaiseEvent(this, new MemberChangedEventArgs<TChangeType, TValue>(_changeTypeValueToUse, _memberValue, _memberValue));
		}


		/// <summary>
		/// Handles the ElementRemoved event of the _memberValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _memberValue_ElementRemoved(object sender, EventArgs e)
		{
			this.ValueElementRemoved.RaiseEvent(this);
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the shared valuechanged handler, and creates one if it's not already created.
		/// </summary>
		private MemberValueElementChangedHandler SharedValueChangedHandler
		{
			get { return _sharedValueChangedHandler ?? (_sharedValueChangedHandler = _memberValue_ElementChanged); }
		}

		/// <summary>
		/// Gets the shared valueremoved handler, and creates one if it's not already created.
		/// </summary>
		private MemberValueElementRemovedHandler SharedValueRemovedHandler
		{
			get { return _sharedValueRemovedHandler ?? (_sharedValueRemovedHandler = _memberValue_ElementRemoved); }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether an exception should be thrown if validation fails (true, default) or not (false)
		/// </summary>
		public bool ThrowExceptionOnValidationError { get; set; }

		/// <summary>
		/// Gets the name of the member.
		/// </summary>
		protected string MemberName
		{
			get { return _memberName; }
		}


		/// <summary>
		/// Gets the logged errors.
		/// </summary>
		protected ErrorContainer LoggedErrors
		{
			get { return _loggedErrors; }
		}


		/// <summary>
		/// Gets or sets the member value.
		/// </summary>
		public TValue MemberValue
		{
			get { return _memberValue; }
			set
			{
				TValue correctValue;
				if(ValidateMemberValue(value, true, this.ThrowExceptionOnValidationError, out correctValue))
				{
					ResetErrorForMember();
					if(GeneralUtils.ValuesAreEqual(_memberValue, correctValue))
					{
						OnMemberSetToSameValue();
					}
					else
					{
						if(CommandQueueManagerSingleton.GetInstance().IsInNonUndoablePeriod)
						{
							// skip ceremony as commands aren't undoable anyway.
							SetMemberValue(correctValue);
						}
						else
						{
							CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TValue>(() => this.SetMemberValue(correctValue),
																											    () => _memberValue,
																											    v => this.SetMemberValue(v),
																											    _memberName));
						}
					}
				}
			}
		}
		#endregion
	}
}
