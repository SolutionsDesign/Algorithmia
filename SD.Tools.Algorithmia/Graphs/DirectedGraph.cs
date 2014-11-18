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
using System.Linq;
using System.Text;
using SD.Tools.Algorithmia.GeneralDataStructures;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// Basic class which simply makes the graph base act like a directed graph. 
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertex.</typeparam>
	/// <typeparam name="TEdge">The type of the information inside the edge.</typeparam>
	public class DirectedGraph<TVertex, TEdge> : GraphBase<TVertex, TEdge>
		where TEdge : DirectedEdge<TVertex>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		public DirectedGraph() : base(true)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public DirectedGraph(bool isCommandified)
			: base(true, isCommandified)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		public DirectedGraph(Func<TVertex, TVertex, TEdge> edgeProducerFunc)
			: base(true)
		{
			this.EdgeProducerFunc = edgeProducerFunc;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public DirectedGraph(Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified)
			: base(true, isCommandified)
		{
			this.EdgeProducerFunc = edgeProducerFunc;
		}


		/// <summary>
		/// Copy constructor of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		public DirectedGraph(DirectedGraph<TVertex, TEdge> graph) : this(graph, null, false)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public DirectedGraph(DirectedGraph<TVertex, TEdge> graph, bool isCommandified)
			: this(graph, null, isCommandified)
		{
		}


		/// <summary>
		/// Copy constructor of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		public DirectedGraph(DirectedGraph<TVertex, TEdge> graph, Func<TVertex, TVertex, TEdge> edgeProducerFunc)
			: this(graph, edgeProducerFunc, false)
		{
		}

		
		/// <summary>
		/// Copy constructor of the <see cref="DirectedGraph&lt;TVertex, TEdge&gt;"/> class.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="edgeProducerFunc">The edge producer func which produces edges for this directed graph. Used in some algorithms which 
		/// have to produce edges.</param>
		/// <param name="isCommandified">If set to true, the graph is a commandified graph, which means all actions taken on this graph which mutate 
		/// graph state are undoable.</param>
		public DirectedGraph(DirectedGraph<TVertex, TEdge> graph, Func<TVertex, TVertex, TEdge> edgeProducerFunc, bool isCommandified)
			: base(graph, true, isCommandified)
		{
			this.EdgeProducerFunc = edgeProducerFunc;
		}


		/// <summary>
		/// Returns the transitive closure of this graph using the Floyd-Warshall algorithm.
		/// See http://en.wikipedia.org/wiki/Transitive_closure and http://en.wikipedia.org/wiki/Floyd-Warshall_algorithm.
		/// </summary>
		/// <returns>The transitive closure of this graph.</returns>
		public virtual DirectedGraph<TVertex, TEdge> TransitiveClosure()
		{
			DirectedGraph<TVertex, TEdge> result = new DirectedGraph<TVertex, TEdge>(this, this.EdgeProducerFunc);
			if(this.EdgeProducerFunc == null)
			{
				throw new InvalidOperationException("To be able to produce a Transitive Closure of this graph, the graph has to have its EdgeProducerFunc set to produce new edges. It's currently not set (null).");
			}

		    foreach(TVertex i in this.Vertices)
		    {
		        foreach(TVertex j in this.Vertices)
		        {
		            foreach(TVertex k in this.Vertices)
		            {
		                if(!j.Equals(i) && !k.Equals(i))
		                {
		                    if(result.ContainsEdge(j, i) && result.ContainsEdge(i, k) && !result.ContainsEdge(j, k))
		                    {
								result.Add(this.EdgeProducerFunc(j, k));
		                    }
		                }
		            }
		        }
		    }
		    return result;
		}
	}
}
