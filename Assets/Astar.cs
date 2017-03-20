using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

	int[,] grid;
	public int currentx;
	public int currenty;
	bool pathing;

	bool idling;
	int idlingcounter;
	float plaqueidlemax = 1f;
	float profidlemax = 0.8f;
	float randidlemax = 0f;

	int pathindex;
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

	Heap<Node> nodelist = new Heap<Node>(1280);
	HashSet<Vector2> closed = new HashSet<Vector2>();
	List<Vector2> path = new List<Vector2>();

	// Use this for initialization
	void Start () {

		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		grid = m.getGrid ();

		currentx = initialx;
		currenty = initialy;

		plan = new BehaviourTree ();

		pathing = false;
		idling = false;
		pathindex = 0;
		idlingcounter = 0;
		randprof = Random.Range (0, 6);
		randplaque = 0;
		randidlemax = Random.Range (0.5f, 2f);

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

		updatePosition ();

	}

	void updatePosition() {

		float x = (((float)currenty)/2 - 8) + (float)0.25;  
		float y = (-((float)currentx)/2 + 5) - (float)0.25;
		this.transform.position = new Vector3(x, 0.65f, y);
	}

	Vector2 getPoint(Vector2 positions) {
		float x = (((float)positions.y)/2 - 8) + (float)0.25;  
		float y = (-((float)positions.x)/2 + 5) - (float)0.25;
		return new Vector2(x, y);
	}

	// Update is called once per frame
	public void updateStudent (Game game, float time) {

		plan.update ();
		if (plan.current.actiontype == Actiontype.NULL)
			return;

		switch (plan.current.actiontype) 
		{
		case Actiontype.FINISH:

			plan.current.status = BNode.Status.SUCESS;
			break;
		
		case Actiontype.CHECKDATA:
			
			if (memory.Contains (randprof)) {
				plan.current.status = BNode.Status.SUCESS;
			} else {
				plan.current.status = BNode.Status.FAILED;
			}

			break;
		
		case Actiontype.PATHPLAQUE:

			if (idling) {
				idlingcounter++;
				int ticks = (int) (plaqueidlemax)*60;
				if (idlingcounter > ticks) {
					idling = false;
					idlingcounter = 0;
					plan.current.status = BNode.Status.SUCESS;
				}
				break;
			}

			if (!pathing) {
				if (game.tick == 1) {
					randplaque = Random.Range (0, 6);
					while (memory.Contains (randplaque)) {
						randplaque = Random.Range (0, 6);
					}
					pathing = true;
					Vector2 goal = plaquecoords [randplaque];
					destx = (int) goal.x;
					desty = (int) goal.y;
					decidePath ();
					pathindex = 0;
				}
			}
			if (pathing) {

				if (path.Count <= 1) {
					pathing = false;
					pathindex = 0;
					idling = true;
					break;
				}
					
				if (game.tick != game.maxtick) {

					Vector2 pos = path [pathindex];
					Vector2 newpos = path [pathindex + 1];
					Vector2 v = getPoint(newpos) - getPoint(pos);
					Vector3 v3 = new Vector3 (v.x, 0, v.y);
					float f = 1/(float)(game.maxtick);
					v3 = Vector3.Scale (new Vector3 (f, 0, f), v3);
					transform.position = transform.position + v3;

				} else {

					Vector2 newpos = path [++pathindex];
					currentx = (int) newpos.x;
					currenty = (int) newpos.y;
					updatePosition ();
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
					}
				}

			}
			break;

		case Actiontype.CHECKPLAQUE:

			if (randplaque == randprof) {
				plan.current.status = BNode.Status.SUCESS;
			} else {
				plan.current.status = BNode.Status.FAILED;
			}

			updateMemory (randplaque);

			break;

		case Actiontype.PATHPROF:

			if (idling) {
				idlingcounter++;
				int ticks = (int)(profidlemax)*60;
				if (idlingcounter > ticks) {
					idling = false;
					idlingcounter = 0;
					plan.current.status = BNode.Status.SUCESS;
				}
				break;
			}

			if (!pathing) {
				if (game.tick == 1) {
					Vector2 goal2 = profcoords [randprof];
					destx = (int) goal2.x;
					desty = (int) goal2.y;
					decidePath ();
					pathing = true;
					pathindex = 0;
				}
			}
			if (pathing) {

				if (path.Count <= 1) {
					pathing = false;
					pathindex = 0;
					idling = true;
					break;
				}

				if (game.tick != game.maxtick) {

					Vector2 pos = path [pathindex];
					Vector2 newpos = path [pathindex + 1];
					Vector2 v = getPoint(newpos) - getPoint(pos);
					Vector3 v3 = new Vector3 (v.x, 0, v.y);
					float f = 1/(float)(game.maxtick);
					v3 = Vector3.Scale (new Vector3 (f, 0, f), v3);
					transform.position = transform.position + v3;

				} else {

					Vector2 newpos = path [++pathindex];
					currentx = (int) newpos.x;
					currenty = (int) newpos.y;
					updatePosition ();
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
					}
				}

			}
			break;

		case Actiontype.PATHRAND:

			if (idling) {
				idlingcounter++;
				int ticks = (int)(randidlemax)*60;
				if (idlingcounter > ticks) {
					idling = false;
					idlingcounter = 0;
					plan.current.status = BNode.Status.SUCESS;
					randidlemax = Random.Range (0.5f, 2f);
				}
				break;
			}

			if (!pathing) {
				if (game.tick == 1) {

					destx = Random.Range (0, 20);
					desty = Random.Range (0, 32);
					while (grid [destx, desty] != 0) {
						destx = Random.Range (0, 20);
						desty = Random.Range (0, 32);
					}
					decidePath ();
					pathing = true;
					pathindex = 0;
				}
			}
			if (pathing) {

				if (path.Count <= 1) {
					pathing = false;
					pathindex = 0;
					idling = true;
					break;
				}

				if (game.tick != game.maxtick) {

					Vector2 pos = path [pathindex];
					Vector2 newpos = path [pathindex + 1];
					Vector2 v = getPoint(newpos) - getPoint(pos);
					Vector3 v3 = new Vector3 (v.x, 0, v.y);
					float f = 1/(float)(game.maxtick);
					v3 = Vector3.Scale (new Vector3 (f, 0, f), v3);
					transform.position = transform.position + v3;

				} else {

					Vector2 newpos = path [++pathindex];
					currentx = (int) newpos.x;
					currenty = (int) newpos.y;
					updatePosition ();
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
					}
				}

			}
			break;
		
		case Actiontype.PICKPROF:

			int newprof = Random.Range (0, 6);
			while (newprof == randprof) {
				newprof = Random.Range (0, 6);
			}
			randprof = newprof;
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

		Vector2 goal = new Vector2 (destx, desty);
		pathing = true;
		nodelist = new Heap<Node>(1280);
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
		path = new List<Vector2>();
		path.Insert(path.Count, end.index);
		Node curr = end;
		while ((curr = curr.parent) != null) {
			path.Insert (0, curr.index);
		}

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
