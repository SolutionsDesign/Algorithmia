//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2018 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2018 Solutions Design. All rights reserved.
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
using System.Linq;
using System.Text;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// Basic class which simply makes the graph base act like a non-directed graph. 
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertex.</typeparam>
	/// <typeparam name="TEdge">The type of the information inside the edge.</typeparam>
	public class NonDirectedGraph<TVertex, TEdge> : GraphBase<TVertex, TEdge>
		where TEdge : NonDirectedEdge<TVertex>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		public NonDirectedGraph()
			: this(isCommandified:false, isSynchronized:false)
		{
		}
		

		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public NonDirectedGraph(bool isCommandified) : this(isCommandified, isSynchronized:false)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		/// <param name="isSynchronized">if set to <c>true</c> this list is a synchronized collection, using a lock on SyncRoot to synchronize activity in multithreading
		/// scenarios</param>
		public NonDirectedGraph(bool isCommandified, bool isSynchronized)
			: base(false, isCommandified, isSynchronized)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		public NonDirectedGraph(Func<TVertex, TVertex, TEdge> edgeProducerFunc)
			: this(edgeProducerFunc, isCommandified:false, isSynchronized:false)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public NonDirectedGraph(Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified) 
			: this(edgeProducerFunc, isCommandified, isSynchronized:false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		/// <param name="isSynchronized">if set to <c>true</c> this list is a synchronized collection, using a lock on SyncRoot to synchronize activity in multithreading
		/// scenarios</param>
		public NonDirectedGraph(Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified, bool isSynchronized)
			: base(false, isCommandified, isSynchronized)
		{
			this.EdgeProducerFunc = edgeProducerFunc;
		}


		/// <summary>
		/// Copy constructor of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		public NonDirectedGraph(NonDirectedGraph<TVertex, TEdge> graph)
			: this(graph, null, false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public NonDirectedGraph(NonDirectedGraph<TVertex, TEdge> graph, bool isCommandified)
			: this(graph, null, isCommandified, isSynchronized:false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		public NonDirectedGraph(NonDirectedGraph<TVertex, TEdge> graph, Func<TVertex, TVertex, TEdge> edgeProducerFunc)
			: this(graph, edgeProducerFunc, isCommandified:false, isSynchronized:false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public NonDirectedGraph(NonDirectedGraph<TVertex, TEdge> graph, Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified)
			: this(graph, edgeProducerFunc, isCommandified, isSynchronized:false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="NonDirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		/// <param name="isSynchronized">if set to <c>true</c> this list is a synchronized collection, using a lock on SyncRoot to synchronize activity in multithreading
		/// scenarios</param>
		public NonDirectedGraph(NonDirectedGraph<TVertex, TEdge> graph, Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified, bool isSynchronized)
			: base(graph, false, isCommandified, isSynchronized)
		{
			this.EdgeProducerFunc = edgeProducerFunc;
		}
	}
}
