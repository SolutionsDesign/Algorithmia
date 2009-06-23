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
//		- Frans  Bouma [FB]
//		- Jeroen van den Bos [JB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.Algorithmia.Commands;
using SD.Tools.BCLExtensions.CollectionsRelated;
using SD.Tools.BCLExtensions.SystemRelated;
using SD.Tools.Algorithmia.Graphs.Algorithms;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// abstract base class for graphs. It can handle non-directed and directed graphs. A directed edge from A to B means that A has a connection with B, but
	/// B doesn't have a connection with A.
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertices in this graph.</typeparam>
	/// <typeparam name="TEdge">The type of the edges in the graph</typeparam>
	public abstract class GraphBase<TVertex, TEdge>
		where TEdge : class, IEdge<TVertex>
	{
		#region Class Member Declarations
		// Adjacency lists: per vertex a list of vertex-edge list combinations is stored, to quickly traverse the graph for algorithms.
		// Per vertex, there can be 0 or more vertices it is connected to. It can be connected to any vertex over 1 or more edges.
		// The related vertex is the key of the MultiValueDictionary, the edges it is connected with this vertex are stored as values in a HashSet
		// in this MultiValueDictionary.
		private Dictionary<TVertex, MultiValueDictionary<TVertex, TEdge>> _graph;
		private bool _isDirected, _isCommandified;
		private Dictionary<GraphCommandType, string> _cachedCommandDescriptions;
		private string _graphDescription;
		#endregion

		#region Enums
		/// <summary>
		/// Enum for finding back command descriptions. These descriptions are cached because they'd otherwise overflow the GC memory with string fragments
		/// </summary>
		private enum GraphCommandType
		{
			AddGraph,
			AddEdge,
			AddVertex,
			RemoveGraph,
			RemoveEdge,
			RemoveVertex,
			DisconnectVertices,
			AddVertexToGraphStructure,
			AddEdgeToGraphStructure,
			RemoveEdgeFromGraphStructure,
			RemoveVertexFromGraphStructure,
			RemoveVertexFromAdjacencyList,
		}
		#endregion

		#region Events
		/// <summary>
		/// Event which is raised when a vertex has been added
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexAdded;
		/// <summary>
		/// Event which is raised when a vertex has been removed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexRemoved;
		/// <summary>
		/// Event which is raised when an edge has been added
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeAdded;
		/// <summary>
		/// Event which is raised when an edge has been removed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeRemoved;
		/// <summary>
		/// Event which is raised when a vertex is about to be added.
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexAdding;
		/// <summary>
		/// Event which is raised when a vertex is about to be removed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexRemoving;
		/// <summary>
		/// Event which is raised when an edge is about to be added
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeAdding;
		/// <summary>
		/// Event which is raised when an edge is about to be removed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeRemoving;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="GraphBase&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="isDirected">if set to true, the graph is directed and only edges which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and edges which have IsDirected set to false are accepted.</param>
		protected GraphBase(bool isDirected) : this (isDirected, false)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GraphBase&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="isDirected">if set to true, the graph is directed and only edges which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and edges which have IsDirected set to false are accepted.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		protected GraphBase(bool isDirected, bool isCommandified)
		{
			Initialize(isDirected, isCommandified);
		}


		/// <summary>
		/// Copy constructor of the <see cref="GraphBase&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="isDirected">if set to true, the graph is directed and only edges which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and edges which have IsDirected set to false are accepted.</param>
		protected GraphBase(GraphBase<TVertex, TEdge> graph, bool isDirected) 
			: this(graph, isDirected, false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="GraphBase&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="isDirected">if set to true, the graph is directed and only edges which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and edges which have IsDirected set to false are accepted.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		protected GraphBase(GraphBase<TVertex, TEdge> graph, bool isDirected, bool isCommandified)
		{
			Initialize(isDirected, isCommandified);
			Add(graph);
		}


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0}. Vertex count: {0}. Edge count: {1}.", _graphDescription, this.VertexCount);
		}


		/// <summary>
		/// Adds all elements (i.e. vertices and edges) from the provided graph to this graph.
		/// </summary>
		/// <param name="graph">The graph to add all elements (i.e. vertices and edges) from to this graph.</param>
		public void Add(GraphBase<TVertex, TEdge> graph)
		{
			ArgumentVerifier.CantBeNull(graph, "graph");
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<GraphBase<TVertex, TEdge>>(() => PerformAddGraph(graph), null, 
										_cachedCommandDescriptions[GraphCommandType.AddGraph]));
			}
			else
			{
				PerformAddGraph(graph);
			}
		}


		/// <summary>
		/// Adds the provided edge to this graph. If the vertex(s) are not yet in this graph, it/they are added as well.
		/// </summary>
		/// <param name="edge">The edge to add to this graph. </param>
		/// <remarks>The edge has to be compatible with this graph: the edge has to be a directed edge if this graph is a directed graph and vice versa.</remarks>
		public void Add(TEdge edge)
		{
			ArgumentVerifier.CantBeNull(edge, "edge");
			CheckEdge(edge);
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TEdge>(() => PerformAddEdge(edge), null,
										_cachedCommandDescriptions[GraphCommandType.AddEdge]));
			}
			else
			{
				PerformAddEdge(edge);
			}
		}


		/// <summary>
		/// Adds the provided vertex to this graph.
		/// </summary>
		/// <param name="vertex">The vertex to add to this graph.</param>
		public void Add(TVertex vertex)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			if(!_graph.ContainsKey(vertex))
			{
				if(_isCommandified)
				{
					// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
					CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TEdge>(() => PerformAddVertex(vertex), null,
										_cachedCommandDescriptions[GraphCommandType.AddVertex]));
				}
				else
				{
					PerformAddVertex(vertex);
				}
			}
		}


		/// <summary>
		/// Removes all elements (i.e. vertices and edges) of the provided graph from this graph.
		/// </summary>
		/// <param name="graph">The graph to remove all elements (i.e. vertices and edges) of from this graph.</param>
		public void Remove(GraphBase<TVertex, TEdge> graph)
		{
			Remove(graph, false);
		}


		/// <summary>
		/// Removes all elements (i.e. vertices and/or edges) of the provided graph from this graph.
		/// </summary>
		/// <param name="graph">The graph to remove all elements (i.e. vertices and edges) of from this graph.</param>
		/// <param name="edgesOnly">if set to true, only the edges are removed, otherwise edges and vertices</param>
		public void Remove(GraphBase<TVertex, TEdge> graph, bool edgesOnly)
		{
			ArgumentVerifier.CantBeNull(graph, "graph");
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<GraphBase<TVertex, TEdge>>(() => PerformRemoveGraph(graph, edgesOnly), null,
										_cachedCommandDescriptions[GraphCommandType.RemoveGraph]));
			}
			else
			{
				PerformRemoveGraph(graph, edgesOnly);
			}
		}


		/// <summary>
		/// Removes the provided edge from the graph.
		/// </summary>
		/// <param name="edge">The edge to remove.</param>
		public void Remove(TEdge edge)
		{
			ArgumentVerifier.CantBeNull(edge, "edge");
			if(!this.Contains(edge.StartVertex) || !this.Contains(edge.EndVertex))
			{
				// not an edge in this graph
				return;
			}
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TEdge>(() => PerformRemoveEdge(edge), null, 
										_cachedCommandDescriptions[GraphCommandType.RemoveEdge]));
			}
			else
			{
				PerformRemoveEdge(edge);
			}
		}


		/// <summary>
		/// Removes a vertex from this graph.
		/// </summary>
		/// <param name="vertex">The vertex to remove from this graph.</param>
		public void Remove(TVertex vertex)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			if(!_graph.ContainsKey(vertex))
			{
				// not a vertex in this graph.
				return;
			}
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TVertex>(() => PerformRemoveVertex(vertex), null,
										_cachedCommandDescriptions[GraphCommandType.RemoveVertex]));
			}
			else
			{
				PerformRemoveVertex(vertex);
			}
		}


		/// <summary>
		/// Removes all edges from this graph between startVertex and endVertex. It will only remove edges between startVertex and endVertex, not between endVertex and startVertex,
		/// unless bothSides is set to true, then all edges between both vertices are removed, discarding their direction.
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <param name="bothSides">if set to true, the edges owned by both sides are removed, otherwise only the edges from startVertex to endVertex</param>
		public void Disconnect(TVertex startVertex, TVertex endVertex, bool bothSides)
		{
			ArgumentVerifier.CantBeNull(startVertex, "startVertex");
			ArgumentVerifier.CantBeNull(endVertex, "endVertex");
			CheckIfPartOfGraph(startVertex, "startVertex");
			CheckIfPartOfGraph(endVertex, "endVertex");
			if(_isCommandified)
			{
				// use a command to call the method so all the commands spawned by the method called are undoable with a single undo.
				CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(new Command<TVertex>(() => PerformDisconnect(startVertex, endVertex, bothSides), 
										null, _cachedCommandDescriptions[GraphCommandType.DisconnectVertices]));
			}
			else
			{
				PerformDisconnect(startVertex, endVertex, bothSides);
			}
		}
		

		/// <summary>
		/// Returns whether an edge exists in this graph between the start vertex and the end vertex specified. 
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <returns>
		/// True if there is an edge in this graph from startVertex to endVertex. False otherwise.
		/// </returns>
		/// <remarks>If this graph is a directed graph, it will return false if there is an edge between endVertex and startVertex but not between startVertex and endVertex</remarks>
		public bool ContainsEdge(TVertex startVertex, TVertex endVertex)
		{
			ArgumentVerifier.CantBeNull(startVertex, "startVertex");
			ArgumentVerifier.CantBeNull(endVertex, "endVertex");

			return _graph.ContainsKey(startVertex) && _graph[startVertex].ContainsKey(endVertex);
		}


		/// <summary>
		/// Determines whether this graph contains the edge object specified. 
		/// </summary>
		/// <param name="edge">The edge.</param>
		/// <returns>true if the edge object is present. False otherwise. Also returns false if there is an edge between the start/end vertices of the edge specified
		/// but the edge object known to this graph is different from the edge specified. 
		/// </returns>
		public bool Contains(TEdge edge)
		{
			ArgumentVerifier.CantBeNull(edge, "edge");
			// true if there's an edge between start vertex and end vertex (which automatically returns false if the vertices aren't part of this graph etc.)
			// and if this particular edge object is part of the adjacency list.
			return (ContainsEdge(edge.StartVertex, edge.EndVertex) && _graph[edge.StartVertex][edge.EndVertex].Contains(edge));
		}


		/// <summary>
		/// Returns whether a vertex exists in this graph.
		/// </summary>
		/// <param name="vertex">The vertex whose inclusion must be checked.</param>
		/// <returns>True if the provided vertex exists in this graph. False otherwise.</returns>
		public bool Contains(TVertex vertex)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			return _graph.ContainsKey(vertex);
		}


		/// <summary>
		/// Gets all the edges between startVertex and endVertex
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <returns>Set of edges between startVertex and endVertex or an empty set if one or both vertices aren't part of this graph or there are no edges.</returns>
		public HashSet<TEdge> GetEdges(TVertex startVertex, TVertex endVertex)
		{
			ArgumentVerifier.CantBeNull(startVertex, "startVertex");
			ArgumentVerifier.CantBeNull(endVertex, "endVertex");
			return this.ContainsEdge(startVertex, endVertex) ? _graph[startVertex][endVertex] : new HashSet<TEdge>();
		}


		/// <summary>
		/// Gets the adjacency list for vertex. The adjacency list is a list of TVertex - Hashset(Of TEdge) tuples, as a vertex can have multiple edges with the same
		/// related vertex. 
		/// </summary>
		/// <param name="vertex">The vertex to obtain the adjacency list for.</param>
		/// <returns>MultiValueDictionary with as key the related vertices of the passed in vertex and as value per related vertex a Hashset with edges which 
		/// connect passed in vertex with the related vertex, or null if the vertex isn't part of this graph</returns>
		public MultiValueDictionary<TVertex, TEdge> GetAdjacencyListForVertex(TVertex vertex)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			MultiValueDictionary<TVertex, TEdge> toReturn;
			_graph.TryGetValue(vertex, out toReturn);
			return toReturn;
		}


		/// <summary>
		/// Gets all the edges started from the startvertex specified.
		/// </summary>
		/// <param name="startVertex">The start vertex of the edges to obtain.</param>
		/// <returns>Set of edges starting from startVertex or an empty set if startVertex isn't part of this graph</returns>
		public HashSet<TEdge> GetEdgesFromStartVertex(TVertex startVertex)
		{
			ArgumentVerifier.CantBeNull(startVertex, "startVertex");
			HashSet<TEdge> toReturn = new HashSet<TEdge>();
			if(this.Contains(startVertex))
			{
				MultiValueDictionary<TVertex, TEdge> adjacencyList = _graph[startVertex];
				foreach(HashSet<TEdge> edges in adjacencyList.Values)
				{
					toReturn.AddRange(edges);
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Gets all the edges which end in the endVertex specified.
		/// </summary>
		/// <param name="endVertex">The end vertex of the edges to obtain.</param>
		/// <returns>Set of edges which have the specified endVertex as their endVertex, or an empty set if endVertex isn't part of this graph</returns>
		public HashSet<TEdge> GetEdgesToEndVertex(TVertex endVertex)
		{
			ArgumentVerifier.CantBeNull(endVertex, "endVertex");
			HashSet<TEdge> toReturn = new HashSet<TEdge>();
			if(this.Contains(endVertex))
			{
				var q = from adjacencyList in _graph.Values
						where adjacencyList.ContainsKey(endVertex)
						select adjacencyList;
				foreach(MultiValueDictionary<TVertex, TEdge> adjacencyList in q)
				{
					foreach(HashSet<TEdge> edges in adjacencyList.Values)
					{
						toReturn.AddRange(edges.Where(e=>e.StartVertex.Equals(endVertex) || e.EndVertex.Equals(endVertex)));
					}
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Gets the orphaned vertices. Orphaned vertices are vertices which are not part of any edge in the graph
		/// </summary>
		/// <returns>Set of vertices which aren't part of any edge in the graph</returns>
		public HashSet<TVertex> GetOrphanedVertices()
		{
			// do an Except query between the list of vertices and the list of vertices in the adjacency lists. As a vertex in an adjancency list is always
			// part of the graph, this doesn't give false positives. 
			HashSet<TVertex> verticesInEdges = new HashSet<TVertex>();
			foreach(KeyValuePair<TVertex, MultiValueDictionary<TVertex, TEdge>> adjacencyListsPerVertex in _graph)
			{
				if(adjacencyListsPerVertex.Value.Count == 0)
				{
					// isn't a startvertex in any edge.
					continue;
				}
				verticesInEdges.Add(adjacencyListsPerVertex.Key);
				verticesInEdges.AddRange(adjacencyListsPerVertex.Value.Keys);
			}
			return new HashSet<TVertex>(this.Vertices.Except(verticesInEdges));
		}


		/// <summary>
		/// Gets a subgraph from this graph with the vertices and edges which match the functions specified. It creates a new instance and 
		/// places the same vertex and edge instances in that graph. 
		/// </summary>
		/// <typeparam name="TGraph">The type of the graph.</typeparam>
		/// <param name="vertexSelector">The vertex selector. Can be null, which will result in all vertices being added</param>
		/// <param name="edgeSelector">The edge selector. Can be null, which will result in all edges being added</param>
		/// <returns>New graph instance with all vertices and edges from this graph which match the selector function specified</returns>
		public TGraph GetSubGraph<TGraph>(Func<TVertex, bool> vertexSelector, Func<TEdge, bool> edgeSelector)
			where TGraph : GraphBase<TVertex, TEdge>, new()
		{
			TGraph toReturn = new TGraph();

			// first copy vertices
			foreach(TVertex vertex in this.Vertices)
			{
				if((vertexSelector == null) || vertexSelector(vertex))
				{
					toReturn.Add(vertex);
				}
			}
			// then copy edges. If the edges aren't compatible with the graph, it's the responsibility of the caller. 
			foreach(TEdge edge in this.Edges)
			{
				if((edgeSelector == null) || edgeSelector(edge))
				{
					toReturn.Add(edge);
				}
			}

			return toReturn;
		}


		/// <summary>
		/// Returns a composition of the two provided graphs. See http://en.wikipedia.org/wiki/Lexicographic_product_of_graphs.
		/// </summary>
		/// <typeparam name="TGraph">The type of the graph.</typeparam>
		/// <param name="g">The first graph to compose.</param>
		/// <param name="h">The second graph to compose.</param>
		/// <param name="edgeProducerFunc">The edge producer func, which is used to produce edges for the graph to return.</param>
		/// <returns>
		/// The graph composition (lexicographic product) of the two provided graphs.
		/// </returns>
		public static TGraph Compose<TGraph>(TGraph g, TGraph h, Func<TVertex, TVertex, TEdge> edgeProducerFunc)
			where TGraph : GraphBase<TVertex, TEdge>, new()
		{
			ArgumentVerifier.CantBeNull(g, "g");
			ArgumentVerifier.CantBeNull(h, "h");
			ArgumentVerifier.CantBeNull(edgeProducerFunc, "edgeProducerFunc");

			TGraph result = new TGraph();
			if(g.EdgeCount > 0 && h.EdgeCount > 0)
			{
				foreach(TEdge gEdge in g.Edges)
				{
					foreach(TEdge hEdge in h.Edges)
					{
						if(gEdge.EndVertex.Equals(hEdge.StartVertex))
						{
							result.Add(edgeProducerFunc(gEdge.StartVertex, hEdge.EndVertex));
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Validates if the edge passed in is addable to this graph structure. The start vertex and the end vertex for the are given as well. The same edge
		/// is present between startVertex and endVertex and also between endVertex and startVertex if the graph is a nondirected graph, hence the necessity of these two
		/// vertices in this routine
		/// </summary>
		/// <param name="edgeToAdd">The edge to validate.</param>
		/// <param name="startVertex">The start vertex the edge is starting from.</param>
		/// <param name="endVertex">The end vertex the edge is leading to.</param>
		/// <returns>true if the edge can be added, false otherwise. Returning false results in the edge not being added</returns>
		protected virtual bool ValidateEdgeForAddition(TEdge edgeToAdd, TVertex startVertex, TVertex endVertex)
		{
			return true;
		}


		/// <summary>
		/// Validates if the vertex passed in is addable to this graph structure.
		/// </summary>
		/// <param name="vertexToAdd">The vertex to validate.</param>
		/// <returns>true if the vertex can be added, false otherwise. Returning false results in the vertex not being added</returns>
		protected virtual bool ValidateVertexForAddition(TVertex vertexToAdd)
		{
			return true;
		}

		/// <summary>
		/// Validates if the edge passed in is removable from this graph structure. The start vertex and the end vertex for the are given as well. The same edge
		/// is present between startVertex and endVertex and also between endVertex and startVertex if the graph is a nondirected graph, hence the necessity of these two
		/// vertices in this routine
		/// </summary>
		/// <param name="edgeToRemove">The edge to remove.</param>
		/// <param name="startVertex">The start vertex the edge is starting from.</param>
		/// <param name="endVertex">The end vertex the edge is leading to.</param>
		/// <returns>true if the edge can be removed, false otherwise. Returning false results in the edge not being removed</returns>
		protected virtual bool ValidateEdgeForRemoval(TEdge edgeToRemove, TVertex startVertex, TVertex endVertex)
		{
			return true;
		}

		/// <summary>
		/// Validates if the vertex passed in is removable from this graph structure. 
		/// </summary>
		/// <param name="vertexToRemove">The vertex to remove.</param>
		/// <returns>true if the vertex can be removed, false otherwise. Returning false results in the vertex not being removed</returns>
		protected virtual bool ValidateVertexForRemoval(TVertex vertexToRemove)
		{
			return true;
		}


		/// <summary>
		/// Validates if the vertexToRemove passed in is removable from the adjacency list of the vertex <i>vertex</i>. Removing the vertex means all edges from vertex to
		/// vertexToRemove are physically removed from the graph structure.
		/// </summary>
		/// <param name="vertexToRemove">The vertex to remove from the adjacency list.</param>
		/// <param name="vertex">The vertex which owns the adjacency list.</param>
		/// <returns></returns>
		protected virtual bool ValidateVertexFromRemovalFromAdjacencyList(TVertex vertexToRemove, TVertex vertex)
		{
			return true;
		}

		/// <summary>
		/// Called when a vertex has been added
		/// </summary>
		/// <param name="vertexAdded">The vertex added.</param>
		/// <remarks>Raises VertexAdded</remarks>
		protected virtual void OnVertexAdded(TVertex vertexAdded)
		{
			if(!this.SuppressEvents)
			{
				this.VertexAdded.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertexAdded));
			}
		}


		/// <summary>
		/// Called when a vertex has been removed
		/// </summary>
		/// <param name="vertexRemoved">The vertex removed.</param>
		/// <remarks>Raises VertexRemoved</remarks>
		protected virtual void OnVertexRemoved(TVertex vertexRemoved)
		{
			if(!this.SuppressEvents)
			{
				this.VertexRemoved.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertexRemoved));
			}
		}


		/// <summary>
		/// Called when an edge has been added
		/// </summary>
		/// <param name="edgeAdded">The edge added.</param>
		/// <remarks>Raises EdgeAdded</remarks>
		protected virtual void OnEdgeAdded(TEdge edgeAdded)
		{
			if(!this.SuppressEvents)
			{
				this.EdgeAdded.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edgeAdded));
			}
		}


		/// <summary>
		/// Called when an edge has been removed
		/// </summary>
		/// <param name="edgeRemoved">The edge removed.</param>
		/// <remarks>Raises EdgeRemoved</remarks>
		protected virtual void OnEdgeRemoved(TEdge edgeRemoved)
		{
			if(!this.SuppressEvents)
			{
				this.EdgeRemoved.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edgeRemoved));
			}
		}

		/// <summary>
		/// Called when a vertex is about to be added
		/// </summary>
		/// <param name="vertexToBeAdded">The vertex to be added.</param>
		/// <remarks>Raises VertexAdding</remarks>
		protected virtual void OnVertexAdding(TVertex vertexToBeAdded)
		{
			if(!this.SuppressEvents)
			{
				this.VertexAdding.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertexToBeAdded));
			}
		}


		/// <summary>
		/// Called when a vertex is about to be removed
		/// </summary>
		/// <param name="vertexToBeRemoved">The vertex to be removed.</param>
		/// <remarks>Raises VertexRemoving</remarks>
		protected virtual void OnVertexRemoving(TVertex vertexToBeRemoved)
		{
			if(!this.SuppressEvents)
			{
				this.VertexRemoving.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertexToBeRemoved));
			}
		}


		/// <summary>
		/// Called when an edge is about to be added
		/// </summary>
		/// <param name="edgeToBeAdded">The edge to be added.</param>
		/// <remarks>Raises EdgeAdding</remarks>
		protected virtual void OnEdgeAdding(TEdge edgeToBeAdded)
		{
			if(!this.SuppressEvents)
			{
				this.EdgeAdding.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edgeToBeAdded));
			}
		}


		/// <summary>
		/// Called when an edge is about to be removed
		/// </summary>
		/// <param name="edgeToBeRemoved">The edge to be removed.</param>
		/// <remarks>Raises EdgeRemoving</remarks>
		protected virtual void OnEdgeRemoving(TEdge edgeToBeRemoved)
		{
			if(!this.SuppressEvents)
			{
				this.EdgeRemoving.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edgeToBeRemoved));
			}
		}


		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="isDirected">if set to true, the graph is directed and only EdgeBase instances which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and EdgeBase instances which have IsDirected set to false are accepted.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		private void Initialize(bool isDirected, bool isCommandified)
		{
			_graph = new Dictionary<TVertex, MultiValueDictionary<TVertex, TEdge>>();
			_isDirected = isDirected;
			_isCommandified = isCommandified;
			string directedString = "Non-directed";
			if(_isDirected)
			{
				directedString = "Directed";
			}
			_graphDescription = string.Format("Graph<{0}, {1}> ({2})", typeof(TVertex).Name, typeof(TEdge).Name, directedString);
			BuildCachedCommandDescriptions();
		}


		/// <summary>
		/// Builds the cached command descriptions.
		/// </summary>
		private void BuildCachedCommandDescriptions()
		{
			_cachedCommandDescriptions = new Dictionary<GraphCommandType, string>();
			_cachedCommandDescriptions.Add(GraphCommandType.AddGraph, string.Format("Add graph to {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.AddEdge, string.Format("Add new edge to {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.AddVertex, string.Format("Add new vertex to {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveGraph, string.Format("Remove sub-graph from {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveEdge, string.Format("Remove edge from {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveVertex, string.Format("Remove vertex from {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.DisconnectVertices, string.Format("Disconnect two vertices in {0} instance", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.AddVertexToGraphStructure, string.Format("Add a new vertex physically to the {0} structure", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.AddEdgeToGraphStructure, string.Format("Add a new edge physically to the {0} structure", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveEdgeFromGraphStructure, string.Format("Remove an edge physically from the {0} structure", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveVertexFromGraphStructure, string.Format("Remove a vertex physically from the {0} structure", _graphDescription));
			_cachedCommandDescriptions.Add(GraphCommandType.RemoveVertexFromAdjacencyList, string.Format("Remove a vertex physically from an adjacencylist in the {0} structure", _graphDescription));
		}
		

		/// <summary>
		/// Checks if the vertex is part of the graph. If not it will throw an ArgumentException that the argument with the passed in name isn't part of the graph
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <param name="argumentName">Name of the argument.</param>
		private void CheckIfPartOfGraph(TVertex vertex, string argumentName)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			if(!this.Contains(vertex))
			{
				throw new ArgumentException(string.Format("'{0}' isn't part of the graph", vertex), argumentName);
			}
		}


		/// <summary>
		/// Checks the edge if it's an edge which can be part of this graph. This depends on the fact if the edge's IsDirected flag is the same as this graph's
		/// IsDirected flag. If the edge isn't compatible, an argumentexception is raised. 
		/// </summary>
		/// <param name="edge">The edge.</param>
		private void CheckEdge(TEdge edge)
		{
			if(edge.IsDirected != _isDirected)
			{
				throw new ArgumentException(string.Format("The edge isn't compatible with this graph: The graph's IsDirected flag is {0}, while the edge's IsDirected flag is {1}", _isDirected, edge.IsDirected));
			}
		}


        /// <summary>
        /// A graph is called connected if every pair of distinct vertices in the graph is connected (directly or indirectly). 
        /// A connected component is a maximal connected subgraph of G. Each vertex belongs to exactly one connected component, as does each edge.
        /// A directed graph is called weakly connected if replacing all of its directed edges with undirected edges produces a connected (undirected) graph. 
        /// It is strongly connected or strong if it contains a directed path from u to v and a directed path from v to u for every pair of vertices u,v. 
        /// The strong components are the maximal strongly connected subgraphs.
        /// See http://en.wikipedia.org/wiki/Connectivity_(graph_theory)
        /// 
        /// We will only check for a connected un-directed graph or a weakly connected directed graph (same logic).
        /// </summary>
        /// <returns>True if the graph is considered connected, false otherwise</returns>
        public virtual bool IsConnected()
        {
			IRootDetector rootDetector;
            if(this.IsDirected)
            {
				// create non-directed copy. 
				rootDetector = new RootDetector<TVertex, NonDirectedEdge<TVertex>>(GetAsNonDirectedCopy());
            }
            else
            {
				// use this instance directly. 
                rootDetector = new RootDetector<TVertex, TEdge>(this);
            }

            // Use the DepthFirstCrawler to detect the number of roots of this graph. If the number of roots is higher than 1, the graph isn't connected. 
			return !(rootDetector.SearchForRoots() > 1);
        }


		/// <summary>
		/// Creates a NonDirectedGraph version of this graph. Always creates a copy, even if this graph is a non-directed graph.
		/// </summary>
		/// <returns>this graph as a nondirected copy, even if this graph is a non-directed graph</returns>
		public NonDirectedGraph<TVertex, NonDirectedEdge<TVertex>> GetAsNonDirectedCopy()
        {            
            NonDirectedGraph<TVertex, NonDirectedEdge<TVertex>> toReturn = new NonDirectedGraph<TVertex, NonDirectedEdge<TVertex>>();
            foreach(TEdge edge in this.Edges)
            {
				toReturn.Add(new NonDirectedEdge<TVertex>(edge.StartVertex, edge.EndVertex));
            }
            return toReturn;
        }


		#region Perform methods for actions on graph state
		/// <summary>
		/// Performs the add action for adding a complete graph to this graph.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformAddGraph(GraphBase<TVertex, TEdge> graph)
		{
			foreach(TEdge edge in graph.Edges)
			{
				Add(edge);
			}
		}


		/// <summary>
		/// Performs the remove action for removing a complete graph from this graph.
		/// </summary>
		/// <param name="graphToRemove">The graph to remove all elements (i.e. vertices and edges) of from this graph.</param>
		/// <param name="edgesOnly">if set to true, only the edges are removed, otherwise edges and vertices</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformRemoveGraph(GraphBase<TVertex, TEdge> graphToRemove, bool edgesOnly)
		{
			if(edgesOnly)
			{
				foreach(TEdge edge in graphToRemove.Edges)
				{
					Remove(edge);
				}
			}
			else
			{
				foreach(TVertex vertex in graphToRemove.Vertices)
				{
					Remove(vertex);
				}
			}

		}


		/// <summary>
		/// Performs the remove action for an edge from this graph
		/// </summary>
		/// <param name="edgeToRemove">The edge to remove.</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformRemoveEdge(TEdge edgeToRemove)
		{
			RemoveEdgeFromGraphStructure(edgeToRemove, edgeToRemove.StartVertex, edgeToRemove.EndVertex);
			if(!edgeToRemove.IsDirected)
			{
				RemoveEdgeFromGraphStructure(edgeToRemove, edgeToRemove.EndVertex, edgeToRemove.StartVertex);
			}
			RemoveOrphanedVerticesAfterEdgeRemove(edgeToRemove);
		}
		

		/// <summary>
		/// Performs the Add action for a vertex to the adjacency lists.
		/// </summary>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformAddVertex(TVertex vertexToAdd)
		{
			AddVertexToGraphStructure(vertexToAdd);
		}


		/// <summary>
		/// Performs the Add action for the edge.
		/// </summary>
		/// <param name="edgeToAdd">The edge to add.</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformAddEdge(TEdge edgeToAdd)
		{
			Add(edgeToAdd.StartVertex);
			Add(edgeToAdd.EndVertex);
			AddEdgeToGraphStructure(edgeToAdd, edgeToAdd.StartVertex, edgeToAdd.EndVertex);
            if (!edgeToAdd.IsDirected)
            {
                // not directed, the endvertex also has a connection with startvertex.
                AddEdgeToGraphStructure(edgeToAdd, edgeToAdd.EndVertex, edgeToAdd.StartVertex);
            }
		}


		/// <summary>
		/// Performs the remove vertex.
		/// </summary>
		/// <param name="vertexToRemove">The vertex to remove.</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformRemoveVertex(TVertex vertexToRemove)
		{
			// First collect all edges and remove these first. Code observing this graph will get notified and the vertex is still there. 
			HashSet<TEdge> edgesToRemove = GetEdgesFromStartVertex(vertexToRemove);
			edgesToRemove.AddRange(GetEdgesToEndVertex(vertexToRemove));
			foreach(TEdge edge in edgesToRemove)
			{
				if(_isDirected)
				{
					RemoveEdgeFromGraphStructure(edge, edge.StartVertex, edge.EndVertex);
				}
				else
				{
					// an undirected edge is added to both sides, but as there's just 1 edge instance, the startvertex is the same for both sides.
					RemoveEdgeFromGraphStructure(edge, edge.StartVertex, edge.EndVertex);
					RemoveEdgeFromGraphStructure(edge, edge.EndVertex, edge.StartVertex);
				}
			}

			// Then remove the vertex itself...
			RemoveVertexFromGraphStructure(vertexToRemove);
			// ...and also all edges to it.
			foreach(TVertex key in _graph.Keys)
			{
				RemoveVertexFromAdjacencyList(vertexToRemove, key);
			}

			// And finally remove orphaned vertices if removal of the edges made them orphaned. 
			RemoveOrphanedVertices();
		}
		

		/// <summary>
		/// Performs the disconnect action for disconnecting two graph vertices. 
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <param name="bothSides">if set to true, the edges owned by both sides are removed, otherwise only the edges from startVertex to endVertex</param>
		/// <remarks>If you want to undo actions performed by this method, call this method using a Command object.</remarks>
		private void PerformDisconnect(TVertex startVertex, TVertex endVertex, bool bothSides)
		{
			RemoveVertexFromAdjacencyList(endVertex, startVertex);
			if(bothSides)
			{
				RemoveVertexFromAdjacencyList(startVertex, endVertex);
			}
		}


		/// <summary>
		/// Removes the orphaned vertices.
		/// </summary>
		/// <remarks>Do not call this method directly if this is a commandified graph, call this through a command if no caller is called by a command</remarks>
		private void RemoveOrphanedVertices()
		{
			if(this.RemoveOrphanedVerticesOnEdgeRemoval)
			{
				HashSet<TVertex> orphanedVertices = GetOrphanedVertices();
				foreach(TVertex vertex in orphanedVertices)
				{
					RemoveVertexFromGraphStructure(vertex);
				}
			}
		}


		/// <summary>
		/// Removes the orphaned vertices after an edge remove, if the flag RemoveOrphanedVerticesOnEdgeRemoval has been set
		/// </summary>
		/// <param name="edgeRemoved">The edge removed.</param>
		private void RemoveOrphanedVerticesAfterEdgeRemove(TEdge edgeRemoved)
		{
			if(this.RemoveOrphanedVerticesOnEdgeRemoval)
			{
				HashSet<TVertex> orphanedVertices = GetOrphanedVertices();
				if(orphanedVertices.Contains(edgeRemoved.StartVertex))
				{
					Remove(edgeRemoved.StartVertex);
				}
				if(orphanedVertices.Contains(edgeRemoved.EndVertex))
				{
					Remove(edgeRemoved.EndVertex);
				}
			}
		}


		/// <summary>
		/// Adds the passed in vertex to the graph structure.
		/// </summary>
		/// <param name="vertexToAdd">The vertex to add.</param>
		/// <remarks>Do not call this method directly, call Add() to add a vertex. This method is used to physically add the vertex to the datastructures</remarks>
		private void AddVertexToGraphStructure(TVertex vertexToAdd)
		{
			if(ValidateVertexForAddition(vertexToAdd))
			{                
				if(_isCommandified)
				{
					CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(
							new Command<TVertex>(
								() =>
								{
									OnVertexAdding(vertexToAdd);
									_graph.Add(vertexToAdd, new MultiValueDictionary<TVertex, TEdge>());
									OnVertexAdded(vertexToAdd);
								},
								() =>
								{
									OnVertexRemoving(vertexToAdd);
									_graph.Remove(vertexToAdd);
									OnVertexRemoved(vertexToAdd);
								}, _cachedCommandDescriptions[GraphCommandType.AddVertexToGraphStructure]));
				}
				else
				{
					OnVertexAdding(vertexToAdd);
					_graph.Add(vertexToAdd, new MultiValueDictionary<TVertex, TEdge>());
					OnVertexAdded(vertexToAdd);
				}
			}
		}


		/// <summary>
		/// Adds the edge to the graph structure. The edge is between startVertex and endVertex specified. 
		/// </summary>
		/// <param name="edgeToAdd">The edge to add.</param>
		/// <param name="startVertex">The start vertex the edge belongs to</param>
		/// <param name="endVertex">The end vertex the edge belongs to.</param>
		/// <remarks>Do not call this method directly, call Add() to add an edge. This method is used to physically add the edge to the datastructures</remarks>
		private void AddEdgeToGraphStructure(TEdge edgeToAdd, TVertex startVertex, TVertex endVertex)
		{
			if(ValidateEdgeForAddition(edgeToAdd, startVertex, endVertex))
			{                
				if(_isCommandified)
				{
					CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(
							new Command<TEdge>(
								() =>
								{
									OnEdgeAdding(edgeToAdd);
									_graph[startVertex].Add(endVertex, edgeToAdd);
									OnEdgeAdded(edgeToAdd);
								},
								() =>
								{
									OnEdgeRemoving(edgeToAdd);
									_graph[startVertex].Remove(endVertex, edgeToAdd);
									OnEdgeRemoved(edgeToAdd);
								}, _cachedCommandDescriptions[GraphCommandType.AddEdgeToGraphStructure]));
				}
				else
				{                    
					OnEdgeAdding(edgeToAdd);
					_graph[startVertex].Add(endVertex, edgeToAdd);
					OnEdgeAdded(edgeToAdd);
				}
			}
		}


		/// <summary>
		/// Removes the edge from the graph structure. The edge to remove is between startVertex and endVertex.
		/// </summary>
		/// <param name="edgeToRemove">The edge to remove.</param>
		/// <param name="startVertex">The start vertex the edge belongs to.</param>
		/// <param name="endVertex">The end vertex the edge belongs to.</param>
		/// <remarks>Do not call this method directly. Call Remove() to remove an edge. This method is used to physically remove the edge from the datastructures</remarks>
		private void RemoveEdgeFromGraphStructure(TEdge edgeToRemove, TVertex startVertex, TVertex endVertex)
		{
			if(this.Contains(startVertex) && this.Contains(endVertex))
			{
				if(_graph[startVertex].ContainsValue(endVertex, edgeToRemove))
				{
					if(ValidateEdgeForRemoval(edgeToRemove, startVertex, endVertex))
					{
						if(_isCommandified)
						{
							CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(
								new Command<TEdge>(
										() =>
										{
											OnEdgeRemoving(edgeToRemove);
											_graph[startVertex].Remove(endVertex, edgeToRemove);
											OnEdgeRemoved(edgeToRemove);
										},
										() =>
										{
											OnEdgeAdding(edgeToRemove);
											_graph[startVertex].Add(endVertex, edgeToRemove);
											OnEdgeAdded(edgeToRemove);
										}, _cachedCommandDescriptions[GraphCommandType.RemoveEdgeFromGraphStructure]));
						}
						else
						{
							OnEdgeRemoving(edgeToRemove);
							_graph[startVertex].Remove(endVertex, edgeToRemove);
							OnEdgeRemoved(edgeToRemove);
						}
					}
				}
			}
		}


		/// <summary>
		/// Removes the vertex from graph structure. 
		/// </summary>
		/// <param name="vertexToRemove">The vertex to remove.</param>
		/// <remarks>Do not call this method directly. Call Remove() to remove a vertex. This method is used to physically remove the vertex from the datastructures</remarks>
		private void RemoveVertexFromGraphStructure(TVertex vertexToRemove)
		{
			if(_graph.ContainsKey(vertexToRemove))
			{
				if(ValidateVertexForRemoval(vertexToRemove))
				{
					if(_isCommandified)
					{
						CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(
							new Command<MultiValueDictionary<TVertex, TEdge>>(
									() =>
									{
										OnVertexRemoving(vertexToRemove);
										_graph.Remove(vertexToRemove);
										OnVertexRemoved(vertexToRemove);
									},
									() => _graph[vertexToRemove],
									al =>
									{
										OnVertexAdding(vertexToRemove);
										_graph.Add(vertexToRemove, al);
										OnVertexAdded(vertexToRemove);
									}, _cachedCommandDescriptions[GraphCommandType.RemoveVertexFromGraphStructure]));
					}
					else
					{
						OnVertexRemoving(vertexToRemove);
						_graph.Remove(vertexToRemove);
						OnVertexRemoved(vertexToRemove);
					}
				}
			}
		}


		/// <summary>
		/// Removes the vertex from adjacency list of the relatedVertex.
		/// </summary>
		/// <param name="vertexToRemove">The vertex to remove.</param>
		/// <param name="relatedVertex">The related vertex, which owns the adjancency list the vertex has to be removed from.</param>
		/// <remarks>Do not call this method directly. Call Remove() to remove a vertex. This method is used to physically remove the vertex from the datastructures</remarks>
		private void RemoveVertexFromAdjacencyList(TVertex vertexToRemove, TVertex relatedVertex)
		{
			if(this.Contains(relatedVertex))
			{
				if(_graph[relatedVertex].ContainsKey(vertexToRemove))
				{
					if(ValidateVertexFromRemovalFromAdjacencyList(vertexToRemove, relatedVertex))
					{
						if(_isCommandified)
						{
							CommandQueueManagerSingleton.GetInstance().EnqueueAndRunCommand(
								new Command<HashSet<TEdge>>(() => _graph[relatedVertex].Remove(vertexToRemove), () => _graph[relatedVertex].GetValues(vertexToRemove, true),
										hs => _graph[relatedVertex].Add(vertexToRemove, hs), _cachedCommandDescriptions[GraphCommandType.RemoveVertexFromAdjacencyList]));
						}
						else
						{
							_graph[relatedVertex].Remove(vertexToRemove);
						}
					}
				}
			}
		}
		#endregion

		#region Class Property Declarations
        
		/// <summary>if true, the graph is directed and only EdgeBase instances which have IsDirected set to true are allowed,
		/// otherwise it's a non-directed graph and EdgeBase instances which have IsDirected set to false are accepted.
		/// </summary>
		public bool IsDirected
		{
			get { return _isDirected; }
		}

		/// <summary>
		/// Returns the vertices in this graph.
		/// </summary>
		public IEnumerable<TVertex> Vertices
		{
			get
			{
				return _graph.Keys;
			}
		}

		/// <summary>
		/// Returns the edges in this graph.
		/// </summary>
		public IEnumerable<TEdge> Edges
		{
			get
			{
				HashSet<TEdge> edgesSeen = new HashSet<TEdge>();
				foreach(KeyValuePair<TVertex, MultiValueDictionary<TVertex, TEdge>> startPair in _graph)
				{
					foreach(KeyValuePair<TVertex, HashSet<TEdge>> endPair in startPair.Value)
					{
						foreach(TEdge edge in endPair.Value)
						{
							if(edgesSeen.Contains(edge))
							{
								continue;
							}
							edgesSeen.Add(edge);
							yield return edge;
						}
					}
				}
			}
		}        

		/// <summary>
		/// Returns the number of vertices in this graph.
		/// </summary>
		public int VertexCount
		{
			get
			{
				return _graph.Keys.Count;
			}
		}

		/// <summary>
		/// Returns the number of edges in this graph.
		/// If this graph is a directed graph, it counts the edge A to B as an edge, but B to A, if not present, isn't counted. A non-directed graph
		/// has an edge between A and B but also between B and A. This is counted as 1 edge, not two. 
		/// </summary>
		public int EdgeCount
		{
			get { return this.Edges.Count(); }
		}
		

		/// <summary>
		/// Gets or sets the edge producer func which produces edges for this graph. Used in some algorithms which have to produce edges. 
		/// </summary>
		public Func<TVertex, TVertex, TEdge> EdgeProducerFunc { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether vertices which are not part of any edge anymore are removed from the graph when the edge they're part 
		/// of is removed from the graph (so they effectively are orphaned). Default is false.
		/// </summary>
		public bool RemoveOrphanedVerticesOnEdgeRemoval { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether events are blocked from being raised (true) or not (false, default)
		/// </summary>
		protected bool SuppressEvents { get; set; }
		#endregion
    }

}
