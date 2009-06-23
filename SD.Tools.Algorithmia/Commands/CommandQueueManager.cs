//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2008 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2008 Solutions Design. All rights reserved.
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
using System.Reflection;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;
using System.Threading;

namespace SD.Tools.Algorithmia.Commands
{
	/// <summary>
	/// Singleton provider for the actual CommandQueueManager class which manages the command queues. There's one instance per <i>appDomain</i>. 
	/// One thread can have access to the instance at any given time. 
	/// </summary>
	public static class CommandQueueManagerSingleton
	{
		#region Class Member Declarations
		private static readonly CommandQueueManager _instance = new CommandQueueManager();
		#endregion

		/// <summary>
		/// Initializes the <see cref="CommandQueueManagerSingleton"/> class.
		/// </summary>
		static CommandQueueManagerSingleton()
		{
		}

		/// <summary>
		/// Gets the single CommandQueueManager instance.
		/// </summary>
		/// <returns>the CommandQueueManager instance managed by this singleton.</returns>
		public static CommandQueueManager GetInstance()
		{
			return _instance;
		}
	}


	/// <summary>
	/// Class which in in charge of managing the command queues of the application. It is used in combination of a singleton provider as there's just
	/// one instance alive in the application. 
	/// </summary>
	public class CommandQueueManager
	{
		#region Class Member Declarations
		private CommandQueueStack _activeCommandQueueStack;
		private Dictionary<Guid, CommandQueueStack> _commandQueueStackPerID;
		private object _semaphore;
		private readonly Guid _ownStackId;

		// Per thread, store the active command queue stack id, so a thread context switch can make the thread's active command queue stack again the active one
		// without code / housekeeping inside the thread.
		[ThreadStatic]
		private static Guid _threadActiveCommandQueueStackId = Guid.Empty;
		// Per thread store a flag which signals if the command manager is in an undoable period, which means commands are dequeued after they've executed. 
		[ThreadStatic]
		private static bool _inUndoablePeriod = false;
		#endregion

		#region Events
		/// <summary>
		/// Event which is raised when a command queue action has been performed. Use this event to update observer objects which track changes in the command queue
		/// manager. Raised in the same critical action block as the action itself, so the thread which started the action is also raising this event. 
		/// </summary>
		public event EventHandler<CommandQueueActionPerformedEventArgs> CommandQueueActionPerformed;
		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandQueueManager"/> class.
		/// </summary>
		internal CommandQueueManager()
		{
			_semaphore = new object();
			_commandQueueStackPerID = new Dictionary<Guid, CommandQueueStack>();
			_ownStackId = Guid.NewGuid();
			ActivateCommandQueueStack(_ownStackId);
		}


		/// <summary>
		/// Activates the command queue stack related to the stackId specified, or if there's no stack yet, it creates a new one. 
		/// </summary>
		/// <param name="stackId">The stack id of the stack to activate. Each different part of the application which has to have its own stack of queues
		/// uses its own stackId. If the Empty guid is passed in, it will de-activate the current active queue stack and will make the main queue stack, the one
		/// owned by CommandQueueManager, the active queue.</param>
		public void ActivateCommandQueueStack(Guid stackId)
		{
			if(stackId == Guid.Empty)
			{
				// activate the stack of this manager
				ActivateCommandQueueStack(_ownStackId);
			}
			else
			{
				try
				{
					ThreadEnter();
					if(!_commandQueueStackPerID.TryGetValue(stackId, out _activeCommandQueueStack))
					{
						// not yet known, create a new one
						_activeCommandQueueStack = new CommandQueueStack(stackId);
						_commandQueueStackPerID.Add(stackId, _activeCommandQueueStack);
						// push the initial, main queue onto the stack
						_activeCommandQueueStack.Push(new CommandQueue());
					}
					// store the id, so a thread switch can re-set the active command queue stack.
					_threadActiveCommandQueueStackId = stackId;
				}
				finally
				{
					ThreadExit();
				}
			}
		}


