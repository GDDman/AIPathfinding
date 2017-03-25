using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Describes the AI behavious, and pathfinding of a student 
public class Astar : MonoBehaviour {

	// How many nodes to seach 
	static int searchnodes = 200;

	// current position of the student
	int currentx;
	int currenty;

	// the number label of the student
	int label;

	// flags for pathfinding
	bool pathing;
	bool finishedpath;
	bool changepath;
	bool idling;

	// variables for changing student color based on frustration
	float lastheur = 1000f;
	float frusttime = 0f;
	float accumtime = 0f;
	MeshRenderer render;
	Material purple;
	Material yellow;
	Material red;

	Vector2 randpath = new Vector2 ();
	int pathindex;

	// The tree that defines the student's behaviours
	BehaviourTree plan;

	// the current plaque or prof to path to chosen at random
	int randprof;
	int randplaque;

	// set coordinates of professors and plaques are known by the student
	Vector2[] profcoords = new Vector2[6];
	Vector2[] plaquecoords = new Vector2[6];
	LinkedList<int> memory = new LinkedList<int>();

	int timecounter = 0;
	int idlingcounter = 0;

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

	// settable in the inspector
	public int initialx = 0;
	public int initialy = 0;

	// the destination coordinates that the student wants to go to
	int destx = 0;
	int desty = 0;

	// list of nodes being searched
	Heap<Node> nodelist = new Heap<Node>(searchnodes);
	// list of nodes already searched
	HashSet<Vector2> closed = new HashSet<Vector2>();
	// the current path the student is travelling
	List<Vector2> path = new List<Vector2>();
	// reference to the main game class
	Game game;

	// random time for the student to idle by 
	int randidleticks = 0;

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

		// Hard setting coordinates baed on the map coordinates (not world)
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

		// this function translate the map coordinates to world coordinates and positions the student 
		updatePosition ();
		finishedpath = false;
		changepath = true;

