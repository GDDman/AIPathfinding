using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Node that returns a sucess if all of its children are sucessful in a certain order
public class SequenceNode: BNode {

	public SequenceNode(BNode p) : base(p) {
	}

	public override void update() {
		bool sucess = true;
		bool notfinished = false;
		foreach (BNode child in children) {
			if (child.status == Status.DORMANT) {
				status = Status.DORMANT;
				notfinished = true;
			}
			if (child.status == Status.FAILED) {
				sucess = false;
				status = Status.FAILED;
				return;
			}
		}
		if (notfinished) {
			return;
		}
		status = Status.SUCESS;
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