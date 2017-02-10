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
//		- Jeroen van den Bos [JB]
//		- Frans Bouma [FB]
//		- Walaa Atef [WA]
//////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using NUnit.Framework;
using SD.Tools.Algorithmia.Graphs;
using System.Collections.Generic;
using System.Threading;
using SD.Tools.Algorithmia.Commands;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.Graphs.Algorithms;

namespace SD.Tools.Algorithmia.Tests
{
	/// <summary>
	/// Class with unit tests for the graphbase based classes and algorithms. All graph instances are synchronized and will use locking internally. 
	/// </summary>
	[TestFixture]
	public class GraphTests_Synchronized
	{
		public const int RandomTests = 50;
		public const int MaxDirectedGraphSize = 50;

		private Random _random;
		private int _seed;

		#region Private classes
		/// <summary>
		/// Simple graph class which is used to test isconnected code with Edge(Of T) instances
		/// </summary>
		/// <typeparam name="TVertex"></typeparam>
		/// <typeparam name="TEdge"></typeparam>
		public class SimpleGraph<TVertex, TEdge> : GraphBase<TVertex, TEdge>
			where TEdge : class, IEdge<TVertex>
		{
			public SimpleGraph(bool isDirected)
				: base(isDirected, isCommandified: false, isSynchronized:true)
			{
			}
		}

		/// <summary>
		/// simple tester class to check what the order is the crawler traverses the graph
		/// </summary>
		/// <typeparam name="TVertex"></typeparam>
		/// <typeparam name="TEdge"></typeparam>
		public class DepthFirstSearchTester<TVertex, TEdge> : DepthFirstSearchCrawler<TVertex, TEdge>
			where TEdge : class, IEdge<TVertex>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="DepthFirstSearchLogger&lt;TVertex, TEdge&gt;"/> class.
			/// </summary>
			/// <param name="toCrawl">To crawl.</param>
			public DepthFirstSearchTester(GraphBase<TVertex, TEdge> toCrawl)
				: base(toCrawl, false)
			{
				this.VerticesLoggedInOnVisited = new List<TVertex>();
				this.VerticesLoggedInOnVisiting = new List<TVertex>();
			}


			/// <summary>
			/// Called when the vertexVisited was visited over the edge edgeUsed. This method is called right after all vertices related to vertexVisited were visited.
			/// </summary>
			/// <param name="vertexVisited">The vertex visited right before this method.</param>
			/// <param name="edges">The edges usable to visit vertexVisited. Can be null, in which case the vertex was visited without using an edge (which would mean
			/// the vertex is a tree root, or the start vertex.)</param>
			protected override void OnVisited(TVertex vertexVisited, HashSet<TEdge> edges)
			{
				this.VerticesLoggedInOnVisited.Add(vertexVisited);
			}


			/// <summary>
			/// Called when the vertexToVisit is about to be visited over the edge edgeUsed. This method is called right before all vertices related to vertexToVisit
			/// are visited.
			/// </summary>
			/// <param name="vertexVisited">The vertex currently visited</param>
			/// <param name="edges">The edges usable to visit vertexToVisit. Can be null, in which case the vertex was visited without using an edge (which would mean
			/// the vertex is a tree root, or the start vertex.)</param>
			protected override void OnVisiting(TVertex vertexVisited, HashSet<TEdge> edges)
			{
				this.VerticesLoggedInOnVisiting.Add(vertexVisited);
			}


			/// <summary>
			/// Starts this instance.
			/// </summary>
			public void Start()
			{
				this.Crawl();
			}


			/// <summary>
			/// Starts the specified start vertex.
			/// </summary>
			/// <param name="startVertex">The start vertex.</param>
			public void Start(TVertex startVertex)
			{
				this.Crawl(startVertex);
			}


			#region Class Property Declarations
			/// <summary>
			/// Gets or sets the vertices logged in OnVisiting.
			/// </summary>
			public List<TVertex> VerticesLoggedInOnVisiting { get; private set; }

			/// <summary>
			/// Gets or sets the vertices logged in OnVisited.
			/// </summary>
			public List<TVertex> VerticesLoggedInOnVisited { get; private set; }
			#endregion
		}
		#endregion

