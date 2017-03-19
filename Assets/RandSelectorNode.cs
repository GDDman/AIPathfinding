using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

		if (child.status == Status.SUCESS) {
			status = Status.SUCESS;
		} else if (child.status == Status.FAILED) {
			status = Status.FAILED;
		}

	}

	public override BNode getNext() {

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
