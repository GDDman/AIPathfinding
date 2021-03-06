﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Leaf node describing an action
public class ActionNode: BNode {

	public ActionNode(BNode p, Astar.Actiontype t) : base(p) {
		actiontype = t;
	}

	// Usually has no children but possibly could
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