		// color materials 
		render = (MeshRenderer)transform.GetComponent<MeshRenderer> ();
		purple = Resources.Load("Materials/Purple", typeof(Material)) as Material;
		yellow = Resources.Load("Materials/Yellow", typeof(Material)) as Material;
		red = Resources.Load("Materials/Red", typeof(Material)) as Material;
		render.material = purple;
	}
		
	//Various getters and setters
	public bool getIdling() {
		return idling;
	}

	public List<Vector2> getPath() {
		return path;
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

	// translate a map coordinate to world coordinate
	Vector2 getPoint(Vector2 positions) {
		float x = (((float)positions.y)/2 - 8) + (float)0.25;  
		float y = (-((float)positions.x)/2 + 5) - (float)0.25;
		return new Vector2(x, y);
	}

	// get euclidian distance between two coordinates
	public float getDist(Vector2 from, Vector2 to) {
		Vector3 vec = to - from;
		return vec.magnitude;
	}

	// get euclidian distance between the student and the goal
	public float dist() {
		Vector2 from = new Vector2 (currentx, currenty);
		Vector2 to = new Vector2 (destx, desty);
		Vector3 vec = to - from;
		return vec.magnitude;
	}

	// Main function of student that loops at 60fps
	public int updateStudent (float time) {

		accumtime += time;
		frusttime += time;

		// Calculating frustration and setting color

		// student has reached goal so reset 
		if (finishedpath) {
			lastheur = 1000f;
			frusttime = 0f;
			render.material = purple;
		}
		else if (accumtime > 0.2f) {
			accumtime = 0f;
			float currentheur = dist ();
			// lastheur is the lst heuristic (euclidian distance) closest to the goal found.
			// If the student is currently on a better space, the frustration is reset
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
			
		// This boolean is returned at the end of the function. It is a flag for the Game class.
		// If -1, the student is sent to the front of the priority queue. If 1, the student is sent to the end of the queue. 
		// 0 they maintain their position. 
		int tolast = 0;

		// This is the behaviour tree for the student

		// The update loops until the tree is traversed to a leaf node (action)
		// The root of the tree is a repeater, so the behaviours repeat indefinitely
		plan.update ();
		if (plan.current.actiontype == Actiontype.NULL)
			return 0;

		switch (plan.current.actiontype) 
		{
		// action always sucessful
		case Actiontype.FINISH:

			plan.current.status = BNode.Status.SUCESS;
			break;

		// student checks their memory to see if they know the prof location of the random prof 
		case Actiontype.CHECKDATA:
			
			if (memory.Contains (randprof)) {
				plan.current.status = BNode.Status.SUCESS;
			} else {
				plan.current.status = BNode.Status.FAILED;
			}
			tolast = 1;
			// skips an iteration
			plan.update ();
			if (plan.current.actiontype == Actiontype.PATHPLAQUE) {
				goto case Actiontype.PATHPLAQUE;
			}
			break;
		
		// pathfind to random unknown plaque if unknown
		case Actiontype.PATHPLAQUE:

			// at the end of the path, end the action with a sucess.
			if (idling) {
				idling = false;
				idlingcounter = 0;
				plan.current.status = BNode.Status.SUCESS;
				changepath = true;
				break;
			}
	
			// Plan the path to travel
			if (!pathing) {

				// path actions happen in set intervals
				if (game.tick == 1) {

					// if a new target plaque
					if (changepath) {
						randplaque = Random.Range (0, 6);
						while (memory.Contains (randplaque)) {
							randplaque = Random.Range (0, 6);
						}
					}

					// set the destination
					Vector2 goal = plaquecoords [randplaque];
					destx = (int) goal.x;
					desty = (int) goal.y;
					tolast = 1;

					// this function sets the path in the global path variable, returns true if it is a full path to the goal
					// and false if it is only a partial path. If complete it returns two goal nodes in the path
					try {
						finishedpath = decidePath (false);
					}
					catch (System.Exception e) {
						finishedpath = false;
					}

					// Edge case
					if (path.Count <= 1) {
						idling = true;
						pathindex = 0;
						idlingcounter = 100000;
						tolast = 1;
						break;
					}
					// If the path is not done, complete the path and then find a new partial path
					if (!finishedpath) {
						// the target plaque is the same
						changepath = false;
					} else {
						// Set 1 square on each side around the goal where other student can't enter while this student is thinking
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
					// pathindex is the current position in the path
					pathing = true;
					pathindex = 0;
				}
				// continue right away to the pathing
			}
			if (pathing) {

				if (path.Count <= 1) {
					pathing = false;
					pathindex = 0;
					idling = true;
					break;
				}
					
				// linear interpolation of the position between grid tiles on non-primary game ticks
				if (game.tick != game.maxtick) {

					Vector2 pos = path [pathindex];
					Vector2 newpos = path [pathindex + 1];
					Vector2 v = getPoint(newpos) - getPoint(pos);
					Vector3 v3 = new Vector3 (v.x, 0, v.y);
					float f = 1/(float)(game.maxtick);
					v3 = Vector3.Scale (new Vector3 (f, 0, f), v3);
					transform.position = transform.position + v3;

				} else {

					// get next position in the path and take its coordinates
					Vector2 newpos = path [++pathindex];
					currentx = (int) newpos.x;
					currenty = (int) newpos.y;

					// update to middle of grid square
					updatePosition ();

					// path is finished, the second last and last nodes will be the goal, set the status of the student to idling
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						game.alertIdle (new Vector2 (currentx, currenty), 2);
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
							game.alertIdle (new Vector2 (currentx, currenty), 2);
						}
					}
				}

			}
			break;

		// Checks a plaque to see if it is the right one
		case Actiontype.CHECKPLAQUE:
			
			if (randplaque == randprof) {
				plan.current.status = BNode.Status.SUCESS;

			} else {
				plan.current.status = BNode.Status.FAILED;
			}

			// student memory has 4 slots and is a priority queue
			updateMemory (randplaque);
			tolast = 1;
			plan.update ();
			// skip iteration
			if (plan.current.actiontype == Actiontype.PATHPROF) {
				goto case Actiontype.PATHPROF;
			}
			break;

		// calcualte and take a path to a known professor location
		case Actiontype.PATHPROF:

			// return a sucess
			if (idling) {
				idling = false;
				idlingcounter = 0;
				plan.current.status = BNode.Status.SUCESS;
				changepath = true;
				tolast = 1;
				break;
			}
			// calulate the path
			if (!pathing) {
				
				if (game.tick == 1) {
					Vector2 goal2 = profcoords [randprof];
					destx = (int)goal2.x;
					desty = (int)goal2.y;
					tolast = 1;
					// Store path in global path variable
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
						// Set unwalkable area near goal
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

				// linear interpolation
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
					// goal is reached, pass to idle state
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						game.alertIdle (new Vector2 (currentx, currenty), 2);
						break;
					}
					if (pathindex == path.Count - 1) {
						pathing = false;
						currentx = (int)path [pathindex].x;
						currenty = (int)path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						if (finishedpath) {
							game.alertIdle (new Vector2 (currentx, currenty), 2);
							idling = true;
						}
					}
				}
			}
			break;

		// Path to a random location
		case Actiontype.PATHRAND:

			if (idling) {

				// idle for a random amount of time before returning a sucess
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

			// Calculate the path
			if (!pathing) {
				if (game.tick == 1) {
					if (changepath) {
						// Keep trying until an empty space is found on the map
						randpath.x = Random.Range (0, 20);
						randpath.y = Random.Range (0, 32);
						while (game.originalgrid [(int)randpath.x, (int)randpath.y] != 0) {
							randpath.x = Random.Range (0, 20);
							randpath.y = Random.Range (0, 32);
						}
					}
					destx = (int)randpath.x;
					desty = (int)randpath.y;
					// Get path and response
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
						// unwalable area includes corners
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
				// linear interpolation
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
					// goal is found, close off coordinate for the amount of turns the student is idling
					if (pathindex == path.Count - 2 && finishedpath) {
						pathing = false;
						currentx = (int) path [pathindex].x;
						currenty = (int) path [pathindex].y;
						updatePosition ();
						pathindex = 0;
						idling = true;
						randidleticks = Random.Range(3, 11);
						// closes the space for a set amount of turns (bigger than the timing window)
						game.blocked [currentx, currenty] = randidleticks + 6;
						game.alertIdle (new Vector2 (currentx, currenty), randidleticks + 6);
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
							randidleticks = Random.Range(3, 11);
							game.blocked [currentx, currenty] = randidleticks + 6;
							game.alertIdle (new Vector2 (currentx, currenty), randidleticks + 6);
						}
					}
				}

			}
			break;
		// pick a random prof to try to find
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

		// the flag indicating the student priority
		return tolast;
	}

	public int getPathIndex() {
		return pathindex;
	}

	public bool isPathing() {
		return pathing;
	}

	// if path crosses idle 
	public void replan() {
		pathing = false;
		pathindex = 0;
	}

	// updates known professors. Priority queue of size 4.
	void updateMemory(int r) {
		memory.Remove (r);
		memory.AddFirst (r);
		if (memory.Count > 4) 
			memory.RemoveLast ();
	}

	// Used to get posible children of a node in the path
	ArrayList getChildren(Node node, bool prof) {

		Vector2 goal = new Vector2 (destx, desty);
		ArrayList children = new ArrayList ();

		Grid[] grids = game.grids;
		Grid og = new Grid ();

		int x = (int) node.index.x;
		int y = (int) node.index.y;

		// checks all the spaces around
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
					// Use the grids in the timing window if possible
					if (timecounter + 1 < game.timeslices) {
						g = grids [timecounter + 1];
					} else {
						g = og;
					}
					// corner neighbours
					if ((g.getgrid((x + i), (y + j)) == 0 || g.getgrid((x + i), (y + j)) == label) && g.getgrid(x, (y + j)) == 0 && g.getgrid((x + i), y) == 0) {
						// use 8 way manhatten distance heuristic
						int hdist = manhattenDist (new Vector2 (x + i, y + j), goal);
						// Cost to neighbour is always 1. Nodes store the time in the path
						Node n = new Node (node.cost + 1, x + i, y + j, hdist, timecounter + 1);
						if (closed.Contains(n.index)) 
							continue;
						n.parent = node;
						if (!nodelist.Contains (n)) {
							//add neighbour to children list
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
						// paths to professors can have the prof position in the path as the goal (it is taken out later)
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

	// returns true if the path ends at the goal, flase if a partial path. Stores the path in the global path variable
	public bool decidePath(bool prof) {

		// Reset the nodelists
		Vector2 goal = new Vector2 (destx, desty);
		pathing = true;
		nodelist = new Heap<Node>(searchnodes);
		closed = new HashSet<Vector2> ();

		// add the current position at time and cost 0 to openlist
		nodelist.Add (new Node (0, currentx, currenty, 0, 0));

		Node end = null;
		while (nodelist.Count > 0) {

			// Gets the node with the best cost from the heap
			Node n = nodelist.RemoveFirst ();
			timecounter = n.time;

			// ignores this node if it is swapping positions with another one (the turn before they were in each other's positions)
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

			// If a student is idling the node space, ignore this node
			if (game.blocked [(int)n.index.x, (int)n.index.y] >= timecounter + 1) {
				continue;
			}

			// nodes is travelled 
			closed.Add (n.index);

			// get valid children for this node
			ArrayList children = getChildren (n, prof);
			foreach (Node child in children) {
				try { 
					nodelist.Add (child);
				}
				catch (System.Exception e) {
					// This occurs when the algorithm has searched through too many nodes (probably means no path to goal exists)
					foreach (Node tempnode in nodelist.items) {
						// checks for best end node
						if (tempnode.time == n.time && getDist (tempnode.index, new Vector2 (destx, desty)) <= getDist (n.index, new Vector2 (destx, desty))) {
							n = tempnode;
						}
					}
					end = n;
					break;
				}
			}
			// get the goal node if found
			if (checkGoal (n, goal)) {
				end = n;
				break;
			}
			// get the next best node if not
			end = n;
		}

		// follow the tree from goal leaf to root and set the path
		path = new List<Vector2>();
		path.Insert (path.Count, end.index);
		Node curr = end;
		while ((curr = curr.parent) != null) {
			path.Insert (0, curr.index);
		}

		// add a second goal node
		path.Add (path [path.Count - 1]);

		// if bigger than the timing window, cut the path off to be the size of the timing window
		if (path.Count >= game.timeslices) {
			path = path.GetRange (0, game.timeslices);
		}
			
		// If second goal was not cut off
		if (path [path.Count - 1] == goal && path[path.Count - 2] == goal) {
			// change goal for professor to spot in front of them
			if (prof) {
				path.RemoveAt (path.Count - 1);
				path.RemoveAt (path.Count - 1);
				path.Add (path[path.Count - 1]);
			}
			// for each timing window on the map, set the reservation for the path
			for (int i = 0; i < path.Count; i++) {
				game.grids [i].add ((int)path[i].x, (int)path[i].y, label);
			}

			return true;
		} 

		// for each timing window on the map, set the reservation for the path
		for (int i = 0; i < path.Count; i++) {
			game.grids [i].add ((int)path[i].x, (int)path[i].y, label);
		}
		return false;

	}

	// Checks to see if this node is the goal node
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
