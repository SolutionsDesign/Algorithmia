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
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.Graphs.Algorithms
{
	/// <summary>
	/// Finder which produces per disconnected subgraph a SubGraphView instance with all the vertices and edges of the disconnected graph. 
	/// </summary>
	/// <typeparam name="TVertex">Type of the vertices in the graph to crawl</typeparam>
	/// <typeparam name="TEdge">Type of the edges in the graph to crawl</typeparam>
	/// <remarks>Only use this on a non-directed graph. Using this algorithm on a directed graph will give an InvalidOperationException as it doesn't lead to 
	/// satisfying results</remarks>
	public class DisconnectedGraphsFinder<TVertex, TEdge> : RootDetector<TVertex, TEdge>
		where TEdge : class, IEdge<TVertex>
	{
		#region Class Member Declarations
		private readonly Func<SubGraphView<TVertex, TEdge>> _subGraphViewCreatorFunc;
		private List<SubGraphView<TVertex, TEdge>> _foundDisconnectedGraphs;
		private SubGraphView<TVertex, TEdge> _currentDisconnectedGraph;
		private bool _stopAfterFirstSubGraph;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="DisconnectedGraphsFinder&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="subGraphViewCreatorFunc">The sub graph view creator func, used to create new subgraphview instances.</param>
		/// <param name="toCrawl">the graph to crawl</param>
		public DisconnectedGraphsFinder(Func<SubGraphView<TVertex, TEdge>> subGraphViewCreatorFunc, GraphBase<TVertex, TEdge> toCrawl)
			: base(toCrawl)
		{
			ArgumentVerifier.CantBeNull(subGraphViewCreatorFunc, "subGraphViewCreatorFunc");
			_subGraphViewCreatorFunc = subGraphViewCreatorFunc;
		}


		/// <summary>
		/// Finds the disconnected graphs.
		/// </summary>
		public void FindDisconnectedGraphs()
		{
			InitClass(false);
			this.Crawl();
			ObtainEdges();
		}


		/// <summary>
		/// Finds the disconnected graphs.
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="onlyDisconnectedGraphOfStartVertex">if set to true, the finder will only determine the disconnected graph of the startvertex and
		/// stop after a new disconnected graph is detected</param>
		public void FindDisconnectedGraphs(TVertex startVertex, bool onlyDisconnectedGraphOfStartVertex)
		{
			InitClass(true);
			this.Crawl(startVertex);
			ObtainEdges();
		}


		/// <summary>
		/// Signal the detection of a root vertex that has been visited by the crawler.
		/// </summary>
		/// <param name="vertexVisited">The detected root vertex</param>
		/// <remarks>Only called in non-directed graphs, as root detection isn't possible with a DFS crawler in directed graphs without additional algorithm code</remarks>
		protected override void RootDetected(TVertex vertexVisited)
		{
			base.RootDetected(vertexVisited);
			if(_stopAfterFirstSubGraph && (_foundDisconnectedGraphs.Count > 0))
			{
				this.AbortCrawl = true;
			}
			else
			{
				NewDisconnectedGraphFound();
				_currentDisconnectedGraph.Add(vertexVisited);
			}
		}


		/// <summary>
		/// Called when the vertexToVisit is about to be visited over the edges specified. This method is called right before all vertices related to vertexToVisit
		/// are visited.
		/// </summary>
		/// <param name="vertexVisited">The vertex currently visited</param>
		/// <param name="edges">The edges usable to visit vertexToVisit. Can be null, in which case the vertex was visited without using an edge (which would mean
		/// the vertex is a tree root, or the start vertex.)</param>
		protected override void OnVisiting(TVertex vertexVisited, HashSet<TEdge> edges)
		{
			base.OnVisiting(vertexVisited, edges);
			_currentDisconnectedGraph.Add(vertexVisited);
			// ignore edges for now, we're obtaining the edges later on, as the DFS crawler will visit a node only once. 
		}


		/// <summary>
		/// A new Disconnected graph was found and this routine updates internal datastructures.
		/// </summary>
		private void NewDisconnectedGraphFound()
		{
			_currentDisconnectedGraph = _subGraphViewCreatorFunc();
			_foundDisconnectedGraphs.Add(_currentDisconnectedGraph);
		}


		/// <summary>
		/// Inits the class.
		/// </summary>
		/// <param name="stopAfterFirstSubGraph">if set to <see langword="true"/> [stop after first sub graph].</param>
		private void InitClass(bool stopAfterFirstSubGraph)
		{
			_foundDisconnectedGraphs = new List<SubGraphView<TVertex, TEdge>>();
			_stopAfterFirstSubGraph = stopAfterFirstSubGraph;
		}


		/// <summary>
		/// Obtains the edges for each found subgraph from the main graph. Called after a crawl has finished. 
		/// </summary>
		private void ObtainEdges()
		{
			foreach(SubGraphView<TVertex, TEdge> graphView in _foundDisconnectedGraphs)
			{
				foreach(TVertex vertex in graphView.Vertices)
				{
					foreach(TEdge edge in graphView.MainGraph.GetEdgesFromStartVertex(vertex))
					{
						graphView.Add(edge);
					}
				}
			}
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the disconnected graphs found;
		/// </summary>
		public List<SubGraphView<TVertex, TEdge>> FoundDisconnectedGraphs
		{
			get { return _foundDisconnectedGraphs; }
		}
		#endregion
	}
}
