using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	public int timeslices = 20;
	public int tick = 1;
	public int maxtick = 6;
	public Grid[] grids;
	public int[,] originalgrid;
	public int[,] blocked;

	ArrayList studentlist = new ArrayList();

	void Start () {

		grids = new Grid[timeslices];

		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		originalgrid = m.getGrid ();

		blocked = new int[20, 32];

		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 32; j++) {
				blocked [i, j] = 0;
			}
		}

		GameObject students = GameObject.Find ("Students");
		int temp = 3;
		foreach (Transform student in students.transform) {
			Astar a = (Astar) student.GetComponent ("Astar");
			a.setGame (this);
			a.setlabel(temp);
			temp++;
			studentlist.Add (a);
		}
		tick = 1;

		for (int i = 0; i < timeslices; i++) {
			grids [i] = new Grid ();
		}

	}

	void printBlocked() {
		System.Console.WriteLine ("");
		for (int i = 0; i < 20; i++) {
			string s = "";
			for (int j = 0; j < 32; j++) {
				if (blocked [i, j] < 0) {
					s += ("0 ");
				} else {
					s += ((blocked[i, j] + 1) + " ");
				}
			}
			System.Console.WriteLine (s);
		}
	}

	void updateGrid() {
		for (int i = 0; i < timeslices - 1; i++) {
			grids [i] = grids [i + 1];
		}
		grids [timeslices - 1] = new Grid ();
	}

	public void printgrids() {
		for (int i = 0; i < timeslices; i++) {
			System.Console.WriteLine (i);
			printgrid(grids [i]);
		}
	}

	public void printgrid(Grid g) {
		int[,] grid = g.grid;
		for (int i = 0; i < 20; i++) {
			string s = "";
			for (int j = 0; j < 32; j++) {
				if (grid [i, j] != 0)
					s += (grid [i, j] + " ");
				else 
					s += "  ";
			}
			System.Console.WriteLine (s);
		}
	}
	
	// Update is called once per frame
	void Update () {

		tick++;
		if (tick > maxtick) {
			tick = 1;
			System.Console.WriteLine ("---------------");
			printgrids ();
			updateGrid ();
			for (int i = 0; i < 20; i++) {
				for (int j = 0; j < 32; j++) {
					if (blocked [i, j] != 0) {
						grids [grids.Length - 1].add (i, j, 3);
						blocked [i, j] = blocked [i, j] - 1;
					}
				}
			}
		}

		List<Astar> toend = new List<Astar> ();

		foreach (Astar student in studentlist) {
			try {
				if (student.updateStudent ()) {
					toend.Add(student);
				}
			}
			catch (System.Exception e) {
				//print (student.ToString ());
			}
		}

		foreach (Astar a in toend) {
			studentlist.Remove (a);
			studentlist.Add (a);
		}

	}
}
