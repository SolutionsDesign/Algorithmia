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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using SD.Tools.BCLExtensions.SystemRelated;

namespace SD.Tools.Algorithmia.GeneralDataStructures.PropertyEditing
{
	/// <summary>
	/// Class which represents a collection of custom properties that can be selected into a PropertyGrid to provide functionality beyond that of the 
	/// simple reflection normally used to query an object's properties.
	/// </summary>
	public class PropertyBag : ICustomTypeDescriptor
	{
		#region Events
		/// <summary>
		/// Event which is raised when a PropertyGrid requests the value of a property.
		/// </summary>
		public event EventHandler<PropertySpecificationEventArgs> GetValue;
		/// <summary>
		/// Event which is raised when the user changes the value of a property in a PropertyGrid.
		/// </summary>
		public event EventHandler<PropertySpecificationEventArgs> SetValue;
		#endregion

		/// <summary>
		/// Initializes a new instance of the PropertyBag class.
		/// </summary>
		public PropertyBag()
		{
			this.DefaultProperty = null;
			this.PropertySpecifications = new List<PropertySpecification>();
		}


		/// <summary>
		/// Raises the GetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected internal virtual void OnGetValue(PropertySpecificationEventArgs e)
		{
			this.GetValue.RaiseEvent(this, e);
		}


		/// <summary>
		/// Raises the SetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected internal virtual void OnSetValue(PropertySpecificationEventArgs e)
		{
			this.SetValue.RaiseEvent(this, e);
		}


		#region ICustomTypeDescriptor explicit interface definitions
		/// <summary>
		/// Returns a collection of custom attributes for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.AttributeCollection"/> containing the attributes for this object.
		/// </returns>
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		/// <summary>
		/// Returns the class name of this instance of a component.
		/// </summary>
		/// <returns>
		/// The class name of the object, or null if the class does not have a name.
		/// </returns>
		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		/// <summary>
		/// Returns the name of this instance of a component.
		/// </summary>
		/// <returns>
		/// The name of the object, or null if the object does not have a name.
		/// </returns>
		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		/// <summary>
		/// Returns a type converter for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.TypeConverter"/> that is the converter for this object, or null if there is no <see cref="T:System.ComponentModel.TypeConverter"/> for this object.
		/// </returns>
		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		/// <summary>
		/// Returns the default event for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptor"/> that represents the default event for this object, or null if this object does not have events.
		/// </returns>
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		/// <summary>
		/// Returns the default property for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the default property for this object, or null if this object does not have properties.
		/// </returns>
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			PropertySpecification propertySpec = this.PropertySpecifications.FirstOrDefault(p => p.Name == this.DefaultProperty);
			return propertySpec == null ? null : new PropertySpecificationDescriptor(propertySpec, this, null);
		}

		/// <summary>
		/// Returns an editor of the specified type for this instance of a component.
		/// </summary>
		/// <param name="editorBaseType">A <see cref="T:System.Type"/> that represents the editor for this object.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> of the specified type that is the editor for this object, or null if the editor cannot be found.
		/// </returns>
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		/// <summary>
		/// Returns the events for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the events for this component instance.
		/// </returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		/// <summary>
		/// Returns the events for this instance of a component using the specified attribute array as a filter.
		/// </summary>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the filtered events for this component instance.
		/// </returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		/// <summary>
		/// Returns the properties for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the properties for this component instance.
		/// </returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
		}

		/// <summary>
		/// Returns the properties for this instance of a component using the attribute array as a filter.
		/// </summary>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the filtered properties for this component instance.
		/// </returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			// Wrap all property specifications in PropertySpecificationDescriptor instances which are returned in a PropertyDescriptorCollection
			// so a calling object will think that the properties described by the descriptors are the properties of this PropertyBag, which is
			// what we want. When the calling object alters a property value, this change is going through PropertySpecificationDescriptor's SetValue
			// which ends up in this class' OnSetValue method. The value retrieval follows the same route, but then via GetValue -> this class' OnGetValue. 

			var toReturn = new List<PropertySpecificationDescriptor>();
			foreach(var specification in PropertySpecifications)
			{
				List<Attribute> additionalAttributes = new List<Attribute>();

				// If a category, description, editor, or type converter are specified
				// in the PropertySpecification, create attributes to define that relationship.
				if(specification.Category != null)
				{
					additionalAttributes.Add(new CategoryAttribute(specification.Category));
				}

				if(specification.Description != null)
				{
					additionalAttributes.Add(new DescriptionAttribute(specification.Description));
				}

				if(specification.EditorTypeName != null)
				{
					additionalAttributes.Add(new EditorAttribute(specification.EditorTypeName, typeof(UITypeEditor)));
				}

				if(specification.ConverterTypeName != null)
				{
					additionalAttributes.Add(new TypeConverterAttribute(specification.ConverterTypeName));
				}

				if(specification.DefaultValue!=null)
				{
					additionalAttributes.Add(new DefaultValueAttribute(specification.DefaultValue));
				}

				// Additionally, append the custom attributes associated with the PropertySpecification, if any.
				if(specification.Attributes != null)
				{
					additionalAttributes.AddRange(specification.Attributes);
				}
				toReturn.Add(new PropertySpecificationDescriptor(specification, this, additionalAttributes.ToArray()));
			}
			return new PropertyDescriptorCollection(toReturn.ToArray());
		}

		/// <summary>
		/// Returns an object that contains the property described by the specified property descriptor.
		/// </summary>
		/// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the property whose owner is to be found.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the owner of the specified property.
		/// </returns>
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
		#endregion

		#region Class Property Declarations
		/// <summary>
		/// Gets or sets the name of the default property in the collection.
		/// </summary>
		public string DefaultProperty { get; set; }

		/// <summary>
		/// Gets the collection of properties contained within this PropertyBag.
		/// </summary>
		public List<PropertySpecification> PropertySpecifications { get; private set; }
		#endregion
	}
}