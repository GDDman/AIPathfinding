using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IHeapItem<Node> {

	public int cost;
	public int tcost;
	public Vector2 index;
	public Node parent;
	int heapIndex;
	public int time;

	public Node (int c, int i, int j, int h, int t) {
		cost = c;
		tcost = c + h;
		index = new Vector2(i, j);
		time = t;
		// outside of bounds for null value
		parent = null;
	}

	public static bool Equals(Node n1, Node n2) {
		if (n1.index == n2.index && n1.time == n2.time)
			return true;
		else
			return false;
	}

	public int CompareTo(Node n) {
		if (n == null)
			return 0;
		return n.tcost.CompareTo (this.tcost);
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}


		
}