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
using System.Reflection;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;
using System.Threading;
using SD.Tools.BCLExtensions.SystemRelated;

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
		private readonly Dictionary<Guid, CommandQueueStack> _commandQueueStackPerID;
		private readonly object _semaphore;
		private readonly Guid _ownStackId;
		private bool _raiseEvents;

		// Per thread, store the active command queue stack id, so a thread context switch can make the thread's active command queue stack again the active one
		// without code / housekeeping inside the thread.
		[ThreadStatic]
		private static Guid _threadActiveCommandQueueStackId = Guid.Empty;
		// Per thread store a flag which signals if the command manager is in a non-undoable period, which means commands are dequeued after they've executed. 
		// these flags are stored on a stack so multiple times a call to Begin/end doesn't ruin a previous value.
		[ThreadStatic]
		private Stack<bool> _isInNonUndoablePeriodStack;
		/// <summary>
		/// per thread store a flag which signals if the command manager is in a special mode called 'an undoable period', which means 'Redo' commands 
		/// execute their queues and 'Undo' commands don't clear the queue, plus newly queued commands are ignored. Only set if an UndoablePeriod is Undone/Redone, 
		/// otherwise false.
		/// </summary>
		[ThreadStatic]
		private static bool _inUndoablePeriod;

		/// <summary>
		/// Flag to signal the CommandQueueManager and command objects whether to throw a DoDuringUndoException if a Do action is detected during an Undo action.
		/// When set to false, the Do action is ignored. Default is true. Leave to true to easily detect bugs in code which utilizes Do/Undo functionality.
		/// </summary>
		public static bool ThrowExceptionOnDoDuringUndo = true;
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
			_raiseEvents = true;
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
		/// Resets the active command queue stack. This means that the stack will get all its commands be removed and an empty queue is the result.
		/// </summary>
		public void ResetActiveCommandQueue()
		{
			try
			{
				ThreadEnter();
				_activeCommandQueueStack.Clear();
				_activeCommandQueueStack.Push(new CommandQueue());
			}
			finally
			{
				ThreadExit();
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
				if(_isInNonUndoablePeriodStack==null)
				{
					_isInNonUndoablePeriodStack = new Stack<bool>();
				}
				_isInNonUndoablePeriodStack.Push(true);
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Ends the non-undoable period started with BeginNonUndoablePeriod. Commands enqueued and ran after this method will be undoable again. 
		/// </summary>
		public void EndNonUndoablePeriod()
		{
			try
			{
				ThreadEnter();
				if(_isInNonUndoablePeriodStack==null)
				{
					return;
				}
				if(_isInNonUndoablePeriodStack.Count==0)
				{
					return;
				}
				_isInNonUndoablePeriodStack.Pop();
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Sets the command manager in a special mode where all subsequential commands are tracked inside an UndoablePeriodCommand which, when undone/redone
		/// will not destroy its command queue during undo and won't accept new commands when redone, which is useful when you want to mark a method as an
		/// undoable piece of code and the method creates objects, which can be a problem with a normal command calling the method because the objects created
		/// inside the method are re-created (so you'll get new instances) when the command is redone. If follow up commands work on the instances, redoing these
		/// commands as well causes a problem as they'll work on objects which aren't there.
		/// </summary>
		/// <param name="cmd">The command to use for the undoableperiod.</param>
		public void BeginUndoablePeriod(UndoablePeriodCommand cmd)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(cmd, "cmd");
				EnqueueAndRunCommand(cmd);
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Ends the undoable period started with BeginUndoablePeriod.
		/// </summary>
		/// <param name="cmd">The command used for the undoable period.</param>
		public void EndUndoablePeriod(UndoablePeriodCommand cmd)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(cmd, "cmd");
				// the period has ended, so we've to pop the queue of the period from the stack.
				cmd.PopCommandQueueFromActiveStackIfRequired();
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Performs the code func inside an undoable period guarded by the command specified. Use this method if the code to run inside the 
		/// undoable period is well known and can be wrapped inside a single func. The func codeToExecuteInPeriodFunc is executed once, all
		/// undo/redo logic is done by undo/redo-ing commands which were ran due to state changes caused by codeToExecuteInPeriodFunc.
		/// If you want to run a command again during redo, you should enqueue and run a normal command instead of using this method.
		/// To create an undoable period, you can also call BeginUndoablePeriod and EndUndoablePeriod.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		/// <param name="codeToExecuteInPeriodFunc">The code to execute in period func.</param>
		public void PerformUndoablePeriod(UndoablePeriodCommand cmd, Action codeToExecuteInPeriodFunc)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(cmd, "cmd");
				ArgumentVerifier.CantBeNull(codeToExecuteInPeriodFunc, "codeToExecuteInPeriod");
				BeginUndoablePeriod(cmd);
				if(cmd.BeforeDoAction!=null)
				{
					cmd.BeforeDoAction();
				}
				codeToExecuteInPeriodFunc();
				if(cmd.AfterDoAction!=null)
				{
					cmd.AfterDoAction();
				}
				EndUndoablePeriod(cmd);
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Determines whether the commandqueuestack with the id passed in can perform a Do operation (or redo)
		/// </summary>
		/// <param name="stackId">The stack id.</param>
		/// <returns>true if the stack contains a command which can be performed, false otherwise.</returns>
		public bool CanDo(Guid stackId)
		{
			try
			{
				ThreadEnter();
				return GetCommandQueueStackForId(stackId).Peek().CanDo;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Determines whether the commandqueuestack with the id passed in can perform an Undo operation
		/// </summary>
		/// <param name="stackId">The stack id.</param>
		/// <returns>true if the stack contains a command which can be undone, false otherwise.</returns>
		public bool CanUndo(Guid stackId)
		{
			try
			{
				ThreadEnter();
				return GetCommandQueueStackForId(stackId).Peek().CanUndo;
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
		/// <returns>true if enqueue action succeeded, false otherwise</returns>
		public bool EnqueueCommand(CommandBase toEnqueue)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(toEnqueue, "toEnqueue");
				bool enqueueSucceeded = _activeCommandQueueStack.Peek().EnqueueCommand(toEnqueue);
				if(enqueueSucceeded)
				{
					RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandEnqueued, _activeCommandQueueStack.StackId));
				}
				return enqueueSucceeded;
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
		/// <remarks>If enqueue action fails due to an undo action that's in progress and ThrowExceptionOnDoDuringUndo is set to false, this routine is a no-op</remarks>
		/// <returns>true if enqueue and Do succeeded, false otherwise</returns>
		public bool EnqueueAndRunCommand(CommandBase toEnqueueAndRun)
		{
			try
			{
				ThreadEnter();
				ArgumentVerifier.CantBeNull(toEnqueueAndRun, "toEnqueue");
				bool enqueueResult = _activeCommandQueueStack.Peek().EnqueueCommand(toEnqueueAndRun);
				if(enqueueResult)
				{
					RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandEnqueued, _activeCommandQueueStack.StackId));
					_activeCommandQueueStack.Peek().DoCurrentCommand();
					RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandExecuted, _activeCommandQueueStack.StackId));
					if(this.IsInNonUndoablePeriod)
					{
						_activeCommandQueueStack.Peek().DequeueLastExecutedCommand();
						RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandDequeued, _activeCommandQueueStack.StackId));
					}
				}
				return enqueueResult;
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
				// set the flag that an undo action is in progress to prevent new commands being added to the top command queue on the active stack. We do this here 
				// as commands which are added through code which is called (indirectly) by the undoFunc of a command undone by this routine (directly or indirectly), 
				// are always added to the command queue at the top of the active stack.
				_activeCommandQueueStack.Peek().UndoInProgress = true;
				_activeCommandQueueStack.Peek().UndoPreviousCommand();
				RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.UndoPerformed, _activeCommandQueueStack.StackId));
			}
			finally
			{
				_activeCommandQueueStack.Peek().UndoInProgress = false;
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
				_activeCommandQueueStack.Peek().RedoCurrentCommand();
				RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.RedoPerformed, _activeCommandQueueStack.StackId));
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Gets the command queue stack currently active in the manager for the active thread
		/// </summary>
		/// <returns>The active command queue stack or null if no stack was active</returns>
		/// <remarks>'Active' is relative to a thread. This means that the returned stack is the active stack for the active thread and should not be passed
		/// to other threads as being the active stack as for every thread a different stack could be active</remarks>
		public CommandQueueStack GetActiveCommandQueueStack()
		{
			try
			{
				ThreadEnter();
				return _activeCommandQueueStack;
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
				Guid idToUse = stackId;
				if(idToUse == Guid.Empty)
				{
					idToUse = _ownStackId;
				}
				CommandQueueStack toReturn;
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
				RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.CommandQueuePushed, _activeCommandQueueStack.StackId));
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
				RaiseCommandQueueActionPerformed(new CommandQueueActionPerformedEventArgs(CommandQueueActionType.RedoPerformed, _activeCommandQueueStack.StackId));
				return toReturn;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Sets the undoable period flag.
		/// </summary>
		/// <param name="value">the value to set the flag to.</param>
		internal void SetUndoablePeriodFlag(bool value)
		{
			try
			{
				ThreadEnter();
				_inUndoablePeriod = value;
			}
			finally
			{
				ThreadExit();
			}
		}


		/// <summary>
		/// Raises the CommandQueueActionPerformed event.
		/// </summary>
		/// <param name="eventArgs">The <see cref="SD.Tools.Algorithmia.Commands.CommandQueueActionPerformedEventArgs"/> instance containing the event data.</param>
		private void RaiseCommandQueueActionPerformed(CommandQueueActionPerformedEventArgs eventArgs)
		{
			if(_raiseEvents)
			{
				this.CommandQueueActionPerformed.RaiseEvent(this, eventArgs);
			}
		}


		/// <summary>
		/// Entrance routine for a thread call to this class. Called from every public method at the very beginning.
		/// </summary>
		private void ThreadEnter()
		{
			Monitor.Enter(_semaphore);
			// if this thread has an active command queue stack, it should be activated, if it's not already active.
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


		#region Class Property Declarations
		/// <summary>
		/// Gets/ sets the RaiseEvents flag. By default, events are raised when an action is performed. To stop this from happening, call this method and pass false.
		/// This flag is for all threads. 
		/// </summary>
		public bool RaiseEvents
		{
			get 
			{
				try
				{
					ThreadEnter();
					return _raiseEvents;
				}
				finally
				{
					ThreadExit();
				}
			}
			set
			{
				try
				{
					ThreadEnter();
					_raiseEvents = value;
				}
				finally
				{
					ThreadExit();
				}
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance is in an undoable period.
		/// </summary>
		internal bool IsInUndoablePeriod
		{
			get { return _inUndoablePeriod; }
		}


		/// <summary>
		/// Gets a value indicating whether this instance is in a non undoable period.
		/// </summary>
		internal bool IsInNonUndoablePeriod
		{
			get
			{
				if((_isInNonUndoablePeriodStack==null) || (_isInNonUndoablePeriodStack.Count==0))
				{
					return false;
				}
				return _isInNonUndoablePeriodStack.Peek();
			}
		}
		#endregion
	}
}
