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

namespace SD.Tools.Algorithmia.Commands
{
	/// <summary>
	/// Abstract base class for command classes to use in the Command pattern implementation. A base class is used instead of an interface to be able to 
	/// keep the Do and Undo methods internal, as execution of commands should be done through the CommandQueue instances.
	/// </summary>
	public abstract class CommandBase
	{
		#region Class Member Declarations
		private bool _commandQueuePushed;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandBase"/> class.
		/// </summary>
		protected CommandBase() : this(string.Empty)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CommandBase"/> class.
		/// </summary>
		/// <param name="description">The description.</param>
		protected CommandBase(string description)
		{
			this.OwnCommandQueue = new CommandQueue();
			this.Description = description;
			_commandQueuePushed = false;
		}


		/// <summary>
		/// Re-executes the command. Normally this is simply calling 'Do', however in an undoable period redo it's calling PerformRedo.
		/// </summary>
		protected internal virtual void Redo()
		{
			if(CommandQueueManagerSingleton.GetInstance().IsInUndoablePeriod)
			{
				PerformRedo();
			}
			else
			{
				Do();
			}
		}
        

		/// <summary>
		/// Pushes the command queue on active stack if required.
		/// </summary>
		internal void PushCommandQueueOnActiveStackIfRequired()
		{
			if(!_commandQueuePushed)
			{
				CommandQueueManagerSingleton.GetInstance().PushCommandQueueOnActiveStack(this.OwnCommandQueue);
				_commandQueuePushed = true;
			}
		}


		/// <summary>
		/// Pops the command queue from active stack if required.
		/// </summary>
		internal void PopCommandQueueFromActiveStackIfRequired()
		{
			if(_commandQueuePushed)
			{
				CommandQueueManagerSingleton.GetInstance().PopCommandQueueFromActiveStack();
				_commandQueuePushed = false;
			}
		}
        

		/// <summary>
		/// Performs the redo action.
		/// </summary>
		private void PerformRedo()
		{
			Do();
			while(this.OwnCommandQueue.CanDo)
			{
				this.OwnCommandQueue.RedoCurrentCommand();
			}
		}


        /// <summary>
		/// Executes the command.
		/// </summary>
		protected internal abstract void Do();
		/// <summary>
		/// Undo's the action done with <see cref="Do"/>.
		/// </summary>
		protected internal abstract void Undo();


		#region Class Property Declarations
		/// <summary>
		/// Gets the own command queue of this command. This queue is then used to store commands which are spawned when this command is executed.
		/// </summary>
		public CommandQueue OwnCommandQueue { get; private set;}
		/// <summary>
		/// Gets or sets the description of the command. This description can be used to show the commands in a queue visually on a screen. 
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Gets or sets the before Do action, which is an action executed right before Do is called on this command
		/// </summary>
		public Action BeforeDoAction { get; set; }
		/// <summary>
		/// Gets or sets the after Do action, which is an action executed right after Do has been called on this command
		/// </summary>
		public Action AfterDoAction { get; set; }
		/// <summary>
		/// Gets or sets the before Undo action, which is an action executed right before Undo is called on this command
		/// </summary>
		public Action BeforeUndoAction { get; set; }
		/// <summary>
		/// Gets or sets the after Undo action, which is an action executed right after Undo has been called on this command
		/// </summary>
		public Action AfterUndoAction { get; set; }
		#endregion
	}
}