		/// <summary>
		/// Starts a period of command execution which aren't undoable. 
		/// </summary>
		public void BeginNonUndoablePeriod()
		{
			try
			{
				ThreadEnter();
				_inUndoablePeriod = true;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Ends the undoable period started with BeginUndoablePeriod. Commands enqueued and ran after this method will be undoable again. 
		/// </summary>
		public void EndNonUndoablePeriod()
		{
			try
			{
				ThreadEnter();
				_inUndoablePeriod = false;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Enqueues the command in the top queue on the active stack. It expects there is an active command queue: if it's not one activated by the callee, it's the
		/// one belonging to this manager, which owns its own main queue. 
		/// </summary>
		/// <param name="toEnqueue">The command to enqueue.</param>
		public void EnqueueCommand(CommandBase toEnqueue)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(toEnqueue, "toEnqueue");
				_activeCommandQueueStack.Peek().EnqueueCommand(toEnqueue);
				this.CommandQueueActionPerformed.RaiseEvent(this, 
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandEnqueued, _activeCommandQueueStack.StackId));
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Enqueues and runs the command in the top queue on the active stack. It expects there is an active command queue: if it's not one activated by 
		/// the callee, it's the one belonging to this manager, which owns its own main queue. 
		/// </summary>
		/// <param name="toEnqueueAndRun">The command to enqueue and run.</param>
		public void EnqueueAndRunCommand(CommandBase toEnqueueAndRun)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(toEnqueueAndRun, "toEnqueue");
				_activeCommandQueueStack.Peek().EnqueueCommand(toEnqueueAndRun);
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandEnqueued, _activeCommandQueueStack.StackId));
				_activeCommandQueueStack.Peek().DoCurrentCommand();
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandExecuted, _activeCommandQueueStack.StackId));
				if(_inUndoablePeriod)
				{
					_activeCommandQueueStack.Peek().DequeueLastExecutedCommand();
					this.CommandQueueActionPerformed.RaiseEvent(this,
							new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandDequeued, _activeCommandQueueStack.StackId));
				}
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Undo'es the last command in the top queue
		/// </summary>
		public void UndoLastCommand()
		{
			try
			{
				ThreadEnter();
				_activeCommandQueueStack.Peek().UndoPreviousCommand();
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.UndoPerformed, _activeCommandQueueStack.StackId));
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Redo'es the last command in the top queue.
		/// </summary>
		public void RedoLastCommand()
		{
			try
			{
				ThreadEnter();
				_activeCommandQueueStack.Peek().DoCurrentCommand();
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.RedoPerformed, _activeCommandQueueStack.StackId));
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Gets the command queue stack related to the stackid passed in
		/// </summary>
		/// <param name="stackId">The stack id of the stack to return. Each different part of the application which has to have its own stack of queues
		/// uses its own stackId. If the Empty guid is passed in, it will return the main queue stack, the one owned by CommandQueueManager.</param>
		/// <returns>The stack related to the stackid passed in or null if the stack isn't found</returns>
		public CommandQueueStack GetCommandQueueStackForId(Guid stackId)
		{
			try
			{
				ThreadEnter();
				CommandQueueStack toReturn = null;
				Guid idToUse = stackId;
				if(idToUse == Guid.Empty)
				{
					idToUse = _ownStackId;
				}
				_commandQueueStackPerID.TryGetValue(idToUse, out toReturn);
				return toReturn;
			}
			finally
			{
				ThreadExit();
			}
		}
		

		/// <summary>
		/// Pushes the command queue passed in onto the active stack. This is done when a command is about to be executed. 
		/// </summary>
		/// <param name="toPush">Command queue to push.</param>
		internal void PushCommandQueueOnActiveStack(CommandQueue toPush)
		{
			try
			{
				ThreadEnter();
				_activeCommandQueueStack.Push(toPush);
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandQueuePushed, _activeCommandQueueStack.StackId));
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Pops the top command queue from the active stack. Done when a command has been executed
		/// </summary>
		/// <returns>the command queue at the top of the active stack</returns>
		internal CommandQueue PopCommandQueueFromActiveStack()
		{
			try
			{
				ThreadEnter();
				CommandQueue toReturn = _activeCommandQueueStack.Pop();
				this.CommandQueueActionPerformed.RaiseEvent(this,
						new CommandQueueActionPerformedEventArgs(CommandQueueActionType.RedoPerformed, _activeCommandQueueStack.StackId));
				return toReturn;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Entrance routine for a thread call to this class. Called from every public method at the very beginning.
		/// </summary>
		private void ThreadEnter()
		{
			Monitor.Enter(_semaphore);
			if(_threadActiveCommandQueueStackId != Guid.Empty)
			{
				if((_activeCommandQueueStack == null) ||
					((_activeCommandQueueStack != null) && (_threadActiveCommandQueueStackId != _activeCommandQueueStack.StackId)))
				{
					ActivateCommandQueueStack(_threadActiveCommandQueueStackId);
				}
			}
		}


		/// <summary>
		/// Exit routine for a thread call to this class. Called from every public method at the end.
		/// </summary>
		private void ThreadExit()
		{
			Monitor.Exit(_semaphore);
		}
	}
}
