using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode: BNode {

	public ActionNode(BNode p, Astar.Actiontype t) : base(p) {
		actiontype = t;
	}

	public override BNode getNext() {
		foreach (BNode child in children) {
			if (child.status == BNode.Status.DORMANT) {
				return child;
			}
		}
		return null;
	}

	public override void update() {}
		
}