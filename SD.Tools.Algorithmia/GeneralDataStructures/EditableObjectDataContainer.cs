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
using System.ComponentModel;
using System.Collections;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Class which is used to implement IEditableObject on objects. It tracks state and performs actions. 
	/// </summary>
	/// <remarks>To use this container, implement IEditableObject on a class and simply call the BeginEdit/EndEdit and CancelEdit of an instance of
	/// this class in the BeginEdit/EndEdit/CancelEdit methods of the IEditableObject implementation. Set DatabindingContainer of the instance
	/// of this class to the container collection in the container collection's IBindingList.AddNew() implementation.</remarks>
	public class EditableObjectDataContainer : IEditableObject
	{
		#region Class Member Declarations
		private bool _editCycleInProgress, _pendingCancelEdit;
		private readonly object _editedObject;
		private IList _databindingContainer;		// for IEditableObject: AddNew() sets this property, CancelEdit has to remove this field from the container.
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="EditableObjectDataContainer"/> class.
		/// </summary>
		/// <param name="editedObject">The edited object for which this container implements IEditableObject.</param>
		public EditableObjectDataContainer(object editedObject)
		{
			ArgumentVerifier.CantBeNull(editedObject, "editedObject");
			_editedObject = editedObject;
		}


		/// <summary>
		/// Begins an edit on an object.
		/// </summary>
		public void BeginEdit()
		{
			_editCycleInProgress = true;
		}


		/// <summary>
		/// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> call.
		/// </summary>
		public void CancelEdit()
		{
			if(_editCycleInProgress)
			{
				if(_databindingContainer != null)
				{
					if(!_pendingCancelEdit)
					{
						try
						{
							_pendingCancelEdit = true;
							_databindingContainer.Remove(_editedObject);
							_databindingContainer = null;
						}
						finally
						{
							_pendingCancelEdit = false;
						}
					}
				}
				_pendingCancelEdit = false;
			}
		}


		/// <summary>
		/// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> or <see cref="M:System.ComponentModel.IBindingList.AddNew"/> call into the underlying object.
		/// </summary>
		public void EndEdit()
		{
			if(_editCycleInProgress)
			{
				_editCycleInProgress = false;
				_databindingContainer = null;
				_pendingCancelEdit = false;
			}
		}


		#region Class Property Declarations
		/// <summary>
		/// Sets the databinding container. Set this property in IBindingList.AddNew().
		/// </summary>
		public IList DatabindingContainer
		{
			set { _databindingContainer = value; }
		}
		#endregion
	}
}
