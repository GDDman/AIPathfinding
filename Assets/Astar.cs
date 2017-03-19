using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

	int[,] grid;
	int currentx;
	int currenty;
	bool pathing;
	BehaviourTree plan;
	int randprof;
	int randplaque;
	Vector2[] profcoords = new Vector2[6];
	Vector2[] plaquecoords = new Vector2[6];
	LinkedList<int> memory = new LinkedList<int>();

	public enum Actiontype {
		NULL,
		PATHPROF,
		PATHRAND,
		PATHPLAQUE,
		CHECKDATA,
		CHECKPLAQUE,
		PICKPROF,
		FINISH
	}

	public int initialx = 0;
	public int initialy = 0;

	int destx = 0;
	int desty = 0;

	Heap<Node> nodelist = new Heap<Node>(640);
	HashSet<Vector2> closed = new HashSet<Vector2>();
	LinkedList<Vector2> path = new LinkedList<Vector2>();

	// Use this for initialization
	void Start () {

		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		grid = m.getGrid ();

		currentx = initialx;
		currenty = initialy;

		plan = new BehaviourTree ();

		pathing = false;
		randprof = Random.Range (0, 6);
		System.Console.WriteLine ("RANDPROF " + (randprof + 1)); 
		randplaque = 0;

		profcoords[0] = new Vector2 (2, 8);
		profcoords[1] = new Vector2(2, 21);
		profcoords[2] = new Vector2(8, 29);
		profcoords[3] = new Vector2(17, 23);
		profcoords[4] = new Vector2(17, 10);
		profcoords[5] = new Vector2(11, 2);

		plaquecoords[0] = new Vector2 (4, 9);
		plaquecoords[1] = new Vector2(4, 22);
		plaquecoords[2] = new Vector2(9, 27);
		plaquecoords[3] = new Vector2(15, 22);
		plaquecoords[4] = new Vector2(15, 9);
		plaquecoords[5] = new Vector2(10, 4);


	}

	// Update is called once per frame
	void Update () {

		plan.update ();
		if (plan.current.actiontype == Actiontype.NULL)
			return;

		switch (plan.current.actiontype) 
		{
		case Actiontype.FINISH:

			plan.current.status = BNode.Status.SUCESS;
			System.Console.WriteLine ("FINISHED");
			break;
		
		case Actiontype.CHECKDATA:
			
			if (memory.Contains (randprof)) {
				System.Console.WriteLine ("PROF IN MEMORY");

				string s = "";
				foreach (int m in memory) {
					s += (m+1) + " ";
				}
				System.Console.WriteLine ("MEM: " + s);

				plan.current.status = BNode.Status.SUCESS;
			} else {
				System.Console.WriteLine ("PROF NOT IN MEMORY");

				string s = "";
				foreach (int m in memory) {
					s += (m+1) + " ";
				}
				System.Console.WriteLine ("MEM: " + s);

				plan.current.status = BNode.Status.FAILED;
			}

			break;
		
		case Actiontype.PATHPLAQUE:

			randplaque = Random.Range (0, 6);
			while (memory.Contains(randplaque)) {
				randplaque = Random.Range (0, 6);
			}
			Vector2 goal = plaquecoords [randplaque];
			destx = (int) goal.x;
			desty = (int) goal.y;
			System.Console.WriteLine ("PATH TO PLAQUE " + (randplaque + 1) + " " + destx + " " + desty);
			decidePath ();
			plan.current.status = BNode.Status.SUCESS;

			break;

		case Actiontype.CHECKPLAQUE:

			if (randplaque == randprof) {
				System.Console.WriteLine ("PLAQUE RIGHT");
				plan.current.status = BNode.Status.SUCESS;
			} else {
				System.Console.WriteLine ("PLAQUE WRONG");
				plan.current.status = BNode.Status.FAILED;
			}

			updateMemory (randplaque);

			break;

		case Actiontype.PATHPROF:
			
			Vector2 goal2 = profcoords [randprof];
			destx = (int) goal2.x;
			desty = (int) goal2.y;
			System.Console.WriteLine ("PATH TO PROF " + destx + " " + desty);
			decidePath ();
			plan.current.status = BNode.Status.SUCESS;
			updateMemory (randprof);

			break;

		case Actiontype.PATHRAND:

			destx = Random.Range (0, 20);
			desty = Random.Range (0, 32);
			while (grid [destx, desty] != 0) {
				destx = Random.Range (0, 20);
				desty = Random.Range (0, 32);
			}
			System.Console.WriteLine ("PATH TO RANDOM " + destx + " " + desty);
			decidePath ();
			plan.current.status = BNode.Status.SUCESS;

			break;
		
		case Actiontype.PICKPROF:

			int newprof = Random.Range (0, 6);
			while (newprof == randprof) {
				newprof = Random.Range (0, 6);
			}
			randprof = newprof;
			System.Console.WriteLine ("NEW PROF " + (randprof + 1));
			plan.current.status = BNode.Status.SUCESS;

			break;

		default:
			break;
		}


	}

	void updateMemory(int r) {
		memory.Remove (r);
		memory.AddFirst (r);
		if (memory.Count > 4) 
			memory.RemoveLast ();
	}

	ArrayList getChildren(Node node) {

		Vector2 goal = new Vector2 (destx, desty);
		ArrayList children = new ArrayList ();
		int x = (int) node.index.x;
		int y = (int) node.index.y;

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 && j == 0) continue;
				if (x + i < 0 || x + i >= 20)
					continue;
				if (y + j < 0 || y + j >= 32)
					continue;
				// corner 
				if (i != 0 && j != 0) {
					if (grid [(x + i), (y + j)] == 0 && grid [x, (y + j)] == 0 && grid [(x + i), y] == 0) {
						int hdist = manhattenDist (new Vector2 (x + i, y + j), goal);
						Node n = new Node (node.cost + 1, x + i, y + j, hdist);
						if (node.parent != null && Node.Equals(n, node.parent))
							continue;
						if (closed.Contains(n.index))
							continue;
						n.parent = node;
						if (!nodelist.Contains (n)) {
							children.Add (n);
						} else {
							nodelist.UpdateItem (n);
						}
					}
				} else {
					if (grid [x + i, y + j] == 0) {
						int hdist = manhattenDist (new Vector2 (x + i, y + j), goal);
						Node n = new Node (node.cost + 1, x + i, y + j, hdist);
						if (node.parent != null && Node.Equals(n, node.parent))
							continue;
						if (closed.Contains(n.index))
							continue;
						n.parent = node;
						children.Add (n);
					}
				}
			}
		}
		return children;
	}

	public void decidePath() {

		System.Console.WriteLine (currentx + " " + currenty);
		Vector2 goal = new Vector2 (destx, desty);
		pathing = true;
		nodelist = new Heap<Node>(640);
		closed = new HashSet<Vector2> ();
		nodelist.Add (new Node (0, currentx, currenty, 0));

		Node end = null;
		while (nodelist.Count > 0) {
			Node n = getBest ();
			if (checkGoal (n, goal)) {
				end = n;
				break;
			}
			end = n;
		}
		path = new LinkedList<Vector2>();
		path.AddLast(end.index);
		Node curr = end;
		while ((curr = curr.parent) != null) {
			path.AddFirst (curr.index);
		}
			
		foreach (Vector2 v in path) {
			System.Console.WriteLine (v.ToString());
		}

		Vector2[] array = new Vector2[path.Count];
		path.CopyTo (array, 0);

		Vector2 newpos = array[array.Length - 1];
		currentx = (int) newpos.x;
		currenty = (int) newpos.y;


	}

	bool checkGoal(Node n, Vector2 goal) {
		Vector2 i = n.index;
		if (new Vector2 () == (i - goal))
			return true;
		return false;
	}

	Node getBest() {
		Node n = nodelist.RemoveFirst ();
		closed.Add (n.index);
		ArrayList children = getChildren (n);
		foreach (Node child in children) {
			nodelist.Add (child);
		}
		return n;
	}

	// 4 way manhatten dist
	int manhattenDist(Vector2 from, Vector2 to) {

		Vector2 dist = from - to;
		int x = (int) Mathf.Abs (dist.x);
		int y = (int) Mathf.Abs (dist.y);
		return (x > y) ? x : y;
	}

}
