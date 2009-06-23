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
using System.Linq;
using System.Text;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.GeneralDataStructures.EventArguments
{
	/// <summary>
	/// Custom event argument class which is used to signal a change in an element.
	/// </summary>
	[Serializable]
	public class ElementChangedEventArgs<TChangeType, TElement> : EventArgs
	{
		/// <summary>
		/// CTor
		/// </summary>
		/// <param name="typeOfChange">The change type to pass on to subscribers</param>
		/// <param name="involvedElement">The involved element which was changed.</param>
		public ElementChangedEventArgs(TChangeType typeOfChange, TElement involvedElement)
		{
			if(!typeof(TChangeType).IsEnum)
			{
				throw new ArgumentException("The generic type of ElementChangedEventArgs has to be an enum type");
			}
			this.TypeOfChange = typeOfChange;
			this.InvolvedElement = involvedElement;
		}

		#region Class Property Declarations
		/// <summary>
		/// Gets / sets typeOfChange
		/// </summary>
		public TChangeType TypeOfChange { get; set; }
		/// <summary>
		/// Gets or sets the involved element. Can be null in the situation where this event args is used in change events on commandified members.
		/// </summary>
		public TElement InvolvedElement { get; set; }
		#endregion
	}
}
