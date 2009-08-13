//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2009 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2009 Solutions Design. All rights reserved.
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SD.Tools.Algorithmia;
using SD.Tools.Algorithmia.GeneralDataStructures.PropertyEditing;
using NUnit.Framework;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Tests for the PropertyBag class
	/// </summary>
	[TestFixture]
	[System.ComponentModel.DesignerCategory("Code")]			// leave full type specification in tact, otherwise the attribute doesn't work in VS.NET
	public class PropertyBagTests
	{
		[Test]
		public void SimplePropertyDescriptorConstructionTest()
		{
			PropertyBag bag = new PropertyBag();
			// add some property specifications. 
			bag.PropertySpecifications.Add(new PropertySpecification("Property 1", typeof(string), "Cat1", "Prop1 desc", "Foo"));
			bag.PropertySpecifications.Add(new PropertySpecification("Property 2", typeof(string), "Cat1", "Prop2 desc", "Bar"));
			bag.PropertySpecifications.Add(new PropertySpecification("Property 3", typeof(string), "Cat2", "Prop3 desc", string.Empty));

			// open a testform which binds the propertybag to a property grid. There's no value store so edited values are not stored. Just
			// to show the categorization etc.
			using(TestForm f = new TestForm(bag))
			{
				f.ShowDialog();
			}
		}

		[Test]
		public void AdvancedPropertyDescriptorConstructionTestWithEvents()
		{
			PropertyBag bag = new PropertyBag();
			// add some property specifications. 
			var property1 = new PropertySpecification("Property 1", typeof(string), "Cat1", "Prop1 desc", "Foo");
			// make readonly
			property1.Attributes.Add(ReadOnlyAttribute.Yes);
			bag.PropertySpecifications.Add(property1);

			// add expanding property
			var property2 = new PropertySpecification("Picture", typeof(Image), "Some Category", "This is a sample description.");
			property2.Attributes.Add(new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
			bag.PropertySpecifications.Add(property2);

			// custom editor
			var property3 = new PropertySpecification("Source folder", typeof(string), "OutputFolders", "The output folder for the sourcecode", "c:\\temp");
			property3.Attributes.Add(new EditorAttribute(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor)));
			bag.PropertySpecifications.Add(property3);

			// use value list
			var property4 = new PropertySpecification("PickAValue", typeof(string), "Cat1", "A property which value has to be picked from a list");
			property4.TypeConverterType = typeof(PropertySpecificationValuesListTypeConverter);
			property4.ValueList.AddRange(new[] { "One", "Two", "Three", "Many" });
			bag.PropertySpecifications.Add(property4);

			// value list to store values in
			Dictionary<string, object> values = new Dictionary<string, object>();

			// event handler binding so values get store inside the dictionary and also retrieved from it. 
			bag.GetValue += (sender, e) => { e.Value = values.GetValue(e.Property.Name); };
			bag.SetValue += (sender, e) => { values[e.Property.Name] = e.Value; };
            
			// open a testform which binds the bag to the propertygrid. Editing values will store the values in the dictionary, default values are
			// not in the dictionary.
			using(TestForm f = new TestForm(bag))
			{
				f.ShowDialog();
			}
		}

	
		[Test]
		public void AdvancedPropertyDescriptorConstructionTestWithFuncs()
		{
			PropertyBag bag = new PropertyBag();
			// add some property specifications. 
			var property1 = new PropertySpecification("Property 1", typeof(string), "Cat1", "Prop1 desc", "Foo");
			// make readonly
			property1.Attributes.Add(ReadOnlyAttribute.Yes);
			bag.PropertySpecifications.Add(property1);

			// add expanding property
			var property2 = new PropertySpecification("Picture", typeof(Image), "Some Category", "This is a sample description.");
			property2.Attributes.Add(new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
			bag.PropertySpecifications.Add(property2);

			// custom editor
			var property3 = new PropertySpecification("Source folder", typeof(string), "OutputFolders", "The output folder for the sourcecode", "c:\\temp");
			property3.Attributes.Add(new EditorAttribute(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor)));
			property3.ConvertEmptyStringToNull = true;
			bag.PropertySpecifications.Add(property3);

			// use value list
			var property4 = new PropertySpecification("PickAValue", typeof(string), "Cat1", "A property which value has to be picked from a list", "One");
			property4.TypeConverterType = typeof(PropertySpecificationValuesListTypeConverter);
			property4.ValueList.AddRange(new[] { "One", "Two", "Three", "Many" });
			bag.PropertySpecifications.Add(property4);

			// value list to store values in
			Dictionary<string, object> values = new Dictionary<string, object>();

			// func setting so values get store inside the dictionary and also retrieved from it. Console write added to see what happens when you bind
			// a windows forms propertygrid to a bag: many many redundant calls to the getters/setters occur.
			bag.ValueGetterFunc = (s) =>
									{
										var v = values.GetValue(s);
										Console.WriteLine("Get: {0} : {1}", s, v ?? "<null>");
										return v;
									};
			bag.ValueSetterFunc = (s, v) =>
			                      	{
										values[s] = v;
										Console.WriteLine("Set: {0} : {1}", s, v ?? "<null>");
									};

			// open a testform which binds the bag to the propertygrid. Editing values will store the values in the dictionary, default values are
			// not in the dictionary.
			using(TestForm f = new TestForm(bag))
			{
				f.ShowDialog();
			}
		}
	}


	public class TestForm : Form
	{
		public TestForm(object toBind) : base()
		{
			this.StartPosition = FormStartPosition.CenterScreen;

			PropertyGrid grid = new PropertyGrid();
			this.Controls.Add(grid);
			grid.Dock = DockStyle.Fill;

			grid.SelectedObject = toBind;
		}
	}
}
