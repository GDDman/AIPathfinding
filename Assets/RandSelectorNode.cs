using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// randomly selects a child to execute
public class RandSelectorNode: BNode {

	BNode child;

	public RandSelectorNode(BNode p) : base(p) {
		child = null;
	}

	public override void update() {

		if (child == null) {
			status = Status.SUCESS;
			return;
		}

		// return result of child and pick a new one
		if (child.status == Status.SUCESS) {
			status = Status.SUCESS;
			if (children.Count > 0) {
				int rand = Random.Range (0, children.Count);
				child = (BNode) children [rand];
			} 
		} else if (child.status == Status.FAILED) {
			status = Status.FAILED;
			if (children.Count > 0) {
				int rand = Random.Range (0, children.Count);
				child = (BNode) children [rand];
			}
		}

	}

	public override BNode getNext() {

		// select child when called
		if (child == null) {
			if (children.Count > 0) {
				int rand = Random.Range (0, children.Count);
				child = (BNode) children [rand];
			}
		} 

		if (child == null)
			return null;
		if (child.status == Status.DORMANT) {
			return child;
		}
		return null;
	}
}
