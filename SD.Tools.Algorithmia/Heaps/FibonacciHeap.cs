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
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.Heaps
{
	/// <summary>
	/// Implementation of a Fibonacci heap. See: http://en.wikipedia.org/wiki/Fibonacci_heap for details about the Fibonacci heap. A Fibonacci heap is a heap with
	/// on average one of the fastest characteristics of all known heap datastructures to date. However, the efficiency is expressed as 'amortized' time, which means 
	/// it's averaged: the fastest operations are a little slower as some other operations can be rather time consuming in some situations. There are cases where a 
	/// Binomial heap (http://en.wikipedia.org/wiki/Binomial_heap) would be more efficient, however as in general the Fibonacci heap performs more efficiently, 
	/// this library contains an implementation of Fibonacci heap instead of a Binomial heap.
	/// <para>
	/// Some time ago another variant (actually two variants) of the Fibonacci heap, the 'Relaxed' Fibonacci Heap has been proposed, one has the same characteristics as
	/// the Fibonacci heap and one has some operations more efficient. As the implementation of a Relaxed Fibonacci Heap is quite complex we've postponed its 
	/// implementation till a later date. For more information about Relaxed Fibonacci Heaps, please see: http://www.pmg.lcs.mit.edu/~chandra/publications/heap.pdf
	/// Relaxed Fibonacci Heaps have a slight advantage in parallel environments.
	/// </para>
	/// </summary>
	/// <typeparam name="TElement">The type of the element in the heap.</typeparam>
	public class FibonacciHeap<TElement> : HeapBase<TElement>
		where TElement : class
	{
		#region Class Member Declarations
		private LinkedBucketList<FibonacciHeapNode<TElement>> _trees;		// the trees which make the heap
		private FibonacciHeapNode<TElement> _heapRoot;				// the max/min element of this heap. We refer to the treenode the element is in so 
																	// we automatically have its tree
		private int _count;
		private List<Dictionary<TElement, FibonacciHeapNode<TElement>>> _elementToHeapNodeMappings;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="FibonacciHeap&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="keyCompareFunc">The key compare func, which is used to compare two elements based on their key. Based on the type of the heap, min heap
		/// or max heap, the first element is placed as parent or as child: if this heap is a min-heap, and the first element, according to keyCompareFunc is
		/// bigger than the second element, the second element will be the parent of the first element. In a max-heap, the first element will be the parent of
		/// the second element in that situation.</param>
		/// <param name="isMinHeap">Flag to signal if this heap is a min heap or a max heap</param>
		public FibonacciHeap(Comparison<TElement> keyCompareFunc, bool isMinHeap) : base(keyCompareFunc, isMinHeap)
		{
			InitDataStructures();
		}


		/// <summary>
		/// Extracts and removes the root of the heap.
		/// </summary>
		/// <returns>
		/// the root element deleted, or null/default if the heap is empty
		/// </returns>
		public override TElement ExtractRoot()
		{
			if(this.Count <= 0)
			{
				return null;
			}

			// as there are elements, root has a value.
			FibonacciHeapNode<TElement> rootElement = _heapRoot;
			TElement toReturn = rootElement.Contents;

			// first remove the root from the heap, this will remove it from its tree its in. 
			RemoveInternal(rootElement);

			// Consolidate trees so there's at most 1 tree per tree degree. During this process a new heap root (the min/max) is found.
			ConsolidateTrees();
			return toReturn;
		}


		/// <summary>
		/// Updates the key of the element passed in. Only use this method for elements where the key is a property of the element, not the element itself.
		/// This means that if you have a heap with value typed elements (e.g. integers), updating the key is updating the value of the element itself, and because
		/// a heap might contain elements with the same value, this could lead to undefined results.
		/// </summary>
		/// <typeparam name="TKeyType">The type of the key type.</typeparam>
		/// <param name="element">The element which key has to be updated</param>
		/// <param name="keyUpdateFunc">The key update func, which takes 2 parameters: the element to update and the new key value.</param>
		/// <param name="newValue">The new value for the key.</param>
		/// <remarks>First, the element to update is looked up in the heap. If the new key violates the heap property (e.g. the key is bigger than the
		/// key of the parent in a max-heap) the elements in the heap are restructured in such a way that the heap again obeys the heap property. </remarks>
		public override void UpdateKey<TKeyType>(TElement element, Action<TElement, TKeyType> keyUpdateFunc, TKeyType newValue)
		{
			FibonacciHeapNode<TElement> elementNode = FindNode(element);
			if(elementNode == null)
			{
				return;
			}

			// perform keyUpdateFunc on it with newValue
			keyUpdateFunc(element, newValue);

			// check if element now voilates heap func with parent or children
			if(CheckIfElementViolatesHeap(elementNode))
			{
				// As this heap can be a maxheap as well, and the action can make the root of the heap be violating the heap, we simply remove the element
				// and re-add it, if the element doesn't have a parent.
				if(elementNode.Parent == null)
				{
					RemoveInternal(elementNode);
					// re-add it
					Insert(elementNode.Contents);
				}
				else
				{
					// correct the situation: the elementNode isn't at the right spot anymore in the tree.
					// two situations: parent is marked or unmarked. Based on that we've to perform different actions. 
					// Place the tree with the elementNode as root as a separate tree in the list of trees of this heap and first preserve the
					// reference to the parent.
					FibonacciHeapNode<TElement> parent = elementNode.Parent;
					ConvertToNewTreeInHeap(elementNode);

					if(elementNode.Parent.IsMarked)
					{
						FibonacciHeapNode<TElement> currentAncestor = parent.Parent;
						while(currentAncestor!=null)
						{
							if(currentAncestor.IsMarked)
							{
								// cut off node, and unmark it.
								currentAncestor.IsMarked = false;
								FibonacciHeapNode<TElement> currentAncestorParent = currentAncestor.Parent;
								ConvertToNewTreeInHeap(currentAncestor);
								currentAncestor = currentAncestorParent;
							}
							else
							{
								currentAncestor.IsMarked = true;
								// done
								break;
							}
						}
					}
					else
					{
						// we'll mark the (now cut off) parent of this element that a child was cut from it.
						// marked nodes are nodes where modifications take place, and these are bumped up to the root level of the trees sooner or later
						// which collides them together in new trees at the first collide operation.
						parent.IsMarked = true;
					}

					UpdateRootIfRequired(elementNode);
				}
			}
		}


		/// <summary>
		/// Inserts the specified element into the heap at the right spot. 
		/// </summary>
		/// <param name="element">The element to insert.</param>
		/// <remarks>It's not possible to add an element twice as this leads to problems with removal and lookup routines for elements: which node to process?</remarks>
		public override void Insert(TElement element)
		{
			ArgumentVerifier.CantBeNull(element, "element");
			ArgumentVerifier.ShouldBeTrue(e => !this.Contains(e), element, "element to insert already exists in this heap.");

			FibonacciHeapNode<TElement> newNode = new FibonacciHeapNode<TElement>(element);
			// new element is the root of a new tree. We'll add it as the first node.
			newNode.BecomeTreeRoot();
			_trees.InsertHead(newNode);
			UpdateRootIfRequired(newNode);

			AddElementNodeMapping(newNode);

			// done. Increase the count of the # of elements in this heap
			_count++;
		}


		/// <summary>
		/// Merges the specified heap into this heap.
		/// </summary>
		/// <param name="toMerge">The heap to merge into this heap.</param>
		/// <remarks>Merge of two fibonacci heaps is considered O(1). This heap implementation keeps some datastructures for quickly finding elements back.
		/// Although these datastructures have an O(1) insert performance characteristic, it can be that the merge performance of this implementation is 
		/// actually slightly less efficient than O(1) with large heaps
		/// <para>
		/// This routine merges objects inside toMerge into this object. This means that this object will have references to objects inside toMerge.
		/// It's therefore not recommended to alter toMerge after this operation.
		/// </para>
		/// </remarks>
		public void Merge(FibonacciHeap<TElement> toMerge)
		{
			if((toMerge == null) || (toMerge.Count <= 0))
			{
				return;
			}

			FibonacciHeapNode<TElement> rootOfToMerge = toMerge._heapRoot;

			// obtain the data structures of the heap to merge, and merge them together. Do this by simply linking the two linked lists together, as
			// that's all that has to be done besides updating the root. All essential work is postponed till the trees have to be collided together.
			_trees.Concat(toMerge._trees);
			// add their element to node mappings to our list of mappings. We don't merge the dictionaries as that would cause this merge operation to become
			// an O(n) operation. With the addition of the list of mappings, our search routine has to check m mappings, where m is the # of merge operations 
			// this heap has been through. 
			_elementToHeapNodeMappings.AddRange(toMerge._elementToHeapNodeMappings);

			UpdateRootIfRequired(rootOfToMerge);
			_count += toMerge.Count;
			toMerge.Clear();
		}


		/// <summary>
		/// Clears all elements from the heap
		/// </summary>
		public override void Clear()
		{
			_trees.Clear();
			_heapRoot = null;
			_count = 0;
		}


		/// <summary>
		/// Removes the element specified
		/// </summary>
		/// <param name="element">The element to remove.</param>
		public override void Remove(TElement element)
		{
			FibonacciHeapNode<TElement> elementNode = FindNode(element);
			if(elementNode == null)
			{
				// not found
				return;
			}
			if(_heapRoot == elementNode)
			{
				// is heaproot, which means Remove() is the same as ExtractRoot()
				ExtractRoot();
			}
			else
			{
				// normal node which isn't the heap root, use shortcut to internal remove routine so we don't have to collide the trees. 
				RemoveInternal(elementNode);
			}
		}


		/// <summary>
		/// Determines whether this heap contains the element specified
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		/// true if the heap contains the element specified, false otherwise
		/// </returns>
		public override bool Contains(TElement element)
		{
			return (FindNode(element)!=null);
		}


		/// <summary>
		/// Inits the data structures for this heap.
		/// </summary>
		private void InitDataStructures()
		{
			_trees = new LinkedBucketList<FibonacciHeapNode<TElement>>();
			_elementToHeapNodeMappings = new List<Dictionary<TElement,FibonacciHeapNode<TElement>>>();
		}

		
		/// <summary>
		/// Converts the passed in node to a new tree in heap.
		/// </summary>
		/// <param name="elementNode">The element node.</param>
		private void ConvertToNewTreeInHeap(FibonacciHeapNode<TElement> elementNode)
		{
			elementNode.BecomeTreeRoot();
			_trees.InsertHead(elementNode);
		}


		/// <summary>
		/// Checks if the element passed in violates the heap. If so, true is returned, otherwise false. 
		/// </summary>
		/// <param name="elementNode">The element node.</param>
		/// <returns>true if the element violates the heap (not at the right spot in the trees), false otherwise</returns>
		private bool CheckIfElementViolatesHeap(FibonacciHeapNode<TElement> elementNode)
		{
			bool toReturn = false;
			if(elementNode.Parent != null)
			{
				// check if the parent of this elementNode is still correctly the parent.
				toReturn |= !this.ElementCompareFunc(elementNode.Parent.Contents, elementNode.Contents);
			}
			if(toReturn)
			{
				// already violates heap
				return true;
			}
			// check with each child (if any)
			foreach(FibonacciHeapNode<TElement> child in elementNode.Children)
			{
				toReturn |= !this.ElementCompareFunc(elementNode.Contents, child.Contents);
				if(toReturn)
				{
					// already violates heap
					break;
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Consolidates the trees in this heap so there's at most 1 tree per tree degree. It also finds the root of the tree (min/max) and sets _heapRoot.
		/// </summary>
		private void ConsolidateTrees()
		{
			Dictionary<int, ListBucket<FibonacciHeapNode<TElement>>> treeNodePerDegree = new Dictionary<int, ListBucket<FibonacciHeapNode<TElement>>>();
			if(_trees.Count < 2)
			{
				// nothing to do
				return;
			}

			// traverse the trees and if there's no tree with such a degree, add it to the list, otherwise merge the tree with the one already with that
			// degree till there's no tree with that degree left.
			ListBucket<FibonacciHeapNode<TElement>> currentTreeNode = _trees.Head;
			while(currentTreeNode != null)
			{
				ListBucket<FibonacciHeapNode<TElement>> treeNodeWithSameDegree;
				if(treeNodePerDegree.TryGetValue(currentTreeNode.Contents.Degree, out treeNodeWithSameDegree))
				{
					// there's a tree with the same node. Merge this tree with that tree till there's no tree with the same degree for this current tree
					treeNodePerDegree.Remove(currentTreeNode.Contents.Degree);
					_trees.Remove(treeNodeWithSameDegree);
					// merge trees. It depends on the nodes which one is added to which as a child. 
					if(this.ElementCompareFunc(treeNodeWithSameDegree.Contents.Contents, currentTreeNode.Contents.Contents))
					{
						// add currenttreenode's tree as child to treeNodeWithSameDegree
						treeNodeWithSameDegree.Contents.AddChild(currentTreeNode.Contents);
						// place the tree inside the currenttreenode's linkedlist node. 
						currentTreeNode.Contents = treeNodeWithSameDegree.Contents;
					}
					else
					{
						currentTreeNode.Contents.AddChild(treeNodeWithSameDegree.Contents);
					}
				}
				else
				{
					// not a tree with this degree
					treeNodePerDegree.Add(currentTreeNode.Contents.Degree, currentTreeNode);
					// move to next node.
					currentTreeNode = currentTreeNode.Next;
					continue;
				}
			}
		}


		/// <summary>
		/// Removes the heapnode passed in from the heap.
		/// </summary>
		/// <param name="nodeToRemove">The node to remove.</param>
		/// <remarks>This routine doesn't collide trees, as that's only necessary if the heaproot is removed. </remarks>
		private void RemoveInternal(FibonacciHeapNode<TElement> nodeToRemove)
		{
			if(nodeToRemove == null)
			{
				return;
			}

			// make all children separate roots in the trees list
			List<FibonacciHeapNode<TElement>> children = nodeToRemove.Children.ToList();
			foreach(FibonacciHeapNode<TElement> child in children)
			{
				ConvertToNewTreeInHeap(child);
			}

			if(nodeToRemove.Parent == null)
			{
				// a tree root
				_trees.Remove(nodeToRemove);
			}
			RemoveElementNodeMapping(nodeToRemove);
			_count--;
		}


		/// <summary>
		/// Updates the root of this heap if required.
		/// </summary>
		/// <param name="node">The node to check if it should become the new root.</param>
		private void UpdateRootIfRequired(FibonacciHeapNode<TElement> node)
		{
			if(_heapRoot == null)
			{
				// Make new node the heap root, as it's the only node in the heap
				_heapRoot = node;
			}
			else
			{
				if(!this.ElementCompareFunc(_heapRoot.Contents, node.Contents))
				{
					// newNode should be the parent of the heaproot, which means the newNode is the new root
					_heapRoot = node;
				}
			}
		}


		/// <summary>
		/// Adds the element-node relation of the node passed in to the element node mappings. 
		/// </summary>
		/// <param name="newNode">The new node.</param>
		/// <remarks>Assumes newNode doesn't exist in heap.</remarks>
		private void AddElementNodeMapping(FibonacciHeapNode<TElement> newNode)
		{
			// add to first mapping dictionary. It can be there are more, every merge operation adds new mappings, however for adding new nodes, we add
			// them to the first, so the find routine doesn't have to traverse all mapping dictionaries. 
			Dictionary<TElement, FibonacciHeapNode<TElement>> mappings;
			if(_elementToHeapNodeMappings.Count <= 0)
			{
				mappings = new Dictionary<TElement, FibonacciHeapNode<TElement>>();
				_elementToHeapNodeMappings.Add(mappings);
			}
			else
			{
				mappings = _elementToHeapNodeMappings[0];
			}

			mappings.Add(newNode.Contents, newNode);
		}


		/// <summary>
		/// Removes the element-node relation from the element node mappings
		/// </summary>
		/// <param name="nodeToRemove">The node to remove.</param>
		private void RemoveElementNodeMapping(FibonacciHeapNode<TElement> nodeToRemove)
		{
			foreach(Dictionary<TElement, FibonacciHeapNode<TElement>> mappings in _elementToHeapNodeMappings)
			{
				if(mappings.Remove(nodeToRemove.Contents))
				{
					// done, the element was located in this mappings set.
					break;
				}
			}
		}


		/// <summary>
		/// Finds the node of the element passed in.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>the node instance which contains the element passed in or null if not found</returns>
		private FibonacciHeapNode<TElement> FindNode(TElement element)
		{
			FibonacciHeapNode<TElement> toReturn = null;
			if(element != null)
			{
				foreach(Dictionary<TElement, FibonacciHeapNode<TElement>> mappings in _elementToHeapNodeMappings)
				{
					if(mappings.TryGetValue(element, out toReturn))
					{
						// found it
						break;
					}
				}
			}
			return toReturn;
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the root of the heap. Depending on the fact if this is a min or max heap, it returns the element with the minimum key (min heap) or the element
		/// with the maximum key (max heap). If the heap is empty, null / default is returned
		/// </summary>
		/// <value></value>
		public override TElement Root
		{
			get 
			{
				TElement toReturn = null;
				if(_heapRoot != null)
				{
					toReturn = _heapRoot.Contents;
				}
				return toReturn; 
			}
		}

		/// <summary>
		/// Gets the number of elements in the heap.
		/// </summary>
		public override int Count
		{
			get { return _count; }
		}
		#endregion
	}


	/// <summary>
	/// Class which is used as a heap element in the internal trees inside a Fibonacci heap.
	/// </summary>
	/// <typeparam name="TElement">The element contained inside this node</typeparam>
	internal class FibonacciHeapNode<TElement>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FibonacciHeapNode&lt;TElement&gt;"/> class.
		/// </summary>
		/// <param name="contents">The contents.</param>
		public FibonacciHeapNode(TElement contents)
		{
			this.Contents = contents;
			this.Children = new LinkedBucketList<FibonacciHeapNode<TElement>>();
		}


		/// <summary>
		/// Cuts the child passed in from this node. If this node isn't a root, it will be marked after this operation.
		/// </summary>
		/// <param name="child">The child.</param>
		public void CutChild(FibonacciHeapNode<TElement> child)
		{
			bool removeSucceeded = this.Children.Remove(child);

			if(!removeSucceeded)
			{
				throw new ArgumentException("The passed in child isn't a child of the node it has to be cut from.", "child");
			}
			child.Parent = null;
			if(this.Parent != null)
			{
				// mark ourselves. This is an essential part of the algorithm: if a child is cut from a node, the node has to be marked.
				this.IsMarked = true;
			}
		}


		/// <summary>
		/// Adds the tree specified by its head 'child' as a child of this node.
		/// </summary>
		/// <param name="child">The child.</param>
		public void AddChild(FibonacciHeapNode<TElement> child)
		{
			this.Children.AppendTail(child);
			// make this node its parent.
			child.CutFromParent();
			child.Parent = this;
		}


		/// <summary>
		/// Becomes the root of a tree in the heap. Becoming a root means it has to be unmarked and the parent has to be null.
		/// </summary>
		public void BecomeTreeRoot()
		{
			CutFromParent();

			// root nodes can't be marked.
			this.IsMarked = false;
		}


		/// <summary>
		/// Cuts this child from its parent. Calls into parent to remove this node as its child. 
		/// </summary>
		public void CutFromParent()
		{
			if(this.Parent != null)
			{
				this.Parent.CutChild(this);
			}
		}


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			string toReturn = (object)this.Contents == null ? "<null>" : this.Contents.ToString();
			return "Contents: " + toReturn;
		}
	

		#region Class Property Declarations
		/// <summary>
		/// Gets the degree of the tree with this node as the root. The degree is the number of children.
		/// </summary>
		public int Degree
		{
			get { return this.Children.Count; }
		}


		/// <summary>
		/// Gets or sets the parent heap element of this element
		/// </summary>
		public FibonacciHeapNode<TElement> Parent { get; set; }

		/// <summary>
		/// Gets or sets the children of this heap element. Children are stored in a doubly linked list.
		/// </summary>
		public LinkedBucketList<FibonacciHeapNode<TElement>> Children { get; private set; }

		/// <summary>
		/// Gets the contents of this heap element
		/// </summary>
		public TElement Contents { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is marked.
		/// </summary>
		public bool IsMarked { get; set; }
		#endregion
	}
}
