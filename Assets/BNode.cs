using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Superclass of all nodes in the behaviour tree. They all need an update function, and a recursive reset, as well as a way to get the next child node
public abstract class BNode {

	public ArrayList children = new ArrayList();
	public BNode parent;

	// If the node is a leaf, it has an action (defined by the AI). If it is intermediary this is Astar.Actiontype.NULL
	public Astar.Actiontype actiontype;
	public Status status;

	// The status of the node in the tree
	public enum Status {
		DORMANT, 
		FAILED,
		RUNNING,
		SUCESS
	}

	// Takes in the parent node
	public BNode (BNode p) {
		status = Status.DORMANT;
		actiontype = Astar.Actiontype.NULL;
		parent = p;
		if (parent != null) p.children.Add (this);
	}
		
	public abstract void update ();

	// Recursively resets the status of this BNode and all subtree BNodes to dormant
	public void refresh() {
		status = Status.DORMANT;
		foreach (BNode child in children) {
			child.refresh ();
		}
	}

	public abstract BNode getNext();
		
}
