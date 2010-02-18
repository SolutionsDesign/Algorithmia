//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2010 Solutions Design. All rights reserved.
// http://www.sd.nl
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2010 Solutions Design. All rights reserved.
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
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.Graphs
{
	/// <summary>
	/// Class which represents an edge in a graph. It can be used for directed and non-directed edges. A directed edge from A to B means A has a connection with B
	/// but B doesn't have a connection with A.
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertices in this edge.</typeparam>
	public class Edge<TVertex> : IEdge<TVertex>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Edge&lt;TVertex&gt;"/> class.
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <remarks>Creates a directed (from startVertex to endVertex) edge</remarks>
		public Edge(TVertex startVertex, TVertex endVertex) : this(startVertex, endVertex, true)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Edge&lt;TVertex&gt;"/> class.
		/// </summary>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="endVertex">The end vertex.</param>
		/// <param name="isDirected">if set to true, this edge is considered a directed edge, from startVertex to endVertex</param>
		public Edge(TVertex startVertex, TVertex endVertex, bool isDirected)
		{
			ArgumentVerifier.CantBeNull(startVertex, "startVertex");
			ArgumentVerifier.CantBeNull(endVertex, "endVertex");
			this.StartVertex = startVertex;
			this.EndVertex = endVertex;
			this.IsDirected = isDirected;
		}

		#region Class Property Declarations
		/// <summary>
		/// Gets the start vertex of the edge.
		/// </summary>
		public TVertex StartVertex { get; private set; }


		/// <summary>
		/// Gets the end vertex of the edge.
		/// </summary>
		public TVertex EndVertex { get; private set;}

		/// <summary>
		/// Gets a value indicating whether this edge is directed. If true, the edge is directed from startVertex to endVertex and is seen as an edge only between
		/// startVertex and endVertex, not between endVertex and startVertex. If false, this edge isn't considered a directed edge and is seen as an edge between
		/// startVertex and endVertex and also between endVertex and startVertex.
		/// </summary>
		public bool IsDirected { get; private set; }
		#endregion
	}
}
