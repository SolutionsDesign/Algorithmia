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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.Commands
{
	/// <summary>
	/// Special command class which marks an Undoable Period. It doesn't perform a command itself, but marks a start and an end to other commands to be 
	/// threated as a single undoable/redoable unit. It has special features: redo will re-do all contained commands and will put the CommandManager
	/// in a special state which blocks any new commands being queued during redo. Undo will not clear the queues after a queue is completely undone. 
	/// The advantage of an undoable period is that it doesn't call methods to do things (like a command would) and therefore can be used to mark a method
	/// which creates new objects as an undoable period while all actions performed on datastructures inside that method are tracked in the queue
	/// of this command. 
	/// An Undoable Period is to be used with undo/redo areas where new objects are created and stored in datastructures. Re-doing these kind of areas with
	/// normal commands will re-create new instances which causes problems with follow up commands if these are re-done too: those commands work on the
	/// previous objects perhaps, causing problems along te way. With an undoable period you can prevent this from happening as any method which is 
	/// called through a command should be replaced with a normal method call and inside the method you should mark the code as an undoable period.
	/// </summary>
	public class UndoablePeriodCommand : Command<object>
	{
		#region Class Member Declarations
		private bool _originalIsInUndoablePeriodFlagValue;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="UndoablePeriodCommand"/> class.
		/// </summary>
		/// <param name="description">The description of the undoable period.</param>
		public UndoablePeriodCommand(string description) :  base(()=> { return;  }, null, description)
		{
		}


		/// <summary>
		/// Re-executes the command. Normally this is simply calling 'Do', however in an undoable period redo it's calling PerformRedo.
		/// </summary>
		protected internal override void Redo()
		{
			try
			{
				_originalIsInUndoablePeriodFlagValue = CommandQueueManagerSingleton.GetInstance().IsInUndoablePeriod;
				CommandQueueManagerSingleton.GetInstance().SetUndoablePeriodFlag(true);
				base.Redo();
			}
			finally
			{
				CommandQueueManagerSingleton.GetInstance().SetUndoablePeriodFlag(_originalIsInUndoablePeriodFlagValue);
				// perform pop on queue, as our queue was pushed on the stack by the Do() method which was called by the Redo method 
				this.PopCommandQueueFromActiveStackIfRequired();
			}
		}


		/// <summary>
		/// Executes the command.
		/// </summary>
		protected internal override void Do()
		{
			PushCommandQueueOnActiveStackIfRequired();
		}


		/// <summary>
		/// Undo's the action done with <see cref="Do"/>.
		/// </summary>
		protected internal override void Undo()
		{
			try
			{
				_originalIsInUndoablePeriodFlagValue = CommandQueueManagerSingleton.GetInstance().IsInUndoablePeriod;
				CommandQueueManagerSingleton.GetInstance().SetUndoablePeriodFlag(true);
				base.Undo();
			}
			finally
			{
				CommandQueueManagerSingleton.GetInstance().SetUndoablePeriodFlag(_originalIsInUndoablePeriodFlagValue);
			}
		}
	}
}
