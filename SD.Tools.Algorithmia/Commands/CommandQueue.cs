//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2014 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2014 Solutions Design. All rights reserved.
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
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;
using System.Collections;

namespace SD.Tools.Algorithmia.Commands
{
	/// <summary>
	/// Class which is used as a command queue to store commands to execute. Supports undo/redo. 
	/// </summary>
	/// <remarks>This command queue uses the policy that the current command pointer points to the command which has been executed the last time. It can only be 
	/// null if it hasn't executed any commands or if the queue is empty.
	/// This policy is different from the policy where the current command pointer points to the command which is about to be executed: as the execution of a command
	/// can spawn commands, it first has to move to the command executed, and then execute it, otherwise there is a state where the current command pointer is actually
	/// the command which was executed last (as the new command to enqueue is spawned during execution of the command), because the move to the new command happens after
	/// the command has been executed in full. 
	/// </remarks>
	public class CommandQueue : IEnumerable<CommandBase>
	{
		#region Class Member Declarations
		private readonly LinkedBucketList<CommandBase> _commands;
		private ListBucket<CommandBase> _currentCommandBucket;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandQueue"/> class.
		/// </summary>
		public CommandQueue()
		{
			_commands = new LinkedBucketList<CommandBase>();
		}


		/// <summary>
		/// Enqueues the command specified and makes it the current command.
		/// </summary>
		/// <param name="toEnqueue">To enqueue.</param>
		/// <returns>true if enqueue action succeeded, false otherwise</returns>
		public bool EnqueueCommand(CommandBase toEnqueue)
		{
			ArgumentVerifier.CantBeNull(toEnqueue, "toEnqueue");
			if(CommandQueueManagerSingleton.GetInstance().IsInUndoablePeriod)
			{
				// ignore, all commands are already there in this mode and this command therefore already is either there or is not important
				return false;
			}
			if(this.UndoInProgress)
			{
				if(CommandQueueManager.ThrowExceptionOnDoDuringUndo)
				{
					// can't add new commands to a queue of a command which executed an undoFunc. This happens through bugs in the using code so we have to 
					// thrown an exception to illustrate that there's a problem with the code using this library.
					throw new DoDuringUndoException();
				}
				// ignore
				return false;
			}

			// clear queue from the active index
			if(_currentCommandBucket == null)
			{
				// clear the complete queue from any commands which might have been undone, as the new command makes them unreachable. 
				_commands.Clear();
			}
			else
			{
				_commands.RemoveAfter(_currentCommandBucket);
			}
			_commands.InsertAfter(new ListBucket<CommandBase>(toEnqueue), _currentCommandBucket);
			return true;
		}


		/// <summary>
		/// Calls the current command's Redo() method, if there's a command left to execute. It first makes the next command the current command and then executes it.
		/// </summary>
		public void RedoCurrentCommand()
		{
			PerformDoRedoCommand(true);
		}


		/// <summary>
		/// Calls the current command's Do() method, if there's a command left to execute. It first makes the next command the current command and then executes it.
		/// </summary>
		public void DoCurrentCommand()
		{
			PerformDoRedoCommand(false);
		}


		/// <summary>
		/// Performs the do / redo action on the current command.
		/// </summary>
		/// <param name="performRedo">if set to <see langword="true"/> perform a redo, otherwise perform a do.</param>
		private void PerformDoRedoCommand(bool performRedo)
		{
			if(this.CanDo)
			{
				MoveNext();
				CommandBase commandToExecute = _currentCommandBucket.Contents;
				if(commandToExecute.BeforeDoAction != null)
				{
					commandToExecute.BeforeDoAction();
				}
				if(performRedo)
				{
					commandToExecute.Redo();
				}
				else
				{
					commandToExecute.Do();
				}
				if(commandToExecute.AfterDoAction != null)
				{
					commandToExecute.AfterDoAction();
				}
			}
		}


		/// <summary>
		/// Calls the last executed command's Undo method, if there's was a command last executed. It then makes the previous command the current command.
		/// </summary>
		public void UndoPreviousCommand()
		{
			if(this.CanUndo)
			{
				CommandBase commandToExecute = _currentCommandBucket.Contents;
				if(commandToExecute.BeforeUndoAction != null)
				{
					commandToExecute.BeforeUndoAction();
				}
				commandToExecute.Undo();
				if(commandToExecute.AfterUndoAction != null)
				{
					commandToExecute.AfterUndoAction();
				}
				MovePrevious();
			}
		}


		/// <summary>
		/// Dequeues the last executed command. This is done in periods which aren't undoable.
		/// </summary>
		public void DequeueLastExecutedCommand()
		{
			if(this.CanUndo)
			{
				ListBucket<CommandBase> toRemove = _currentCommandBucket;
				MovePrevious();
				_commands.Remove(toRemove);
			}
		}


		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear()
		{
			_commands.Clear();
			_currentCommandBucket = null;
		}


		/// <summary>
		/// Moves the previous.
		/// </summary>
		private void MovePrevious()
		{
			if(_currentCommandBucket == null)
			{
				return;
			}
			_currentCommandBucket = _currentCommandBucket.Previous;
		}


		/// <summary>
		/// Moves the current command to the next command in the queue.
		/// </summary>
		private void MoveNext()
		{
			_currentCommandBucket = _currentCommandBucket == null ? _commands.Head : _currentCommandBucket.Next;
		}


		#region IEnumerable<CommandBase> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<CommandBase> GetEnumerator()
		{
			return _commands.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
		
		#region Class Property Declarations
		/// <summary>
		/// Gets or sets a value indicating whether an undo action is in progress. If an undo action is in progress, no commands can be added to this queue, as 
		/// those commands will never be undoable nor processable in a normal way: undo actions should restore state with direct actions. Adding commands to a queue
		/// which is in an undo action will cause an exception. 
		/// </summary>
		internal bool UndoInProgress { get; set;}
		
		/// <summary>
		/// Gets a value indicating whether this command queue can undo the last executed command (so there are commands left to undo) (true), or false if no more
		/// commands can be undone in this queue.
		/// </summary>
		public bool CanUndo
		{
			get { return _currentCommandBucket != null; }
		}


		/// <summary>
		/// Gets a value indicating whether this command queue can do a current command (so there's a command left to execute) (true) or false if no more commands
		/// can be executed in this queue.
		/// </summary>
		public bool CanDo
		{
			get { return (((_currentCommandBucket != null) && (_currentCommandBucket.Next!=null)) || ((_currentCommandBucket==null) && (_commands.Count>0))); }
		}


		/// <summary>
		/// Gets the active command in this queue.
		/// </summary>
		public CommandBase ActiveCommand
		{
			get { return _currentCommandBucket == null ? null : _currentCommandBucket.Contents; }
		}

		/// <summary>
		/// Gets the number of commands in this queue
		/// </summary>
		public int Count
		{
			get { return _commands == null ? 0 : _commands.Count; }
		}
		#endregion
	}
}
