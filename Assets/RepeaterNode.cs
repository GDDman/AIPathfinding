using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeaterNode: BNode {

	public RepeaterNode(BNode p) : base(p) {
	}

	public override void update() {
		foreach (BNode child in children) {
			if (child.status == Status.SUCESS || child.status == Status.FAILED) {
				status = Status.DORMANT;
				refresh ();
			}
		}
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
