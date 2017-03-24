using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

	static int searchdepth = 320;
	int currentx;
	int currenty;
	int label;
	bool pathing;
	bool finishedpath;

	bool changepath;

	bool idling;
	float plaqueidlemax = 0.7f;
	float profidlemax = 0.8f;

	float lastheur = 1000f;
	float frusttime = 0f;
	float accumtime = 0f;
	MeshRenderer render;
	Material purple;
	Material yellow;
	Material red;

	Vector2 randpath = new Vector2 ();
	int pathindex;
	BehaviourTree plan;
	int randprof;
	int randplaque;
	Vector2[] profcoords = new Vector2[6];
	Vector2[] plaquecoords = new Vector2[6];
	LinkedList<int> memory = new LinkedList<int>();

	int timecounter = 0;
	int idlingcounter = 0;
	bool ticksleft = false;

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

	Heap<Node> nodelist = new Heap<Node>(searchdepth);
	HashSet<Vector2> closed = new HashSet<Vector2>();
	List<Vector2> path = new List<Vector2>();
	Game game;

	int idletime = 0;
	int randidleticks = 0;
	int randticksleft = 0;

	// Use this for initialization
	void Start () {

		currentx = initialx;
		currenty = initialy;

		plan = new BehaviourTree ();
		game = new Game ();

		pathing = false;
		idling = false;
		pathindex = 0;
		randprof = Random.Range (0, 6);
		randplaque = 0;

		profcoords[0] = new Vector2 (2, 7);
		profcoords[1] = new Vector2(2, 20);
		profcoords[2] = new Vector2(7, 29);
		profcoords[3] = new Vector2(17, 24);
		profcoords[4] = new Vector2(17, 11);
		profcoords[5] = new Vector2(12, 2);

		plaquecoords[0] = new Vector2 (4, 9);
		plaquecoords[1] = new Vector2(4, 22);
		plaquecoords[2] = new Vector2(9, 27);
		plaquecoords[3] = new Vector2(15, 22);
		plaquecoords[4] = new Vector2(15, 9);
		plaquecoords[5] = new Vector2(10, 4);

		updatePosition ();
		finishedpath = false;
		changepath = true;

		render = (MeshRenderer)transform.GetComponent<MeshRenderer> ();
		purple = Resources.Load("Materials/Purple", typeof(Material)) as Material;
		yellow = Resources.Load("Materials/Yellow", typeof(Material)) as Material;
		red = Resources.Load("Materials/Red", typeof(Material)) as Material;
		render.material = purple;
	}

	public bool getIdling() {
		return idling;
	}

	public void setGame(Game g) {
		game = g;
	}

	public int getlabel() {
		return label;
	}

	public void setlabel(int i) {
		label = i;
	}

	public int getcurrentx() {
		return currentx;
	}

	public int getcurrenty() {
		return currenty;
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

	public float getDist(Vector2 from, Vector2 to) {
		Vector3 vec = to - from;
		return vec.magnitude;
	}

	public float dist() {
		Vector2 from = new Vector2 (currentx, currenty);
		Vector2 to = new Vector2 (destx, desty);
		Vector3 vec = to - from;
		return vec.magnitude;
	}

	public int updateStudent (float time) {

		accumtime += time;
		frusttime += time;

		if (finishedpath) {
			lastheur = 1000f;
			frusttime = 0f;
			render.material = purple;
		}
		else if (accumtime > 0.2f) {
			accumtime = 0f;
			float currentheur = dist ();
			if (currentheur <= lastheur) {
				frusttime = 0f;
				lastheur = currentheur;
			}
			if (frusttime > 0.1f && frusttime <= 3f) {
				render.material = yellow;
			} else if (frusttime > 3f) {
				render.material = red;
			} else {
				render.material = purple;
			}
		}
			
		int tolast = 0;

		plan.update ();
		if (plan.current.actiontype == Actiontype.NULL)
			return 0;

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
			tolast = 1;
			plan.update ();
			if (plan.current.actiontype == Actiontype.PATHPLAQUE) {
				goto case Actiontype.PATHPLAQUE;
			}
			break;
		
		case Actiontype.PATHPLAQUE:
			
			if (idling) {
				idling = false;
				idlingcounter = 0;
				idletime = 0;
				plan.current.status = BNode.Status.SUCESS;
				changepath = true;
				break;
			}

			if (!pathing) {
				if (game.tick == 1) {
					if (changepath) {
						randplaque = Random.Range (0, 6);
						while (memory.Contains (randplaque)) {
							randplaque = Random.Range (0, 6);
						}
					}
					Vector2 goal = plaquecoords [randplaque];
					destx = (int) goal.x;
					desty = (int) goal.y;
					tolast = 1;
					try {
						finishedpath = decidePath (false);
					}
					catch (System.Exception e) {
						finishedpath = false;
					}
					if (path.Count <= 1) {
						idling = true;
						pathindex = 0;
						idlingcounter = 100000;
						tolast = 1;
						break;
					}
					if (!finishedpath) {
						changepath = false;
					} else {
						for (int i = -1; i <= 1; i++) {
							for (int j = -1; j <= 1; j++) {
								if (i == 0 || j == 0) {
									Vector2 last = path [path.Count - 1];
									if (((int)last.x + i) >= 0 && ((int)last.x + i) < 20 && ((int)last.y + j) >= 0 && ((int)last.y + j) < 32) {
										game.grids [path.Count - 1].add ((int)last.x + i, (int)last.y + j, label);
									}
								}
							}
						}
					}
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
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						break;
					}
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						if (finishedpath) {
							idling = true;
						}
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
			tolast = 1;
			plan.update ();
			if (plan.current.actiontype == Actiontype.PATHPROF) {
				goto case Actiontype.PATHPROF;
			}
			break;

		case Actiontype.PATHPROF:
			
			if (idling) {
				idling = false;
				idlingcounter = 0;
				idletime = 0;
				plan.current.status = BNode.Status.SUCESS;
				changepath = true;
				tolast = 1;
				break;
			}

			if (!pathing) {
				
				if (game.tick == 1) {
					Vector2 goal2 = profcoords [randprof];
					destx = (int)goal2.x;
					desty = (int)goal2.y;
					tolast = 1;
					try {
						finishedpath = decidePath (true);
					} catch (System.Exception e) {
						finishedpath = false;
					}
					if (path.Count <= 1) {
						idling = true;
						pathindex = 0;
						idlingcounter = 100000;
						tolast = 1;
						break;
					}
					if (finishedpath) {
						for (int i = -1; i <= 1; i++) {
							for (int j = -1; j <= 1; j++) {
								if (i != 0 && j != 0) {
									Vector2 last = path [path.Count - 1];
									if (((int)last.x + i) >= 0 && ((int)last.x + i) < 20 && ((int)last.y + j) >= 0 && ((int)last.y + j) < 32) {
										game.grids [path.Count - 1].add ((int)last.x + i, (int)last.y + j, label);
									}
								}
							}
						}
					}
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
					Vector2 v = getPoint (newpos) - getPoint (pos);
					Vector3 v3 = new Vector3 (v.x, 0, v.y);
					float f = 1 / (float)(game.maxtick);
					v3 = Vector3.Scale (new Vector3 (f, 0, f), v3);
					transform.position = transform.position + v3;

				} else {

					Vector2 newpos = path [++pathindex];
					currentx = (int)newpos.x;
					currenty = (int)newpos.y;
					updatePosition ();
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						break;
					}
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int)path [pathindex].x;
						currenty = (int)path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						if (finishedpath) {
							idling = true;
						}
					}
				}

			}
			break;

		case Actiontype.PATHRAND:

			if (idling) {

				randidleticks = Random.Range(3, 10);
				int ticks = game.maxtick*(randidleticks);

				idlingcounter++;
				if (idlingcounter >= ticks) {
					idling = false;
					idlingcounter = 0;
					plan.current.status = BNode.Status.SUCESS;
					changepath = true;
					tolast = 1;
					break;
				}

				break;
			}

			if (!pathing) {
				if (game.tick == 1) {
					if (changepath) {
						randpath.x = Random.Range (0, 20);
						randpath.y = Random.Range (0, 32);
						while (game.originalgrid [(int)randpath.x, (int)randpath.y] != 0) {
							randpath.x = Random.Range (0, 20);
							randpath.y = Random.Range (0, 32);
						}
					}
					destx = (int)randpath.x;
					desty = (int)randpath.y;
					try {
						finishedpath = decidePath (false);
					}
					catch (System.Exception e) {
						finishedpath = false;
					}
					if (path.Count <= 1) {
						idling = true;
						pathindex = 0;
						idlingcounter = 100000;
						tolast = 1;
						break;
					}
					if(!finishedpath) {
						changepath = false;
					} else {
						for (int i = -1; i <= 1; i++) {
							for (int j = -1; j <= 1; j++) {
								if (i != 0 && j != 0) {
									Vector2 last = path [path.Count - 1];
									if (((int)last.x + i) >= 0 && ((int)last.x + i) < 20 && ((int)last.y + j) >= 0 && ((int)last.y + j) < 32) {
										game.grids [path.Count - 1].add ((int)last.x + i, (int)last.y + j, label);
									}
								}
							}
						}
					}
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
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						break;
					}
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int)path [pathindex].x;
						currenty = (int)path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						if (finishedpath) {
							idling = true;
						}
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

		return tolast;
	}

	void updateMemory(int r) {
		memory.Remove (r);
		memory.AddFirst (r);
		if (memory.Count > 4) 
			memory.RemoveLast ();
	}

	ArrayList getChildren(Node node, bool prof) {

		Vector2 goal = new Vector2 (destx, desty);
		ArrayList children = new ArrayList ();

		Grid[] grids = game.grids;
		Grid og = new Grid ();

		int x = (int) node.index.x;
		int y = (int) node.index.y;

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 && j == 0) continue;
				if (x + i < 0 || x + i >= 20)
					continue;
				if (y + j < 0 || y + j >= 32)
					continue;
				if (i != 0 && j != 0) {
					// corner 
					Grid g = null;
					if (timecounter + 1 < game.timeslices) {
						g = grids [timecounter + 1];
					} else {
						g = og;
					}
					if ((g.getgrid((x + i), (y + j)) == 0 || g.getgrid((x + i), (y + j)) == label) && g.getgrid(x, (y + j)) == 0 && g.getgrid((x + i), y) == 0) {
						int hdist = manhattenDist (new Vector2 (x + i, y + j), goal);
						Node n = new Node (node.cost + 1, x + i, y + j, hdist, timecounter + 1);
						if (closed.Contains(n.index)) 
							continue;
						n.parent = node;
						if (!nodelist.Contains (n)) {
							children.Add (n);
						}
					}
				} else {
					// sides
					Grid g = null;
					if (timecounter + 1 < game.timeslices) {
						g = grids [timecounter + 1];
					} else {
						g = og;
					}
					if (g.getgrid(x + i, y + j) == 0 || g.getgrid(x + i, y + j) == label || g.getgrid(x + i, y + j) == 2) {
						if (!prof) {
							if (g.getgrid (x + i, y + j) == 2) {
								continue;
							}
						} else {
							if ((g.getgrid (x + i, y + j) == 2) && ((destx != x + i) || (desty != y + j))) {
								continue;
							}
						}
						int hdist = manhattenDist (new Vector2 (x + i, y + j), goal);
						Node n = new Node (node.cost + 1, x + i, y + j, hdist, timecounter + 1);
						if (closed.Contains(n.index))
							continue;
						n.parent = node;
						if (!nodelist.Contains (n)) {
							children.Add (n);
						}
					}
				}
			}
		}
		return children;
	}

	public bool decidePath(bool prof) {


		Vector2 goal = new Vector2 (destx, desty);
		pathing = true;
		nodelist = new Heap<Node>(searchdepth);
		closed = new HashSet<Vector2> ();
		nodelist.Add (new Node (0, currentx, currenty, 0, 0));

		Node end = null;
		while (nodelist.Count > 0) {

			Node n = nodelist.RemoveFirst ();
			timecounter = n.time;

			if (timecounter >= 1) {
				if (timecounter < game.timeslices) {
					if (game.grids [timecounter - 1].getgrid ((int)n.index.x, (int)n.index.y) >= 3) {
						int num = game.grids [timecounter - 1].getgrid ((int)n.index.x, (int)n.index.y);
						Node p = n.parent;
						if (num == game.grids [timecounter].getgrid ((int)p.index.x, (int)p.index.y)) {
							continue;
						}
					}
				}
			} 

			closed.Add (n.index);

			ArrayList children = getChildren (n, prof);
			foreach (Node child in children) {
				try { 
					nodelist.Add (child);
				}
				catch (System.Exception e) {
					foreach (Node tempnode in nodelist.items) {
						if (tempnode.time == n.time && getDist (tempnode.index, new Vector2 (destx, desty)) <= getDist (n.index, new Vector2 (destx, desty))) {
							n = tempnode;
						}
					}
					end = n;
					break;
				}
			}

			if (checkGoal (n, goal)) {
				end = n;
				break;
			}
			end = n;
		}

		path = new List<Vector2>();
		path.Insert (path.Count, end.index);
		Node curr = end;
		while ((curr = curr.parent) != null) {
			path.Insert (0, curr.index);
		}

		path.Add (path [path.Count - 1]);

		if (path.Count >= game.timeslices) {
			path = path.GetRange (0, game.timeslices);
		}

		if (path [path.Count - 1] == goal && path[path.Count - 2] == goal) {
			if (prof) {
				foreach (Vector2 v in path) {
					print (v);
				}
				print ("");
				path.RemoveAt (path.Count - 1);
				path.RemoveAt (path.Count - 1);
				path.Add (path[path.Count - 1]);
				foreach (Vector2 v in path) {
					print (v);
				}
				print ("");
			}
			for (int i = 0; i < path.Count; i++) {
				game.grids [i].add ((int)path[i].x, (int)path[i].y, label);
			}

			return true;
		} 

		for (int i = 0; i < path.Count; i++) {
			game.grids [i].add ((int)path[i].x, (int)path[i].y, label);
		}
		return false;

	}

	bool checkGoal(Node n, Vector2 goal) {
		Vector2 i = n.index;
		if (new Vector2 () == (i - goal))
			return true;
		return false;
	}
		
	// 8 way manhatten dist
	int manhattenDist(Vector2 from, Vector2 to) {

		Vector2 dist = from - to;
		int x = (int) Mathf.Abs (dist.x);
		int y = (int) Mathf.Abs (dist.y);
		return (x > y) ? x : y;
	}
		
	public override string ToString() {
		string s = "prof: " + randprof + "   dest: " + destx + " " + desty + "   pos: " + currentx + " " + currenty + "\n";
		return s;
	}

}
