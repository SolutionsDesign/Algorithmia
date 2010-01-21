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

namespace SD.Tools.Algorithmia.Commands
{
	/// <summary>
	/// Simple class for the event args for the CommandQueueActionPerformed event raised in the CommandQueueManager instance.
	/// </summary>
	public class CommandQueueActionPerformedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandQueueActionPerformedEventArgs"/> class.
		/// </summary>
		/// <param name="actionType">Type of the action.</param>
		/// <param name="activeCommandStackId">The active command stack id. The command is executed on a queue in the stack with this id. Observers
		/// should examine this id and decide whether they act upon this event or ignore it (e.g. the command was on a different stack)</param>
		public CommandQueueActionPerformedEventArgs(CommandQueueActionType actionType, Guid activeCommandStackId)
		{
			this.ActionPerformed = actionType;
			this.ActiveCommandStackId = activeCommandStackId;
		}

		/// <summary>
		/// Gets or sets the action performed.
		/// </summary>
		public CommandQueueActionType ActionPerformed { get; set; }
		/// <summary>
		/// Gets or sets the active command stack id. The command is executed on a queue in the stack with this id. Observers
		/// should examine this id and decide whether they act upon this event or ignore it (e.g. the command was on a different stack)
		/// </summary>
		public Guid ActiveCommandStackId { get; set; }
	}
}
