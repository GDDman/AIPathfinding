using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Node that returns sucess if any of its children return a sucess (has a specific order)
public class SelectorNode: BNode {

	public SelectorNode(BNode p) : base(p) {
	}

	public override void update() {
		bool notfinished = false;
		foreach (BNode child in children) {
			if (child.status == Status.DORMANT) {
				status = Status.DORMANT;
				notfinished = true;
			}
			if (child.status == Status.SUCESS) {
				status = Status.SUCESS;
				return;
			}
		}
		if (notfinished) {
			return;
		}
		status = Status.FAILED;
	}

	public override BNode getNext() {
		foreach (BNode child in children) {
			if (child.status == BNode.Status.DORMANT) {
				return child;
			}
		}
		return null;
	}
}
