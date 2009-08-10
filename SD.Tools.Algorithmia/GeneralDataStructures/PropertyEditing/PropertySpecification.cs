//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2009 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2009 Solutions Design. All rights reserved. (Algorithmia)
// Copyright (c) 2009 Tony Allowatt (property bag code)
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

namespace SD.Tools.Algorithmia.GeneralDataStructures.PropertyEditing
{
	/// <summary>
	/// Class which is used to specify a single property definition.
	/// </summary>
	public class PropertySpecification
	{
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		public PropertySpecification(string name, string type)
			: this(name, type, null, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		public PropertySpecification(string name, Type type)
			: this(name, type.AssemblyQualifiedName, null, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		public PropertySpecification(string name, string type, string category)
			: this(name, type, category, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		public PropertySpecification(string name, Type type, string category)
			: this(name, type.AssemblyQualifiedName, category, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		public PropertySpecification(string name, string type, string category, string description)
			: this(name, type, category, description, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		public PropertySpecification(string name, Type type, string category, string description)
			: this(name, type.AssemblyQualifiedName, category, description, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		public PropertySpecification(string name, Type type, string category, string description, object defaultValue) 
			:this(name, type.AssemblyQualifiedName, category, description, defaultValue)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The fully qualified name of the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The fully qualified name of the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, string type, string category, string description, object defaultValue, string editor, string typeConverter) 
				:this(name, type, category, description, defaultValue)
		{
			this.EditorTypeName = editor;
			this.ConverterTypeName = typeConverter;
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The fully qualified name of the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The fully qualified name of the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, Type type, string category, string description, object defaultValue, string editor, string typeConverter)
			: this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor, typeConverter)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The Type that represents the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The fully qualified name of the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, string type, string category, string description, object defaultValue, Type editor, string typeConverter)
			: this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName, typeConverter)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The Type that represents the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The fully qualified name of the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, Type type, string category, string description, object defaultValue,
			Type editor, string typeConverter)
			: this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor.AssemblyQualifiedName, typeConverter)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The fully qualified name of the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, string type, string category, string description, object defaultValue, string editor, Type typeConverter)
			: this(name, type, category, description, defaultValue, editor, typeConverter.AssemblyQualifiedName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The fully qualified name of the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, Type type, string category, string description, object defaultValue, string editor, Type typeConverter)
			: this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor, typeConverter.AssemblyQualifiedName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The Type that represents the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, string type, string category, string description, object defaultValue, Type editor, Type typeConverter)
			: this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName, typeConverter.AssemblyQualifiedName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		/// <param name="editor">The Type that represents the type of the editor for this property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpecification(string name, Type type, string category, string description, object defaultValue, Type editor, Type typeConverter)
			: this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor.AssemblyQualifiedName, typeConverter.AssemblyQualifiedName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">The fully qualified name of the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is no default value.</param>
		public PropertySpecification(string name, string type, string category, string description, object defaultValue)
		{
			this.Name = name;
			this.TypeName = type;
			this.Category = category;
			this.Description = description;
			this.DefaultValue = defaultValue;
			this.Attributes = null;
		}

		#region Class Property Declarations
		/// <summary>
		/// Gets or sets a collection of additional Attributes for this property.  This can
		/// be used to specify attributes beyond those supported intrinsically by the
		/// PropertySpec class, such as ReadOnly and Browsable.
		/// </summary>
		public Attribute[] Attributes { get; set; }

		/// <summary>
		/// Gets or sets the category name of this property.
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the fully qualified name of the type converter
		/// type for this property.
		/// </summary>
		public string ConverterTypeName { get; set; }

		/// <summary>
		/// Gets or sets the default value of this property.
		/// </summary>
		public object DefaultValue { get; set; }

		/// <summary>
		/// Gets or sets the help text description of this property.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the fully qualified name of the editor type for
		/// this property.
		/// </summary>
		public string EditorTypeName { get; set; }

		/// <summary>
		/// Gets or sets the name of this property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the fully qualfied name of the type of this
		/// property.
		/// </summary>
		public string TypeName { get; set; }
		#endregion
	}
}