		/// <summary>
		/// Called before any other methods when this TestFixture is to be used.
		/// </summary>
		/// <remarks>
		/// Sets up the random number generator once to prevent the same seed from being obtained from the timer when
		/// succeeding random tests finish quicker than the timer updates. Also saves the used seed so that errors may
		/// be reproduced.
		/// 
		/// Perhaps a constructor should be used instead of the SetUp-method, since this setup is only relevant for a
		/// single test.
		/// </remarks>
		[SetUp]
		public void SetUp()
		{
			// Only take the int part because that's what the Random constructor needs.
			_seed = (int)DateTime.Now.Ticks;
			_random = new Random(_seed);
		}


		[Test]
		public void MultiThreadedSyncedGraphAccessTest()
		{
			// set up our session.
			Guid sessionId = Guid.NewGuid();
			CQManager.ActivateCommandQueueStack(sessionId);

			var toTest = new DirectedGraph<int, DirectedEdge<int>>((a, b) => new DirectedEdge<int>(a, b), isCommandified: true, isSynchronized: true);
			for(int i = 0; i < 30; i++)
			{
				toTest.Add(new DirectedEdge<int>(i, i+1));
				toTest.Add(new DirectedEdge<int>(i+3, i));
			}

			var waitHandles = new WaitHandle[] { new AutoResetEvent(false), new AutoResetEvent(false) };
			Exception caughtException = null;
			Console.WriteLine("Starting threads...");
			var threadA = new Thread(() => MultiThreadedSyncedGraphAccessTest_ThreadA(toTest, waitHandles[0], (e) => caughtException = e));
			var threadB = new Thread(() => MultiThreadedSyncedGraphAccessTest_ThreadB(toTest, waitHandles[1], (e) => caughtException = e));
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


		private void MultiThreadedSyncedGraphAccessTest_ThreadA(DirectedGraph<int, DirectedEdge<int>> graph, WaitHandle waitHandle, Action<Exception> handler)
		{
			Console.WriteLine("Thread A started");
			try
			{
				for(int i = 0; i < 100; i++)
				{
					var vertexCountBefore = graph.VertexCount;
					var index = _random.Next(0, vertexCountBefore);
					graph.Remove(index);
					Assert.AreEqual(vertexCountBefore - 1, graph.VertexCount);
					CQManager.UndoLastCommand();
					Assert.AreEqual(vertexCountBefore, graph.VertexCount);
					Thread.Sleep(1);
					CQManager.RedoLastCommand();
					Assert.AreEqual(vertexCountBefore - 1, graph.VertexCount);
					Thread.Sleep(2);
					CQManager.UndoLastCommand();
					Assert.AreEqual(vertexCountBefore, graph.VertexCount);
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


		private void MultiThreadedSyncedGraphAccessTest_ThreadB(DirectedGraph<int, DirectedEdge<int>> graph, WaitHandle waitHandle, Action<Exception> handler)
		{
			Console.WriteLine("Thread B started");
			try
			{
				for(int i = 0; i < 100; i++)
				{
					bool lockTaken = false;
					object syncRoot = graph.SyncRoot;
					try
					{
						if(graph.IsSynchronized)
						{
							Monitor.Enter(syncRoot);
							lockTaken = true;
						}
						int countBefore = graph.VertexCount;
						int count = 0;
						foreach(var v in graph.Vertices)
						{
							count++;
						}
						Assert.AreEqual(countBefore, count, "Count after enumeration differs from initial count of vertices set");
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

		/// <summary>
		/// Generates RandomTests (default 50) random graphs containing up to MaxDirectedGraphSize (default 50) edges. Next
		/// the transitive closure is created using the TransitiveClosure() method and checked against the Compose()
		/// method. This check is based on the fact that if you compose a graph that is a transitive closure with itself,
		/// no new edges are added.
		/// </summary>
		[Test]
		public void RandomTransitiveClosureAndCompositionTest()
		{
			for(int i = 0; i < RandomTests; i++)
			{
				DirectedGraph<int, DirectedEdge<int>> graph = GenerateRandomIntegerDirectedGraph(MaxDirectedGraphSize);
				DirectedGraph<int, DirectedEdge<int>> composed = DirectedGraph<int, DirectedEdge<int>>.Compose(graph, graph, (a, b) => new DirectedEdge<int>(a, b));
				DirectedGraph<int, DirectedEdge<int>> closure = graph.TransitiveClosure();
				foreach(Edge<int> edge in composed.Edges)
				{
					Assert.IsTrue(closure.ContainsEdge(edge.StartVertex, edge.EndVertex), "Failure on run " + i.ToString() + " based on seed " + _seed.ToString() + ".");
				}
			}
		}


		/// <summary>
		/// Generates a random integer graph of maxSize size. Size here denotes the amount of edges, nodes may be up to
		/// twice that amount.
		/// </summary>
		/// <param name="maxSize">The maximum amount of edges the graph contains.</param>
		/// <returns>The randomly generated graph.</returns>
		private DirectedGraph<int, DirectedEdge<int>> GenerateRandomIntegerDirectedGraph(int maxSize)
		{
			int size = _random.Next(maxSize);
			// create an integer graph and set the edge producer func
			var graph = new DirectedGraph<int, DirectedEdge<int>>((a, b) => new DirectedEdge<int>(a, b), isCommandified: false, isSynchronized:true);
			for(int i = 0; i < size;)
			{
				int startNode = _random.Next(maxSize);
				int endNode = _random.Next(maxSize);
				if(!graph.ContainsEdge(startNode, endNode))
				{
					graph.Add(startNode);
					graph.Add(endNode);
					graph.Add(new DirectedEdge<int>(startNode, endNode));
					i++;
				}
			}
			return graph;
		}


		/// <summary>
		/// Checks whether the TransitiveClosure() method works correctly on a small hand-written graph.
		/// </summary>
		/// <remarks>
		/// Small hand-written test to make sure the random testing doesn't mess up and fools you into thinking
		/// everything works when it doesn't :)
		/// </remarks>
		[Test]
		public void TransitiveClosureBasicTest()
		{
			// create an integer graph and set the edge producer func
			var g = new DirectedGraph<int, DirectedEdge<int>>((a, b) => new DirectedEdge<int>(a, b), isCommandified: false, isSynchronized: true);
			DirectedEdge<int> toAdd = new DirectedEdge<int>(1, 2);
			g.Add(toAdd);
			Assert.IsTrue(g.Contains(toAdd));
			g.Add(new DirectedEdge<int>(2, 3));
			g.Add(new DirectedEdge<int>(3, 4));
			DirectedGraph<int, DirectedEdge<int>> h = g.TransitiveClosure();
			Assert.IsTrue(h.VertexCount == 4);
			Assert.IsTrue(h.EdgeCount == 6);
			Assert.IsTrue(h.ContainsEdge(1, 2));
			Assert.IsTrue(h.ContainsEdge(2, 3));
			Assert.IsTrue(h.ContainsEdge(3, 4));
			Assert.IsTrue(h.ContainsEdge(1, 3));
			Assert.IsTrue(h.ContainsEdge(1, 4));
			Assert.IsTrue(h.ContainsEdge(2, 4));
		}


		/// <summary>
		/// Checks whether the TransitiveClosure() method works correctly on a small hand-written cyclic graph.
		/// </summary>
		/// <remarks>
		/// Small hand-written test to make sure the random testing doesn't mess up and fools you into thinking
		/// everything works when it doesn't :)
		/// </remarks>
		[Test]
		public void TransitiveClosureCyclicTest()
		{
			var g = new DirectedGraph<int, DirectedEdge<int>>((a, b) => new DirectedEdge<int>(a, b), isCommandified: false, isSynchronized: true);
			g.Add(new DirectedEdge<int>(1, 2));
			g.Add(new DirectedEdge<int>(2, 3));
			g.Add(new DirectedEdge<int>(3, 1));
			DirectedGraph<int, DirectedEdge<int>> h = g.TransitiveClosure();
			Assert.AreEqual(3, h.VertexCount);
			Assert.AreEqual(9, h.EdgeCount);
			Assert.IsTrue(h.ContainsEdge(1, 2));
			Assert.IsTrue(h.ContainsEdge(2, 3));
			Assert.IsTrue(h.ContainsEdge(3, 1));
			Assert.IsTrue(h.ContainsEdge(1, 3));
			Assert.IsTrue(h.ContainsEdge(1, 1));
			Assert.IsTrue(h.ContainsEdge(2, 1));
			Assert.IsTrue(h.ContainsEdge(2, 2));
			Assert.IsTrue(h.ContainsEdge(3, 2));
			Assert.IsTrue(h.ContainsEdge(3, 3));
		}


		/// <summary>
		/// Tests the depth first search crawler logic with the example graph from Sedgewick's Algorithms in C example graph (Chapter 29, figure 29.1 and further)
		/// </summary>
		[Test]
		public void BasicDepthFirstSearchCrawlerTest()
		{
			// create example graph, non-directed.
			var graph = new NonDirectedGraph<string, NonDirectedEdge<string>>(isCommandified: false, isSynchronized:true);
			// create edges. simply use string literals, which will point to the same vertices anyway.
			graph.Add(new NonDirectedEdge<string>("A", "B"));
			graph.Add(new NonDirectedEdge<string>("A", "C"));
			graph.Add(new NonDirectedEdge<string>("A", "G"));
			graph.Add(new NonDirectedEdge<string>("A", "F"));
			graph.Add(new NonDirectedEdge<string>("F", "D"));
			graph.Add(new NonDirectedEdge<string>("F", "E"));
			graph.Add(new NonDirectedEdge<string>("D", "E"));
			graph.Add(new NonDirectedEdge<string>("E", "G"));
			graph.Add(new NonDirectedEdge<string>("H", "I"));
			graph.Add(new NonDirectedEdge<string>("J", "K"));
			graph.Add(new NonDirectedEdge<string>("J", "L"));
			graph.Add(new NonDirectedEdge<string>("J", "M"));
			graph.Add(new NonDirectedEdge<string>("L", "M"));

			DepthFirstSearchTester<string, NonDirectedEdge<string>> dfs = new DepthFirstSearchTester<string, NonDirectedEdge<string>>(graph);
			dfs.Start();
			Assert.AreEqual(13, dfs.VerticesLoggedInOnVisiting.Count);
			Assert.AreEqual(13, dfs.VerticesLoggedInOnVisited.Count);

			// visual confirmation logging code
			Console.Write("Vertices logged in order by OnVisiting:\n\t");
			foreach(string v in dfs.VerticesLoggedInOnVisiting)
			{
				Console.Write(v + " ");
			}
			Console.Write("\n\nVertices logged in order by OnVisited:\n\t");
			foreach(string v in dfs.VerticesLoggedInOnVisited)
			{
				Console.Write(v + " ");
			}

			// start in another tree in the graph
			dfs.VerticesLoggedInOnVisited.Clear();
			dfs.VerticesLoggedInOnVisiting.Clear();
			dfs.Start("L");
			Assert.AreEqual(13, dfs.VerticesLoggedInOnVisiting.Count);
			Assert.AreEqual(13, dfs.VerticesLoggedInOnVisited.Count);

			Console.Write("\n\nVertices logged in order by OnVisiting:\n\t");
			foreach(string v in dfs.VerticesLoggedInOnVisiting)
			{
				Console.Write(v + " ");
			}
			Console.Write("\n\nVertices logged in order by OnVisited:\n\t");
			foreach(string v in dfs.VerticesLoggedInOnVisited)
			{
				Console.Write(v + " ");
			}
		}


		[Test]
		public void TopologicalSorterOnDirectedGraphWithCycle()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E
			graph.Add(new DirectedEdge<string>("E", "A"));  // E->A, creates cycle A, B/C, D, E, A
			graph.Add(new DirectedEdge<string>("H", "G"));  // H->G
			graph.Add(new DirectedEdge<string>("G", "F"));  // G->F

			TopologicalSorter<string, DirectedEdge<string>> sorter = new TopologicalSorter<string, DirectedEdge<string>>(graph);
			Assert.That(() => sorter.Sort(), Throws.TypeOf<InvalidOperationException>());
		}


		[Test]
		public void TopologicalSorterOnDirectedAcyclicGraphWhereDirectionMeansDependency()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E
			graph.Add(new DirectedEdge<string>("H", "G"));  // H->G
			graph.Add(new DirectedEdge<string>("G", "F"));  // G->F

			TopologicalSorter<string, DirectedEdge<string>> sorter = new TopologicalSorter<string, DirectedEdge<string>>(graph);
			sorter.Sort();
			List<string> expectedResults = new List<string>() { "E", "D", "B", "C", "A", "F", "G", "H" };
			for(int i = 0; i < sorter.SortResults.Count; i++)
			{
				Assert.AreEqual(expectedResults[i], sorter.SortResults[i]);
			}
		}

		[Test]
		public void TopologicalSorterOnDirectedAcyclicGraphWhereDirectionMeansOrder()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E
			graph.Add(new DirectedEdge<string>("H", "G"));  // H->G
			graph.Add(new DirectedEdge<string>("G", "F"));  // G->F

			TopologicalSorter<string, DirectedEdge<string>> sorter = new TopologicalSorter<string, DirectedEdge<string>>(graph, true);
			sorter.Sort();
			List<string> expectedResults = new List<string>() { "H", "G", "F", "A", "C", "B", "D", "E" };
			for(int i = 0; i < sorter.SortResults.Count; i++)
			{
				Assert.AreEqual(expectedResults[i], sorter.SortResults[i]);
			}
		}

		[Test]
		public void OrphanedVerticesRetrievalTest()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E
			graph.Add(new DirectedEdge<string>("H", "G"));  // H->G
			DirectedEdge<string> toAdd = new DirectedEdge<string>("G", "F");
			graph.Add(toAdd);   // G->F
			graph.Remove(toAdd);
			HashSet<string> orphanedVertices = graph.GetOrphanedVertices();
			Assert.AreEqual(1, orphanedVertices.Count);
			Assert.IsTrue(orphanedVertices.Contains("F"));
		}


		[Test]
		public void RemoveOrphanedVerticesWhenEdgeIsRemovedTest()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E
			graph.Add(new DirectedEdge<string>("H", "G"));  // H->G
			DirectedEdge<string> toAdd = new DirectedEdge<string>("G", "F");
			graph.Add(toAdd);   // G->F
			graph.RemoveOrphanedVerticesOnEdgeRemoval = true;
			graph.Remove(toAdd);
			HashSet<string> orphanedVertices = graph.GetOrphanedVertices();
			Assert.AreEqual(0, orphanedVertices.Count);
			Assert.IsFalse(graph.Contains("F"));
		}

		/// <summary>
		/// Test the logic of the IsConnected method.
		/// </summary>
		[Test]
		public void IsConnectedTest()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E

			Assert.IsTrue(graph.IsConnected());

			// Add an un-connected edge.
			DirectedEdge<string> toAdd = new DirectedEdge<string>("G", "F");
			graph.Add(toAdd);

			Assert.IsFalse(graph.IsConnected());

			// Add a directed edge from F to A.
			toAdd = new DirectedEdge<string>("F", "A");
			graph.Add(toAdd);

			Assert.IsTrue(graph.IsConnected());
		}


		[Test]
		public void IsConnected2Test()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add("C");     // lonely, non-connected vertex

			Assert.IsFalse(graph.IsConnected());

			// Add a connected edge.
			DirectedEdge<string> toAdd = new DirectedEdge<string>("C", "B");
			graph.Add(toAdd);

			Assert.IsTrue(graph.IsConnected());
		}



		/// <summary>
		/// Tests the logic of the IsConnected method with a simple graph which uses Edge(Of T) instances to see if the logic internally can deal 
		/// with graphs which are non-directed but don't use NonDirectedEdge(Of T). 
		/// </summary>
		[Test]
		public void IsConnectedTestUsingSimpleNonDirectedGraph()
		{
			SimpleGraph<string, Edge<string>> graph = new SimpleGraph<string, Edge<string>>(false);
			graph.Add(new Edge<string>("A", "B", false));   // A->B
			graph.Add(new Edge<string>("A", "C", false));   // A->C
			graph.Add(new Edge<string>("B", "D", false));   // B->D
			graph.Add(new Edge<string>("C", "D", false));   // C->D
			graph.Add(new Edge<string>("D", "E", false));   // D->E

			Assert.IsTrue(graph.IsConnected());

			// Add an un-connected edge.
			Edge<string> toAdd = new Edge<string>("G", "F", false);
			graph.Add(toAdd);

			Assert.IsFalse(graph.IsConnected());

			// Add a directed edge from F to A.
			toAdd = new Edge<string>("F", "A", false);
			graph.Add(toAdd);

			Assert.IsTrue(graph.IsConnected());
		}


		[Test]
		public void FindDisconnectedSubGraphsTest()
		{
			SimpleGraph<string, Edge<string>> graph = new SimpleGraph<string, Edge<string>>(false);
			graph.Add(new Edge<string>("A", "B", false));   // A->B
			graph.Add(new Edge<string>("A", "C", false));   // A->C
			graph.Add(new Edge<string>("B", "D", false));   // B->D
			graph.Add(new Edge<string>("C", "D", false));   // C->D
			graph.Add(new Edge<string>("D", "E", false));   // D->E
			graph.Add(new Edge<string>("G", "F", false));   // G->F, different subgraph

			Assert.IsFalse(graph.IsConnected());

			DisconnectedGraphsFinder<string, Edge<string>> finder = new DisconnectedGraphsFinder<string, Edge<string>>(
							() => new SubGraphView<string, Edge<string>>(graph), graph);

			finder.FindDisconnectedGraphs();
			Assert.AreEqual(2, finder.FoundDisconnectedGraphs.Count);
			Assert.AreEqual(5, finder.FoundDisconnectedGraphs[0].Vertices.Count());
			Assert.AreEqual(2, finder.FoundDisconnectedGraphs[1].Vertices.Count());
			Assert.AreEqual(5, finder.FoundDisconnectedGraphs[0].Edges.Count());
			Assert.AreEqual(1, finder.FoundDisconnectedGraphs[1].Edges.Count());

			finder.FindDisconnectedGraphs("A", true);
			Assert.AreEqual(1, finder.FoundDisconnectedGraphs.Count);
			Assert.AreEqual(5, finder.FoundDisconnectedGraphs[0].Vertices.Count());
			Assert.AreEqual(5, finder.FoundDisconnectedGraphs[0].Edges.Count());
		}


		/// <summary>
		/// Test the GetNonDirectedCopy method.
		/// </summary>
		[Test]
		public void AsNonDirectedGraphTest()
		{
			DirectedGraph<string, DirectedEdge<string>> graph = new DirectedGraph<string, DirectedEdge<string>>(isCommandified: false, isSynchronized: true);
			graph.Add(new DirectedEdge<string>("A", "B"));  // A->B
			graph.Add(new DirectedEdge<string>("A", "C"));  // A->C
			graph.Add(new DirectedEdge<string>("B", "D"));  // B->D
			graph.Add(new DirectedEdge<string>("C", "D"));  // C->D
			graph.Add(new DirectedEdge<string>("D", "E"));  // D->E

			Assert.IsTrue(graph.IsDirected);
			Assert.IsTrue(graph.EdgeCount == 5);

			NonDirectedGraph<string, NonDirectedEdge<string>> nonDirectedGraph = (NonDirectedGraph<string, NonDirectedEdge<string>>)graph.GetAsNonDirectedCopy();

			Assert.IsFalse(nonDirectedGraph.IsDirected);

			Assert.IsTrue(nonDirectedGraph.ContainsEdge("A", "B"));
			Assert.IsTrue(nonDirectedGraph.ContainsEdge("B", "A"));

			Assert.IsTrue(nonDirectedGraph.ContainsEdge("A", "C"));
			Assert.IsTrue(nonDirectedGraph.ContainsEdge("C", "A"));

			Assert.IsTrue(nonDirectedGraph.ContainsEdge("B", "D"));
			Assert.IsTrue(nonDirectedGraph.ContainsEdge("D", "B"));

			Assert.IsTrue(nonDirectedGraph.ContainsEdge("C", "D"));
			Assert.IsTrue(nonDirectedGraph.ContainsEdge("D", "C"));

			Assert.IsTrue(nonDirectedGraph.ContainsEdge("D", "E"));
			Assert.IsTrue(nonDirectedGraph.ContainsEdge("E", "D"));

			foreach(Edge<string> edge in nonDirectedGraph.Edges)
			{
				Console.Write("\n\nEdge Start index: " + edge.StartVertex + "\tEdge End index:" + edge.EndVertex + "\n\t");

				Assert.IsFalse(edge.IsDirected);
			}

			//Assert.IsTrue(nonDirectedGraph.EdgeCount == 10);
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
}
