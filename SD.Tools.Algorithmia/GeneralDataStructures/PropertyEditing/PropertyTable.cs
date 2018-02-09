#if !NETSTANDARD2_0 // Idiotic requirement, but otherwise dumbass msbuild still compiles this file for netstandard, even though we have excluded it in the csproj
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
using System.Collections.Generic;
using System.Linq;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.GeneralDataStructures.PropertyEditing
{
	/// <summary>
	/// An extension of PropertyBag that manages a table of property values, in addition to firing events when property values are requested or set.
	/// </summary>
	public class PropertyTable : PropertyBag
	{
		#region Class Member Declarations
		private readonly Dictionary<string, object> _propertyValues;
		#endregion

		/// <summary>
		/// Initializes a new instance of the PropertyTable class.
		/// </summary>
		public PropertyTable()
		{
			_propertyValues = new Dictionary<string, object>();
		}

		/// <summary>
		/// This member overrides PropertyBag.OnGetValue.
		/// </summary>
		protected internal override void OnGetValue(PropertySpecificationEventArgs e)
		{
			e.Value = this[e.Property.Name];
			base.OnGetValue(e);
		}

		/// <summary>
		/// This member overrides PropertyBag.OnSetValue.
		/// </summary>
		protected internal override void OnSetValue(PropertySpecificationEventArgs e)
		{
			this[e.Property.Name] = e.Value;
			base.OnSetValue(e);
		}

		#region Class Property Declarations
		/// <summary>
		/// Gets or sets the value of the property with the specified name.
		/// <p>In C#, this property is the indexer of the PropertyTable class.</p>
		/// </summary>
		public object this[string key]
		{
			get { return _propertyValues.GetValue(key); }
			set { _propertyValues[key] = value; }
		}
		#endregion
	}
}
#endif