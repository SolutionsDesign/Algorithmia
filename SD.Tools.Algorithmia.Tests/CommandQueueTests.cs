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
using NUnit.Framework;
using SD.Tools.Algorithmia;
using SD.Tools.Algorithmia.Commands;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.Sorting;
using SD.Tools.Algorithmia.Graphs;
using SD.Tools.BCLExtensions.CollectionsRelated;
using SD.Tools.Algorithmia.GeneralDataStructures.EventArguments;
using System.ComponentModel;
using System.Threading;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Tests for command queue behavior.
	/// </summary>
	[TestFixture]
	public class CommandQueueTests
	{
		[Test]
		public void MultiThreadedSyncedCommandifiedListAccessTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			var toTest = new CommandifiedList<string>(isSynchronized: true);
			for(int i = 0; i < 10; i++)
			{
				toTest.Add(i.ToString());
			}

			var waitHandles = new WaitHandle[] {new AutoResetEvent(false), new AutoResetEvent(false)};
			Exception caughtException = null;
			Console.WriteLine("Starting threads...");
			var threadA = new Thread(()=>MultiThreadedSyncedCommandifiedListAccessTest_ThreadA(toTest, waitHandles[0], (e)=> caughtException=e));
			var threadB = new Thread(()=>MultiThreadedSyncedCommandifiedListAccessTest_ThreadB(toTest, waitHandles[1], (e) => caughtException=e));
			threadA.Start();
			threadB.Start();
			Console.WriteLine("Threads started... waiting for handles");
			WaitHandle.WaitAll(waitHandles);
			if(caughtException != null)
			{
				throw caughtException;
			}
			Console.WriteLine("All completed.");
		}


		private void MultiThreadedSyncedCommandifiedListAccessTest_ThreadA(CommandifiedList<string> list, WaitHandle waitHandle, Action<Exception> handler)
		{
			Console.WriteLine("Thread A started");
			try
			{
				var random = new Random((int)DateTime.Now.Ticks);
				for(int i = 0; i < 500; i++)
				{
					//Console.WriteLine("\tThread A: iteration: {0}", i);
					var countBefore = list.Count;
					var index = random.Next(0, countBefore);
					list.RemoveAt(index);
					Assert.AreEqual(countBefore - 1, list.Count);
					CQManager.UndoLastCommand();
					Assert.AreEqual(countBefore, list.Count);
					Thread.Sleep(1);
					CQManager.RedoLastCommand();
					Assert.AreEqual(countBefore - 1, list.Count);
					Thread.Sleep(2);
					CQManager.UndoLastCommand();
					Assert.AreEqual(countBefore, list.Count);
				}
			}
			catch(Exception e)
			{
				handler(e);
			}
			finally
			{
				((AutoResetEvent)waitHandle).Set();
			}
			Console.WriteLine("Thread A ended");
		}


		private void MultiThreadedSyncedCommandifiedListAccessTest_ThreadB(CommandifiedList<string> list, WaitHandle waitHandle, Action<Exception> handler)
		{
			Console.WriteLine("Thread B started");
			try
			{
				for(int i = 0; i < 500; i++)
				{
					//Console.WriteLine("\tThread B: iteration: {0}", i);
					bool lockTaken = false;
					object syncRoot = list.SyncRoot;
					try
					{
						if(list.IsSynchronized)
						{
							Monitor.Enter(syncRoot);
							lockTaken = true;
						}
						int countBefore = list.Count;
						int count = 0;
						foreach(var v in list)
						{
							count++;
						}
						Assert.AreEqual(countBefore, count, "Count after enumeration differs from initial count of list");
					}
					finally
					{
						if(lockTaken)
						{
							Monitor.Exit(syncRoot);
						}
					}
					Thread.Sleep(7);
				}
			}
			catch(Exception e)
			{
				handler(e);
			}
			finally
			{
				((AutoResetEvent)waitHandle).Set();
			}
			Console.WriteLine("Thread B ended");
		}



		[Test]
		public void SingleUndoOfSingleCommandLevelTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			HelperClass h = new HelperClass();
			string name = "Foo";
			h.Name = name;
			Assert.AreEqual(name, h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void MultipleUndoOfSingleCommandLevelTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			HelperClass h = new HelperClass();
			h.Name = "Foo";
			Assert.AreEqual("Foo", h.Name);
			h.Name = "Bar";
			Assert.AreEqual("Bar", h.Name);

			CQManager.UndoLastCommand();
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			// we're now back at the beginstate, with the exception that the queue is currently filled with undone commands.
			// these have to be overwritten with the new ones. 
			h.Name = "Foo";
			Assert.AreEqual("Foo", h.Name);
			h.Name = "Bar";
			Assert.AreEqual("Bar", h.Name);

			CQManager.UndoLastCommand();
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void MultipleUndoRedoOfSingleCommandLevelTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			HelperClass h = new HelperClass();
			h.Name = "Foo";
			Assert.AreEqual("Foo", h.Name);
			h.Name = "Bar";
			Assert.AreEqual("Bar", h.Name);

			CQManager.UndoLastCommand();
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			// we're now back at the beginstate, with the exception that the queue is currently filled with undone commands.
			// we'll now re-do these commands
			CQManager.RedoLastCommand();
			Assert.AreEqual("Foo", h.Name);
			CQManager.RedoLastCommand();
			Assert.AreEqual("Bar", h.Name);

			// now back to the start.
			CQManager.UndoLastCommand();
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void SingleUndoOfMultiCommandLevelTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			HelperClass h = new HelperClass();

			// set the property through a command. As the property name change also spawns a command, it will create a 2-level command set.
			// it doesn't use an undo function, as it doesn't change state itself, that's delegated to another command. Typically one wouldn't do it this way,
			// as this would assume knowledge of how e.Name's setter works, however for the test it's ok. 
			Command<string> nameSetter = new Command<string>(() => h.Name = "Foo");
			CQManager.EnqueueAndRunCommand(nameSetter);
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			// equal to the test above, but now with an undo function. The undo function will try to undo the state as well. e.Name therefore has to check
			// whether the value is different, otherwise the undo call will spawn a new command
			nameSetter = new Command<string>(() => h.Name = "Foo", ()=>h.Name, v=>h.Name=v);
			CQManager.EnqueueAndRunCommand(nameSetter);
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}



		[Test]
		public void MultiUndoRedoOfMultiCommandLevelWithBeforeAfterActionCallsTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			HelperClass h = new HelperClass();

			int beforeDoCounter = 0;
			int afterDoCounter = 0;
			int beforeUndoCounter = 0;
			int afterUndoCounter = 0;

			// set the property through a command. As the property name change also spawns a command, it will create a 2-level command set.
			// it doesn't use an undo function, as it doesn't change state itself, that's delegated to another command. Typically one wouldn't do it this way,
			// as this would assume knowledge of how e.Name's setter works, however for the test it's ok. 
			Command<string> nameSetter = new Command<string>(() => h.Name = "Foo")
				{
					AfterDoAction = () => afterDoCounter++,
					AfterUndoAction = () => afterUndoCounter++,
					BeforeDoAction = () => beforeDoCounter++,
					BeforeUndoAction = () => beforeUndoCounter++
				};
			CQManager.EnqueueAndRunCommand(nameSetter);
			Assert.AreEqual("Foo", h.Name);
			CQManager.UndoLastCommand();
			Assert.AreEqual(string.Empty, h.Name);

			CQManager.RedoLastCommand();
			CQManager.UndoLastCommand();
			CQManager.RedoLastCommand();
			CQManager.UndoLastCommand();
			// we called do 3 times, and undo also 3 times
			Assert.AreEqual(3, beforeDoCounter);
			Assert.AreEqual(3, beforeUndoCounter);
			Assert.AreEqual(3, afterDoCounter);
			Assert.AreEqual(3, afterUndoCounter);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListClearTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>() { "Foo", "Bar", "Blah" };
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);
			Assert.AreEqual("Blah", toTest[2]);

			// perform a clear operation. We'll undo this later on.
			toTest.Clear();
			Assert.AreEqual(0, toTest.Count);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);
			Assert.AreEqual("Blah", toTest[2]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}

		
		[Test]
		public void CommandifiedListInsertTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>() { "Foo", "Bar"};
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform an insert operation, this can be triggered by both 'Add' and 'Insert'. We'll undo this later on.
			toTest.Add("Blah");
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Blah", toTest[2]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			toTest.Insert(1, "Blah");
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Blah", toTest[1]);
			Assert.AreEqual("Bar", toTest[2]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListRemoveTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>() { "Foo", "Bar" };
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform a remove operation. We'll undo this later on.
			toTest.Remove("Foo");
			Assert.AreEqual(1, toTest.Count);
			Assert.AreEqual("Bar", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			toTest.RemoveAt(1);
			Assert.AreEqual(1, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListSetItemTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>() { "Foo", "Bar" };
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform a set index operation. We'll undo this later on.
			toTest[0] = "Blah";
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Blah", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// use a library extension method to swap two items. We want to roll back the swap call completely, so the SwapValues call is seen
			// as an atomic action. We therefore have to create a command to make it undoable as an atomic action. It doesn't have an undo action,
			// it relies on the actions it executes by itself to undo.
			CQManager.EnqueueAndRunCommand(new Command<string>(() => toTest.SwapValues(0, 1)));
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Bar", toTest[0]);
			Assert.AreEqual("Foo", toTest[1]);

			// undo operation. This is undoing the call to SwapValues, which by undoing that, will undo all the actions SwapValues took, i.e. setting 2 items at
			// 2 indexes.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListSortWithUndoTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>() { "aaa", "aab", "aba", "baa" };

			// use command to sort the list, so it's undoable.
			CQManager.EnqueueAndRunCommand(new Command<string>(() => toTest.Sort(SortAlgorithm.ShellSort, SortDirection.Descending)));
			Assert.AreEqual("baa", toTest[0]);
			Assert.AreEqual("aba", toTest[1]);
			Assert.AreEqual("aab", toTest[2]);
			Assert.AreEqual("aaa", toTest[3]);

			// undo the sort
			CQManager.UndoLastCommand();
			Assert.AreEqual("aaa", toTest[0]);
			Assert.AreEqual("aab", toTest[1]);
			Assert.AreEqual("aba", toTest[2]);
			Assert.AreEqual("baa", toTest[3]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}




		[Test]
		public void CommandifiedListClear_Sync_Test()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>(isSynchronized:true) { "Foo", "Bar", "Blah" };
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);
			Assert.AreEqual("Blah", toTest[2]);

			// perform a clear operation. We'll undo this later on.
			toTest.Clear();
			Assert.AreEqual(0, toTest.Count);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);
			Assert.AreEqual("Blah", toTest[2]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListInsert_Sync_Test()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>(isSynchronized: true) { "Foo", "Bar" };
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform an insert operation, this can be triggered by both 'Add' and 'Insert'. We'll undo this later on.
			toTest.Add("Blah");
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Blah", toTest[2]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			toTest.Insert(1, "Blah");
			Assert.AreEqual(3, toTest.Count);
			Assert.AreEqual("Blah", toTest[1]);
			Assert.AreEqual("Bar", toTest[2]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListRemove_Sync_Test()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>(isSynchronized: true) { "Foo", "Bar" };
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform a remove operation. We'll undo this later on.
			toTest.Remove("Foo");
			Assert.AreEqual(1, toTest.Count);
			Assert.AreEqual("Bar", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			toTest.RemoveAt(1);
			Assert.AreEqual(1, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListSetItem_Sync_Test()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>(isSynchronized: true) { "Foo", "Bar" };
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// perform a set index operation. We'll undo this later on.
			toTest[0] = "Blah";
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Blah", toTest[0]);

			// undo operation.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			// use a library extension method to swap two items. We want to roll back the swap call completely, so the SwapValues call is seen
			// as an atomic action. We therefore have to create a command to make it undoable as an atomic action. It doesn't have an undo action,
			// it relies on the actions it executes by itself to undo.
			CQManager.EnqueueAndRunCommand(new Command<string>(() => toTest.SwapValues(0, 1)));
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Bar", toTest[0]);
			Assert.AreEqual("Foo", toTest[1]);

			// undo operation. This is undoing the call to SwapValues, which by undoing that, will undo all the actions SwapValues took, i.e. setting 2 items at
			// 2 indexes.
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, toTest.Count);
			Assert.AreEqual("Foo", toTest[0]);
			Assert.AreEqual("Bar", toTest[1]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedListSortWithUndo_Sync_Test()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			CommandifiedList<string> toTest = new CommandifiedList<string>(isSynchronized: true) { "aaa", "aab", "aba", "baa" };

			// use command to sort the list, so it's undoable.
			CQManager.EnqueueAndRunCommand(new Command<string>(() => toTest.Sort(SortAlgorithm.ShellSort, SortDirection.Descending)));
			Assert.AreEqual("baa", toTest[0]);
			Assert.AreEqual("aba", toTest[1]);
			Assert.AreEqual("aab", toTest[2]);
			Assert.AreEqual("aaa", toTest[3]);

			// undo the sort
			CQManager.UndoLastCommand();
			Assert.AreEqual("aaa", toTest[0]);
			Assert.AreEqual("aab", toTest[1]);
			Assert.AreEqual("aba", toTest[2]);
			Assert.AreEqual("baa", toTest[3]);

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedGraphAddRemoveVertexWithUndoTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			DirectedGraph<int, DirectedEdge<int>> graph = new DirectedGraph<int, DirectedEdge<int>>(true);
			graph.Add(42);
			Assert.IsTrue(graph.Contains(42));
			CQManager.UndoLastCommand();
			Assert.IsFalse(graph.Contains(42));
			Assert.AreEqual(0, graph.VertexCount);

			graph.Add(42);
			graph.Add(13);
			graph.Add(10);
			Assert.IsTrue(graph.Contains(42));
			Assert.IsTrue(graph.Contains(13));
			Assert.IsTrue(graph.Contains(10));

			graph.Add(new DirectedEdge<int>(42, 13));	// 42 -> 13
			graph.Add(new DirectedEdge<int>(42, 10));	// 42 -> 10
			Assert.AreEqual(2, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 13));
			Assert.IsTrue(graph.ContainsEdge(42, 10));

			graph.Remove(42);
			Assert.IsFalse(graph.Contains(42));
			Assert.AreEqual(0, graph.EdgeCount);
			Assert.AreEqual(2, graph.VertexCount);

			// undo removal of 42. This should re-add 42, but also re-add the edges
			CQManager.UndoLastCommand();
			Assert.AreEqual(2, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 13));
			Assert.IsTrue(graph.ContainsEdge(42, 10));
			Assert.IsTrue(graph.Contains(42));

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CommandifiedGraphAddRemoveGraphAndEdgesWithUndoTest()
		{
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			DirectedGraph<int, DirectedEdge<int>> graph = new DirectedGraph<int, DirectedEdge<int>>(true);
			graph.Add(42);
			graph.Add(13);
			graph.Add(10);
			graph.Add(new DirectedEdge<int>(42, 13));	// 42 -> 13
			graph.Add(new DirectedEdge<int>(42, 10));	// 42 -> 10
			Assert.IsTrue(graph.Contains(42));
			Assert.IsTrue(graph.Contains(13));
			Assert.IsTrue(graph.Contains(10));
			Assert.AreEqual(2, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 13));
			Assert.IsTrue(graph.ContainsEdge(42, 10));

			// create graph to add to this graph. Doesn't have to be a commandified graph, as we're not rolling back the actions on that graph. 
			DirectedGraph<int, DirectedEdge<int>> graphToAdd = new DirectedGraph<int, DirectedEdge<int>>();
			graphToAdd.Add(1);
			graphToAdd.Add(2);
			graphToAdd.Add(3);
			graphToAdd.Add(new DirectedEdge<int>(1, 2));	// 1 -> 2
			graphToAdd.Add(new DirectedEdge<int>(1, 3));	// 1 -> 3
			graphToAdd.Add(new DirectedEdge<int>(2, 3));	// 2 -> 3
			Assert.AreEqual(3, graphToAdd.VertexCount);
			Assert.AreEqual(3, graphToAdd.EdgeCount);

			// add this graph to the main graph. This is an undoable action.
			graph.Add(graphToAdd);
			Assert.AreEqual(6, graph.VertexCount);
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsTrue(graph.Contains(1));
			Assert.IsTrue(graph.Contains(2));
			Assert.IsTrue(graph.Contains(3));
			Assert.IsTrue(graph.ContainsEdge(1, 2));
			Assert.IsTrue(graph.ContainsEdge(1, 3));
			Assert.IsTrue(graph.ContainsEdge(2, 3));

			// undo add
			CQManager.UndoLastCommand();
			Assert.AreEqual(3, graph.VertexCount);
			Assert.AreEqual(2, graph.EdgeCount);
			Assert.IsFalse(graph.Contains(1));
			Assert.IsFalse(graph.Contains(2));
			Assert.IsFalse(graph.Contains(3));
			Assert.IsFalse(graph.ContainsEdge(1, 2));
			Assert.IsFalse(graph.ContainsEdge(1, 3));
			Assert.IsFalse(graph.ContainsEdge(2, 3));

			// redo
			CQManager.RedoLastCommand();
			Assert.AreEqual(6, graph.VertexCount);
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsTrue(graph.Contains(1));
			Assert.IsTrue(graph.Contains(2));
			Assert.IsTrue(graph.Contains(3));
			Assert.IsTrue(graph.ContainsEdge(1, 2));
			Assert.IsTrue(graph.ContainsEdge(1, 3));
			Assert.IsTrue(graph.ContainsEdge(2, 3));

			// remove the graph we added
			graph.Remove(graphToAdd);
			Assert.AreEqual(3, graph.VertexCount);
			Assert.AreEqual(2, graph.EdgeCount);
			Assert.IsFalse(graph.Contains(1));
			Assert.IsFalse(graph.Contains(2));
			Assert.IsFalse(graph.Contains(3));
			Assert.IsFalse(graph.ContainsEdge(1, 2));
			Assert.IsFalse(graph.ContainsEdge(1, 3));
			Assert.IsFalse(graph.ContainsEdge(2, 3));

			CQManager.UndoLastCommand();
			Assert.AreEqual(6, graph.VertexCount);
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsTrue(graph.Contains(1));
			Assert.IsTrue(graph.Contains(2));
			Assert.IsTrue(graph.Contains(3));
			Assert.IsTrue(graph.ContainsEdge(1, 2));
			Assert.IsTrue(graph.ContainsEdge(1, 3));
			Assert.IsTrue(graph.ContainsEdge(2, 3));

			DirectedEdge<int> newEdge = new DirectedEdge<int>(42, 1);		// 42 -> 1
			graph.Add(newEdge);		
			Assert.AreEqual(6, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 1));
			Assert.IsFalse(graph.ContainsEdge(1, 42));

			CQManager.UndoLastCommand();
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsFalse(graph.ContainsEdge(42, 1));

			CQManager.RedoLastCommand();
			Assert.AreEqual(6, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 1));

			graph.Remove(newEdge);
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsFalse(graph.ContainsEdge(42, 1));

			CQManager.UndoLastCommand();
			Assert.AreEqual(6, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 1));

			graph.Disconnect(42, 1, false);
			Assert.AreEqual(5, graph.EdgeCount);
			Assert.IsFalse(graph.ContainsEdge(42, 1));

			CQManager.UndoLastCommand();
			Assert.AreEqual(6, graph.EdgeCount);
			Assert.IsTrue(graph.ContainsEdge(42, 1));

			CQManager.ActivateCommandQueueStack(Guid.Empty);
		}


		[Test]
		public void CancelableEventsOnCommandifiedListTests()
		{
			CommandifiedList<int> list = new CommandifiedList<int>() { 1, 2, 3 };
			EventHandler<CancelableListModificationEventArgs<int>> cancelableHandler = delegate(object sender, CancelableListModificationEventArgs<int> e) { e.Cancel = true; };
			list.ElementAdding += cancelableHandler;
			list.ElementRemoving += cancelableHandler;
			list.ListClearing += cancelableHandler;
			// reset command queue manager, so it will only undo commands issued after this reset
			CommandQueueManagerSingleton.GetInstance().ResetActiveCommandQueue();

			list.Add(4);
			Assert.AreEqual(3, list.Count);
			Assert.IsFalse(list.Contains(4));
			// undo shouldn't have any effect
			CommandQueueManagerSingleton.GetInstance().UndoLastCommand();
			Assert.AreEqual(3, list.Count);
			Assert.IsFalse(list.Contains(4));

			list.Clear();
			Assert.AreEqual(3, list.Count);
			// undo shouldn't have any effect
			CommandQueueManagerSingleton.GetInstance().UndoLastCommand();
			Assert.AreEqual(3, list.Count);

			list.Remove(1);
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.Contains(1));

			// undo shouldn't have any effect
			CommandQueueManagerSingleton.GetInstance().UndoLastCommand();
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.Contains(1));

			list.ElementAdding -= cancelableHandler;
			list.ElementRemoving -= cancelableHandler;
			list.ListClearing -= cancelableHandler;

			list.Add(4);
			Assert.AreEqual(4, list.Count);
			Assert.IsTrue(list.Contains(4));

			list.Remove(1);
			Assert.AreEqual(3, list.Count);
			Assert.IsFalse(list.Contains(1));

			list.Clear();
			Assert.AreEqual(0, list.Count);
		}


		[Test]
		public void MoveItemInCommandifiedListTest()
		{
			CommandifiedList<int> list = new CommandifiedList<int> { 1, 2, 3, 4 };
			int oldIndexInEvent = -1;
			int newIndexInEvent = -1;
			bool eventRaised = false;
			list.ListChanged += (sender, e) =>
			                    	{
										eventRaised = true;
										oldIndexInEvent = e.OldIndex;
										newIndexInEvent = e.NewIndex;
			                    	};

			// move item at position 0 to position 1
			list.MoveElement(0, 1);
			Assert.IsTrue(eventRaised);
			Assert.AreEqual(0, oldIndexInEvent);
			Assert.AreEqual(1, newIndexInEvent);
			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(1, list[1]);
		}


		#region properties
		/// <summary>
		/// Gets the CQ manager.
		/// </summary>
		private static CommandQueueManager CQManager
		{ 
			get { return CommandQueueManagerSingleton.GetInstance(); }
		}
		#endregion
	}


	/// <summary>
	/// Simple helper class which produces commands to set inner data. This is all wrapped inside CommandifiedMember. 
	/// </summary>
	public class HelperClass
	{
		private enum HelperChangeType
		{
			Name
		}

		private CommandifiedMember<string, HelperChangeType> _name;

		public HelperClass()
		{
			_name = new CommandifiedMember<string, HelperChangeType>("Name", HelperChangeType.Name, string.Empty);
		}

		public string Name
		{
			get { return _name.MemberValue; }
			set { _name.MemberValue = value; }
		}
	}



	public class BindableHelperClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private enum HelperChangeType
		{
			Name
		}

		private readonly CommandifiedMember<string, HelperChangeType> _name;

		public BindableHelperClass()
		{
			_name = new CommandifiedMember<string, HelperChangeType>("Name", HelperChangeType.Name, string.Empty);
			_name.ValueChanged += new EventHandler<MemberChangedEventArgs<HelperChangeType, string>>(_name_ValueChanged);
		}

		private void OnPropertyChanged(string propertyName)
		{
			if(this.PropertyChanged!=null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void _name_ValueChanged(object sender, 
				MemberChangedEventArgs<HelperChangeType, string> e)
		{
			switch(e.TypeOfChange)
			{
				case HelperChangeType.Name:
					OnPropertyChanged("Name");
					break;
			}
		}

		public string Name
		{
			get { return _name.MemberValue; }
			set { _name.MemberValue = value; }
		}
	}

}
