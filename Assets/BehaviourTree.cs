using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tree consisting of Bnodes
public class BehaviourTree {

	public BNode current;
	public bool finished;

	public BehaviourTree() {

		finished = false;

		// Hard coding of the specific behaviour tree for a student
		// outlined in more detail in the provide diagram

		// intermediate nodes take a parent, action leaf nodes take in the type of action they are

		BNode root = new RepeaterNode (null);

		BNode getadvising = new SequenceNode (root);

		BNode branch = new SelectorNode(getadvising); 

		BNode checkdata = new ActionNode (branch, Astar.Actiontype.CHECKDATA);

		BNode randomplaque = new SequenceNode (branch);
		BNode pathplaque = new ActionNode (randomplaque, Astar.Actiontype.PATHPLAQUE);
		BNode checkplaque = new ActionNode (randomplaque, Astar.Actiontype.CHECKPLAQUE); 

		BNode pathprof3 = new ActionNode(getadvising, Astar.Actiontype.PATHPROF);
		BNode pickprof3 = new ActionNode (getadvising, Astar.Actiontype.PICKPROF);

		BNode randbranch = new RandSelectorNode (getadvising);
		BNode finish = new ActionNode (randbranch, Astar.Actiontype.FINISH);
		BNode pathrand = new ActionNode(randbranch, Astar.Actiontype.PATHRAND);

		// current is the node being looked at in the tree at any time
		current = root;

	}

	public void update() {

		if (finished) return;

		// recursively find the first leaf and set it to current
		if (current.status == BNode.Status.DORMANT) {
			current.status = BNode.Status.RUNNING;
			if (current.getNext () != null) {
				current = current.getNext ();
				update ();
			}
		} else if (current.status == BNode.Status.SUCESS || current.status == BNode.Status.FAILED) {
			// if the current node returns a result, pass to the parent
			if (current.parent != null) {
				current = current.parent;
				update ();
			} else {
				finished = true;
			}
		} else {
			current.update ();
		}
	}
		
}
