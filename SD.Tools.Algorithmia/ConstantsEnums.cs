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

namespace SD.Tools.Algorithmia
{
	/// <summary>
	/// Enum for specifying the sort direction for the various sort algorithms
	/// </summary>
	public enum SortDirection
	{
		/// <summary>
		/// Sort ascending
		/// </summary>
		Ascending,
		/// <summary>
		/// Sort descending
		/// </summary>
		Descending
	}


	/// <summary>
	/// Enum for specifying the sort algorithm to use
	/// </summary>
	public enum SortAlgorithm
	{
		/// <summary>
		/// Use selection sort to sort the source
		/// </summary>
		SelectionSort,
		/// <summary>
		/// Use shell sort to sort the source
		/// </summary>
		ShellSort,
		/// <summary>
		/// Use quick sort to sort the source
		/// </summary>
		QuickSort,
	}


	/// <summary>
	/// Enum for specifying the command queue action type in the CommandQueueActionPerformedEventArgs objects.
	/// </summary>
	public enum CommandQueueActionType
	{
		/// <summary>
		/// A command was executed
		/// </summary>
		CommandExecuted,
		/// <summary>
		/// A command was enqueued
		/// </summary>
		CommandEnqueued,
		/// <summary>
		/// A command was undo-ed
		/// </summary>
		UndoPerformed,
		/// <summary>
		/// A command was redo-ed
		/// </summary>
		RedoPerformed,
		/// <summary>
		/// A command's command queue was pushed onto the command stack. (so any spawned commands are enqueued inside the command itself)
		/// </summary>
		CommandQueuePushed,
		/// <summary>
		/// A command's command queue was popped from the command stack. 
		/// </summary>
		CommandQueuePopped,
		/// <summary>
		/// A command was dequeued and the current command pointer was pushed back.
		/// </summary>
		CommandDequeued,
	}
}
