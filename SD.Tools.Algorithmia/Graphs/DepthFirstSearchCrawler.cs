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
//		- Frans  Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.Algorithmia.GeneralDataStructures;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// Basic crawler class which crawls over all vertices in the graph it is set to crawl. For every visited vertex, it will call a visit routine, which can be
	/// overriden in derived classes. Depth first search is discussed here: http://en.wikipedia.org/wiki/Depth_first_search and it's the base for many graph known
	/// algorithms. An alternative is breadth first search, which uses a method which is non-recursive.
	/// </summary>
	/// <typeparam name="TVertex">The type of vertices</typeparam>
	/// <typeparam name="TEdge">The type of the edges in the graph</typeparam>
	/// <remarks>This class has no public methods, as it's meant to be a mechanism for algorithm implementation to consume a graph in a given order</remarks>
	public abstract class DepthFirstSearchCrawler<TVertex, TEdge>
		where TEdge : class, IEdge<TVertex>
	{
		#region Enums
		/// <summary>
		/// Enum for marking vertices along the way. For cycle detection
		/// </summary>
		private enum VertexColor
		{
			/// <summary>
			/// Vertex hasn't been visited yet
			/// </summary>
			NotVisited,
			/// <summary>
			/// Vertex is in the process of being visited. As this algorithm is recusive, if we run into a vertex which has this color, we ran into a cycle
			/// </summary>
			Visiting,
			/// <summary>
			/// Vertex was completely visited.
			/// </summary>
			Visited
		}
		#endregion

		#region Class Member Declarations
		private readonly GraphBase<TVertex, TEdge> _graphToCrawl;
		private readonly bool _detectCycles;
		private bool _abortCrawl;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="DepthFirstSearchCrawler&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graphToCrawl">The graph to crawl.</param>
		/// <param name="detectCycles">if set to true, the crawler will notify derived classes that a cycle has been detected if the graph to crawl is a 
		/// directed graph, as cycles in directed graphs influence algorithms which work with directed graphs.</param>
		protected DepthFirstSearchCrawler(GraphBase<TVertex, TEdge> graphToCrawl, bool detectCycles)
		{
			ArgumentVerifier.CantBeNull(graphToCrawl, "graphToCrawl");
			_graphToCrawl = graphToCrawl;
			_detectCycles = detectCycles;
		}


		/// <summary>
		/// Crawls the graph set as the graphToCrawl in the constructor. It picks the first vertex in the graph to start.
		/// </summary>
		protected void Crawl()
		{
			TVertex vertexToStart = GeneralUtils.PerformSyncedAction(()=>_graphToCrawl.Vertices.FirstOrDefault(), _graphToCrawl.SyncRoot, _graphToCrawl.IsSynchronized);
			if((object)vertexToStart==null)
			{
				// nothing to crawl
				return;
			}
			Crawl(vertexToStart);
		}


		/// <summary>
		/// Crawls the graph set as the graphToCrawl in the constructor, starting with the vertex specified. If the vertex isn't in the graph, the routine is a no-op
		/// </summary>
		/// <param name="vertexToStart">The vertex to start the crawl operation.</param>
		/// <remarks>vertexToStart can't be null, as a graph can't contain null vertices</remarks>
		protected void Crawl(TVertex vertexToStart)
		{
			if(((object)vertexToStart==null) || (!_graphToCrawl.Contains(vertexToStart)))
			{
				return;
			}
			var verticesProcessed = new Dictionary<TVertex, VertexColor>();
			bool firstRun = true;
			IEnumerable<TVertex> toEnumerate = _graphToCrawl.Vertices;
			if(_graphToCrawl.IsSynchronized)
			{
				// create safe copy to work with. This can be a bit of a problem regarding performance with massive graphs, but toArray is in general rather quick. 
				toEnumerate = GeneralUtils.PerformSyncedAction(()=> _graphToCrawl.Vertices.ToArray(), _graphToCrawl.SyncRoot, _graphToCrawl.IsSynchronized);
			}
			foreach(TVertex vertex in toEnumerate)
			{
				if(_abortCrawl)
				{
					break;
				}
				TVertex vertexToProcess = vertex;
				if(firstRun)
				{
					// simply use the vertexToStart. As it's being processed right away, any subsequential occurence later on will be a no-op as the vertex has
					// already been processed. 
					vertexToProcess = vertexToStart;
					firstRun = false;
				}
				if(!verticesProcessed.ContainsKey(vertexToProcess))
				{
					verticesProcessed.Add(vertexToProcess, VertexColor.NotVisited);
				}
				if(verticesProcessed[vertexToProcess] != VertexColor.NotVisited)
				{
					continue;
				}
				// we only get back to this level for tree roots if it's a non-directed graph. If it's a directed graph, we can end up here as well if we moved
				// into a path to a vertex which has no out-going edges.
				if(!_graphToCrawl.IsDirected)
				{
					RootDetected(vertexToProcess);
				}
				Visit(vertexToProcess, verticesProcessed, null);
			}
		}


		/// <summary>
		/// Signal the detection of a root vertex that has been visited by the crawler.
		/// </summary>
		/// <param name="vertex">The detected root vertex</param>
		/// <remarks>Only called in non-directed graphs, as root detection isn't possible with a DFS crawler in directed graphs without additional algorithm code</remarks>
		protected virtual void RootDetected(TVertex vertex)
		{ 
		}


		/// <summary>
		/// A cycle has been detected in a directed graph. This method is called to signal derived classes that the cycle has been detected, as the derived class
		/// ordered this class to signal it if cycles were detected. Cycle checks are only performed on directed graphs.
		/// </summary>
		/// <param name="relatedVertex">The related vertex which formed the cycle.</param>
		/// <param name="edges">The edges used to visit the related vertex.</param>
		/// <returns>true if routines should proceed deeper into the graph, false otherwise (so traversing will stop at this point and backtrack with
		/// other paths). Returning true could lead to infinite loops. Default is false</returns>
		/// <remarks>It's recommended you throw an exception to quit the operation entirely if your algorithm can't deal with cycles.</remarks>
		protected virtual bool CycleDetected(TVertex relatedVertex, HashSet<TEdge> edges)
		{
			return false;
		}


		/// <summary>
		/// Called when the vertexToVisit is about to be visited over the edges specified. This method is called right before all vertices related to vertexToVisit
		/// are visited.
		/// </summary>
		/// <param name="vertexVisited">The vertex currently visited</param>
		/// <param name="edges">The edges usable to visit vertexToVisit. Can be null, in which case the vertex was visited without using an edge (which would mean
		/// the vertex is a tree root, or the start vertex.)</param>
		protected virtual void OnVisiting(TVertex vertexVisited, HashSet<TEdge> edges)
		{
		}


		/// <summary>
		/// Called when the vertexVisited was visited over the edges specified. This method is called right after all vertices related to vertexVisited were visited.
		/// </summary>
		/// <param name="vertexVisited">The vertex visited right before this method.</param>
		/// <param name="edges">The edges usable to visit vertexVisited. Can be null, in which case the vertex was visited without using an edge (which would mean
		/// the vertex is a tree root, or the start vertex.)</param>
		protected virtual void OnVisited(TVertex vertexVisited, HashSet<TEdge> edges)
		{
		}
		

		/// <summary>
		/// The actual crawler routine. It visits the specified vertex over the edge used. If the vertex is visitable over multiple edges from the last visited vertex,
		/// the routine is called multiple times. 
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <param name="verticesProcessed">The vertices processed.</param>
		/// <param name="edges">The edges usable to visit the vertex.</param>
		private void Visit(TVertex vertex, IDictionary<TVertex, VertexColor> verticesProcessed, HashSet<TEdge> edges)
		{
			if(_abortCrawl)
			{
				return;
			}
			if(!verticesProcessed.ContainsKey(vertex))
			{
				verticesProcessed.Add(vertex, VertexColor.NotVisited);
			}
			if(verticesProcessed[vertex]==VertexColor.Visiting)
			{
				// check if we ran into a cycle, in the case of a directed graph
				if(_graphToCrawl.IsDirected)
				{
					if(_detectCycles)
					{
						// cycle detected, as we're reaching a vertex (via recursion) which is still in progress. It depends on the CycleDetected routine if we may
						// proceed.
						bool continueTraverse = CycleDetected(vertex, edges);
						if(!continueTraverse)
						{
							// simply break off traversal.
							return;
						}
					}
				}
				else
				{
					// as the crawler visits vertices over all the edges at once, in an undirected graph there is no such situation where the visited vertex
					// has more edges to the previous vertex as the previous vertex has to the currently visited vertex. 
					// break off traversal
					return;
				}
			}
			if(verticesProcessed[vertex] == VertexColor.Visited)
			{
				// vertex has already been processed
				return;
			}
			verticesProcessed[vertex] = VertexColor.Visiting;
			var adjacencyList = GeneralUtils.PerformSyncedAction(()=>_graphToCrawl.GetAdjacencyListForVertex(vertex), _graphToCrawl.SyncRoot, _graphToCrawl.IsSynchronized);
			OnVisiting(vertex, edges);

			// crawl to all this vertex' related vertices, if they've not yet been processed. 
			foreach(KeyValuePair<TVertex, HashSet<TEdge>> relatedVertexEdgesTuple in adjacencyList)
			{
				if(_abortCrawl)
				{
					break;
				}
				// recurse to the related vertex, over the edges in the list of edges. We simply pass the whole hashset as it doesn't matter which edge is picked
				// to crawl the graph, but an algorithm might want to know which edges to pick from, so it's better to pass them as a whole than to pass them one by
				// one in a non-sequential order (through backtracking)
				Visit(relatedVertexEdgesTuple.Key, verticesProcessed, relatedVertexEdgesTuple.Value);
			}

			OnVisited(vertex, edges);
			verticesProcessed[vertex] = VertexColor.Visited;
		}


		#region Class Property Declarations
		/// <summary>
		/// Sets the abortCrawl flag which will abort the crawl of the graph. 
		/// </summary>
		protected bool AbortCrawl
		{
			set { _abortCrawl = value; }
		}
		#endregion
	}
}
