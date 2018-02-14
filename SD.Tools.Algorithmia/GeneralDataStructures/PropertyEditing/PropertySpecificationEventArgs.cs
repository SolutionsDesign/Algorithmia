//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2018 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2018 Solutions Design. All rights reserved. (Algorithmia)
// Copyright (c) 2018 Tony Allowatt (property bag code)
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
//		- Tony Allowatt 
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
// The code in this file and related property bag files are based on the work of Tony Allowatt
// which can be found here: http://www.codeproject.com/KB/miscctrl/bending_property.aspx .
// I ([FB]) re-implemented/ported the code for .NET 3.5, though the credits go to Tony Allowatt for the 
// initial idea and ground work
//////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.GeneralDataStructures.PropertyEditing
{
	/// <summary>
	/// Class used in the PropertyBag's GetValue and SetValue events. 
	/// </summary>
	public class PropertySpecificationEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the PropertySpecEventArgs class.
		/// </summary>
		/// <param name="property">The PropertySpec that represents the property whose value is being requested or set.</param>
		/// <param name="val">The current value of the property.</param>
		public PropertySpecificationEventArgs(PropertySpecification property, object val)
		{
			ArgumentVerifier.CantBeNull(property, "property");
			this.Property = property;
			this.Value = val;
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the PropertySpec that represents the property whose value is being
		/// requested or set.
		/// </summary>
		public PropertySpecification Property { get; private set; }

		/// <summary>
		/// Gets or sets the current value of the property.
		/// </summary>
		public object Value { get; set; }
		#endregion
	}
}
