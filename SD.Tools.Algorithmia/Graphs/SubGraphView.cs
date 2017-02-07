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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SD.Tools.Algorithmia.UtilityClasses;
using SD.Tools.BCLExtensions.SystemRelated;
using SD.Tools.Algorithmia.GeneralInterfaces;
using SD.Tools.Algorithmia.Commands;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// Class which represents a subgraph view on a main graph with a subset of the vertices/edges of the main graph.
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertices in this graph.</typeparam>
	/// <typeparam name="TEdge">The type of the edges in the graph</typeparam>
	/// <remarks>SubGraphView instances are used to 'view' a subset of a bigger graph and maintain themselves based on the actions on the
	/// main graph. Adding/removing vertices / edges from this SubGraphView removes them only from this view, not from the main graph. Adding
	/// vertices/edges to the main graph will add the vertex/edge to this view if the added element meets criteria (implemented through polymorphism, 
	/// by default no criteria are set, so no vertex/edge is added if it's added to the main graph). Removing a vertex/edge from the main graph will remove
	/// the vertex / edge from this view if it's part of this view. As this view binds to events on the main graph, it's key to call Dispose() on an 
	/// instance of SubGraphView if it's no longer needed to make sure event handlers are cleaned up.
	/// This view has no adjacency lists, as they're located in the main graph. 
	/// </remarks>
	public class SubGraphView<TVertex, TEdge> : IDisposable, INotifyAsRemoved
		where TEdge : class, IEdge<TVertex>
	{
		#region Class Property Declarations
		private bool _isDisposed, _eventsBound;
		private readonly bool _isCommandified;
		private readonly HashSet<TVertex> _vertices;
		private readonly HashSet<TEdge> _edges;
		#endregion

		#region Events
		/// <summary>
		/// Event which is raised when a vertex has been added to this SubGraphView
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexAdded;
		/// <summary>
		/// Event which is raised when a vertex has been removed from this SubGraphView
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexRemoved;
		/// <summary>
		/// If TVertex supports change notification, this event is raised when a vertex in the subgraph view was changed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TVertex>> VertexChanged;
		/// <summary>
		/// Event which is raised when an edge has been added to this SubGraphView
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeAdded;
		/// <summary>
		/// Event which is raised when an edge has been removed from this SubGraphView
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeRemoved;
		/// <summary>
		/// If TEdge supports change notification, this event is raised when an edge in the subgraph view was changed
		/// </summary>
		public event EventHandler<GraphChangeEventArgs<TEdge>> EdgeChanged;
		/// <summary>
		/// Event which is raised when the subgraphview is made empty. Observers can use this event to dispose an empty subgraphview to avoid dangling event handlers.
		/// </summary>
		public event EventHandler IsEmpty;
		/// <summary>
		/// Event which is raised when this instance was disposed.
		/// </summary>
		public event EventHandler Disposed;
		/// <summary>
		/// Raised when the implementing element has been removed from its container
		/// </summary>
		public event EventHandler HasBeenRemoved;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="SubGraphView&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="mainGraph">The main graph this SubGraphView is a view on.</param>
		/// <remarks>Creates a non-commandified instance</remarks>
		public SubGraphView(GraphBase<TVertex, TEdge> mainGraph)
			: this(mainGraph, false)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="SubGraphView&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="mainGraph">The main graph this SubGraphView is a view on.</param>
		/// <param name="isCommandified">If set to true, the SubGraphView is a commandified SubGraphView, which means all actions taken on this SubGraphView 
		/// which mutate its state are undoable.</param>
		public SubGraphView(GraphBase<TVertex, TEdge> mainGraph, bool isCommandified)
		{
			ArgumentVerifier.CantBeNull(mainGraph, "mainGraph");
			_isCommandified = isCommandified;
			_vertices = new HashSet<TVertex>();
			_edges = new HashSet<TEdge>();
			this.MainGraph = mainGraph;
			BindEvents();
		}


		/// <summary>
		/// Adds the specified vertex.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		public void Add(TVertex vertex)
		{
			if(this.MainGraph.Contains(vertex))
			{
				if(!_vertices.Contains(vertex))
				{
					if(_isCommandified)
					{
						Command<TVertex>.DoNow(() =>
												{
													_vertices.Add(vertex);
													OnVertexAdded(vertex);
												},
											   () =>
												{
													_vertices.Remove(vertex);
													OnVertexRemoved(vertex);
												}, "Add vertex to SubGraphView");
					}
					else
					{
						_vertices.Add(vertex);
						OnVertexAdded(vertex);
					}
				}
			}
		}


		/// <summary>
		/// Adds the specified edge. If the vertices aren't in the view, they're added too.
		/// </summary>
		/// <param name="edge">The edge.</param>
		public void Add(TEdge edge)
		{
			if(this.MainGraph.Contains(edge))
			{
				if(!_edges.Contains(edge))
				{
					if(_isCommandified)
					{
						Command<TEdge>.DoNow(() =>
												{
													Add(edge.StartVertex);
													Add(edge.EndVertex);
													_edges.Add(edge);
													OnEdgeAdded(edge);
												},
											 () =>
												{
													_edges.Remove(edge);
													OnEdgeRemoved(edge);
												}, "Add edge to SubGraphView");
					}
					else
					{
						_edges.Add(edge);
						OnEdgeAdded(edge);
					}
				}
			}
		}


		/// <summary>
		/// Removes the vertex.
		/// </summary>
		/// <param name="toRemove">To remove.</param>
		/// <remarks>toRemove can't be null, as a graph can't have null vertices</remarks>
		public void Remove(TVertex toRemove)
		{
			ArgumentVerifier.CantBeNull(toRemove, "toRemove");
			if(_vertices.Contains(toRemove))
			{
				if(_isCommandified)
				{
					Command<TVertex>.DoNow(() =>
											{
												_vertices.Remove(toRemove);
												OnVertexRemoved(toRemove);
												CheckIsEmpty();
											},
										   () =>
											{
												_vertices.Add(toRemove);
												OnVertexAdded(toRemove);
											}, "Remove vertex to SubGraphView");
				}
				else
				{
					_vertices.Remove(toRemove);
					OnVertexRemoved(toRemove);
					CheckIsEmpty();
				}
			}
		}


		/// <summary>
		/// Removes the edge.
		/// </summary>
		/// <param name="toRemove">To remove.</param>
		/// <remarks>toRemove can't be null as a graph can't have null edges</remarks>
		public void Remove(TEdge toRemove)
		{
			ArgumentVerifier.CantBeNull(toRemove, "toRemove");
			if(_edges.Contains(toRemove))
			{
				if(_isCommandified)
				{
					Command<TEdge>.DoNow(() =>
											{
												_edges.Remove(toRemove);
												OnEdgeRemoved(toRemove);
												CheckIsEmpty();
											},
										 () =>
											{
												_edges.Add(toRemove);
												OnEdgeAdded(toRemove);
											}, "Remove edge to SubGraphView");
				}
				else
				{
					_edges.Remove(toRemove);
					OnEdgeRemoved(toRemove);
					CheckIsEmpty();
				}
			}
		}


		/// <summary>
		/// Determines whether this SubGraphView contains the passed in vertex.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <returns>true if the vertex is in this SubGraphView, false otherwise. 
		/// </returns>
		public bool Contains(TVertex vertex)
		{
			ArgumentVerifier.CantBeNull(vertex, "vertex");
			return _vertices.Contains(vertex);
		}


		/// <summary>
		/// Determines whether this SubGraphView contains the passed in edge.
		/// </summary>
		/// <param name="edge">The edge.</param>
		/// <returns>
		/// true if the edge is in this SubGraphView, false otherwise.
		/// </returns>
		public bool Contains(TEdge edge)
		{
			ArgumentVerifier.CantBeNull(edge, "edge");
			return _edges.Contains(edge);
		}


		/// <summary>
		/// Marks this instance as removed. It raises ElementRemoved
		/// </summary>
		public void MarkAsRemoved()
		{
			this.HasBeenRemoved.RaiseEvent(this);
		}


		/// <summary>
		/// Binds the event handlers to the events of the main graph.
		/// </summary>
		/// <remarks>Use this method to Bind the view to the main graph again when the view is added to a list in an undo-redo system if the events are
		/// unbound using UnbindEvents in the removal action.</remarks>
		public void BindEvents()
		{
			if(!_eventsBound)
			{
				this.MainGraph.EdgeAdded += MainGraph_EdgeAdded;
				this.MainGraph.EdgeRemoved += MainGraph_EdgeRemoved;
				this.MainGraph.VertexAdded += MainGraph_VertexAdded;
				this.MainGraph.VertexRemoved += MainGraph_VertexRemoved;
				_eventsBound = true;
			}
		}


		/// <summary>
		/// Unbinds the event handlers from the events of the main graph.
		/// </summary>
		/// <remarks>Use this method to unbind the view from the main graph in an undo-redo system rather than calling Dispose(), as
		/// Dispose can't be undone and if a subgraphview's removal has to be undone, BindEvents() has to be called again.</remarks>
		public void UnbindEvents()
		{
			if(_eventsBound)
			{
				this.MainGraph.EdgeAdded -= MainGraph_EdgeAdded;
				this.MainGraph.EdgeRemoved -= MainGraph_EdgeRemoved;
				this.MainGraph.VertexAdded -= MainGraph_VertexAdded;
				this.MainGraph.VertexRemoved -= MainGraph_VertexRemoved;
				_eventsBound = false;
			}
		}


		/// <summary>
		/// Called when a vertex has been added to this view
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		protected virtual void OnVertexAdded(TVertex vertex)
		{
			this.VertexAdded.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertex));
			BindToINotifyPropertyChanged(vertex);
		}


		/// <summary>
		/// Called when an edge has been added to this view
		/// </summary>
		/// <param name="edge">The edge.</param>
		protected virtual void OnEdgeAdded(TEdge edge)
		{
			this.EdgeAdded.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edge));
			BindToINotifyPropertyChanged(edge);
		}


		/// <summary>
		/// Called when a vertex has been removed from this view
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		protected virtual void OnVertexRemoved(TVertex vertex)
		{
			this.VertexRemoved.RaiseEvent(this, new GraphChangeEventArgs<TVertex>(vertex));
			UnbindFromINotifyPropertyChanged(vertex);
			RemoveEdgesWithVertex(vertex);
		}


		/// <summary>
		/// Called when an edge has been removed from this view
		/// </summary>
		/// <param name="edge">The edge.</param>
		protected virtual void OnEdgeRemoved(TEdge edge)
		{
			this.EdgeRemoved.RaiseEvent(this, new GraphChangeEventArgs<TEdge>(edge));
			UnbindFromINotifyPropertyChanged(edge);
		}

		
		/// <summary>
		/// Handles the event that a new vertex was added to the main graph.
		/// </summary>
		/// <param name="vertexAdded">The vertex added.</param>
		/// <remarks>By default, this routine does nothing. If you want to add this vertex to this SubGraphView, you've to add it by calling Add
		/// in a derived class, overriding this method.</remarks>
		protected virtual void HandleVertexAddedToMainGraph(TVertex vertexAdded)
		{
		}


		/// <summary>
		/// Handles the event that a new edge was added to the main graph
		/// </summary>
		/// <param name="edgeAdded">The edge added.</param>
		/// <remarks>By default, this routine does nothing. If you want to add this edge to this SubGraphView, you've to add it by calling Add
		/// in a derived class, overriding this method.</remarks>
		protected virtual void HandleEdgeAddedToMainGraph(TEdge edgeAdded)
		{
		}


		/// <summary>
		/// Handles the event that an edge was removed from the main graph.
		/// </summary>
		/// <param name="edgeRemoved">The edge removed.</param>
		/// <remarks>The view automatically updates its own datastructures already, so use this method to perform additional work</remarks>
		protected virtual void HandleEdgeRemovedFromMainGraph(TEdge edgeRemoved)
		{
		}

		
		/// <summary>
		/// Handles the event that a vertex was removed from the main graph.
		/// </summary>
		/// <param name="vertexRemoved">The vertex removed.</param>
		/// <remarks>The view automatically updates its own datastructures already, so use this method to perform additional work</remarks>
		protected virtual void HandleVertexRemovedFromMainGraph(TVertex vertexRemoved)
		{
		}


		/// <summary>
		/// Called when Disposing
		/// </summary>
		protected virtual void OnDisposing()
		{
		}


		/// <summary>
		/// Removes the edges with vertex, which is necessary when a vertex is removed so all dangling edges are removed as well. 
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		private void RemoveEdgesWithVertex(TVertex vertex)
		{
			var edgesToRemove = _edges.Where(e=>(e.EndVertex!=null && e.EndVertex.Equals(vertex)) || (e.StartVertex!=null && e.StartVertex.Equals(vertex))).ToList();
			foreach(var edge in edgesToRemove)
			{
				this.Remove(edge);
			}
		}

		
		/// <summary>
		/// Binds to INotifyPropertyChanged on the item specified
		/// </summary>
		/// <param name="item">The item.</param>
		private void BindToINotifyPropertyChanged<T>(T item)
		{
			INotifyPropertyChanged itemAsINotifyPropertyChanged = item as INotifyPropertyChanged;
			if(itemAsINotifyPropertyChanged != null)
			{
				itemAsINotifyPropertyChanged.PropertyChanged += OnElementPropertyChanged;
			}
		}


		/// <summary>
		/// Unbinds from INotifyPropertyChanged on the item specified
		/// </summary>
		/// <param name="item">The item.</param>
		private void UnbindFromINotifyPropertyChanged<T>(T item)
		{
			INotifyPropertyChanged itemAsINotifyPropertyChanged = item as INotifyPropertyChanged;
			if(itemAsINotifyPropertyChanged != null)
			{
				itemAsINotifyPropertyChanged.PropertyChanged -= OnElementPropertyChanged;
			}
		}


		/// <summary>
		/// Called when the PropertyChanged event was raised by an element in this list.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(sender is TVertex)
			{
				this.VertexChanged.RaiseEvent(this, new GraphChangeEventArgs<TVertex>((TVertex)sender));
			}
			else
			{
				if(sender is TEdge)
				{
					this.EdgeChanged.RaiseEvent(this, new GraphChangeEventArgs<TEdge>((TEdge)sender));
				}
			}
		}


		/// <summary>
		/// Checks if the subgraphview is empty and if so, it raises IsEmpty
		/// </summary>
		private void CheckIsEmpty()
		{
			if((_edges.Count <= 0) && (_vertices.Count <= 0))
			{
				this.IsEmpty.RaiseEvent(this);
			}
		}


		/// <summary>
		/// Handles the VertexRemoved event of the MainGraph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void MainGraph_VertexRemoved(object sender, GraphChangeEventArgs<TVertex> e)
		{
			Remove(e.InvolvedElement);
			HandleVertexRemovedFromMainGraph(e.InvolvedElement);
		}


		/// <summary>
		/// Handles the VertexAdded event of the MainGraph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void MainGraph_VertexAdded(object sender, GraphChangeEventArgs<TVertex> e)
		{
			HandleVertexAddedToMainGraph(e.InvolvedElement);
		}


		/// <summary>
		/// Handles the EdgeRemoved event of the MainGraph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void MainGraph_EdgeRemoved(object sender, GraphChangeEventArgs<TEdge> e)
		{
			Remove(e.InvolvedElement);
			HandleEdgeRemovedFromMainGraph(e.InvolvedElement);
		}


		/// <summary>
		/// Handles the EdgeAdded event of the MainGraph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event arguments instance containing the event data.</param>
		private void MainGraph_EdgeAdded(object sender, GraphChangeEventArgs<TEdge> e)
		{
			HandleEdgeAddedToMainGraph(e.InvolvedElement);
		}

		
		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if(disposing && !_isDisposed)
			{
				OnDisposing();
				UnbindEvents();
				_isDisposed = true;
				this.Disposed.RaiseEvent(this);
			}
		}
		#endregion

		#region Class Property Declarations
		/// <summary>
		/// Gets the main graph this SubGraphView is a view on
		/// </summary>
		public GraphBase<TVertex, TEdge> MainGraph { get; private set; }
		/// <summary>
		/// Gets the vertices contained in this SubGraphView. All vertices are part of this.MainGraph
		/// </summary>
		public IEnumerable<TVertex> Vertices 
		{
			get { return _vertices; }
		}

		/// <summary>
		/// Gets the edges contained in this SubGraphView. All edges are part of this.MainGraph
		/// </summary>
		public IEnumerable<TEdge> Edges 
		{
			get { return _edges; }
		}
		#endregion

	}
}
