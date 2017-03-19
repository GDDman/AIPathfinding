using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BNode {

	public ArrayList children = new ArrayList();
	public BNode parent;
	public Astar.Actiontype actiontype;
	public Status status;

	public enum Status {
		DORMANT, 
		FAILED,
		RUNNING,
		SUCESS
	}

	public BNode (BNode p) {
		status = Status.DORMANT;
		actiontype = Astar.Actiontype.NULL;
		parent = p;
		if (parent != null) p.children.Add (this);
	}
		
	public abstract void update ();

	public void refresh() {
		status = Status.DORMANT;
		foreach (BNode child in children) {
			child.refresh ();
		}
	}

	public abstract BNode getNext();
		
}
