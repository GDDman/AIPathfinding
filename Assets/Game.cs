using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	public int timeslices = 25;
	public int tick = 1;
	public int maxtick = 6;
	public Grid[] grids;
	public int[,] originalgrid;

	LinkedList<Astar> studentlist = new LinkedList<Astar>();

	void Start () {

		Application.targetFrameRate = 60;

		grids = new Grid[timeslices];

		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		originalgrid = m.getGrid ();

		GameObject students = GameObject.Find ("Students");
		int temp = 3;
		foreach (Transform student in students.transform) {
			Astar a = (Astar) student.GetComponent ("Astar");
			a.setGame (this);
			a.setlabel(temp);
			temp++;
			studentlist.AddLast (a);
		}
		tick = 1;

		for (int i = 0; i < timeslices; i++) {
			grids [i] = new Grid ();
		}

	}

	void updateGrid() {
		for (int i = 0; i < timeslices - 1; i++) {
			grids [i] = grids [i + 1];
		}
		grids [timeslices - 1] = new Grid ();
	}

	public void printgrids() {
		System.Console.WriteLine ("START");
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
			updateGrid ();
		}

		List<Astar> toend = new List<Astar> ();
		List<Astar> tostart = new List<Astar> ();

		foreach (Astar student in studentlist) {
			int flag = student.updateStudent (Time.deltaTime);
			if (flag == 1) {
				toend.Add(student);
			}
			else if (flag == -1) {
				tostart.Add(student);
			}
		}

		foreach (Astar s in tostart) {
			studentlist.Remove (s);
			studentlist.AddFirst (s);
		}

		foreach (Astar e in tostart) {
			studentlist.Remove (e);
			studentlist.AddLast (e);
		}

	}
}
