﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	// timing window
	public int timeslices = 25;

	// every 6 ticks 1 square is travelled
	public int tick = 1;
	public int maxtick = 6;

	// reservation table
	public Grid[] grids;
	// single base map
	public int[,] originalgrid;
	public int[,] blocked;

	// checks if an idle was called on last iteration and replans paths
	bool replan;
	Vector2 replancoords;
	int replantime;

	LinkedList<Astar> studentlist = new LinkedList<Astar>();

	void Start () {

		// runs at 60 fps
		Application.targetFrameRate = 60;

		grids = new Grid[timeslices];

		// Get map from gameobject
		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		originalgrid = m.getGrid ();

		GameObject students = GameObject.Find ("Students");

		// Students are labelled from 3+
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

		// for checking idle students
		blocked = new int [20, 32];

		replan = false;
		replancoords = new Vector2();
		replantime = 0;
	}

	// shifts the time of every grid in the reservation table down by 1 and sets a fresh one at the end
	void updateGrid() {
		for (int i = 0; i < timeslices - 1; i++) {
			grids [i] = grids [i + 1];
		}
		grids [timeslices - 1] = new Grid ();
		foreach (Astar s in studentlist) {
			if (s.getIdling ()) {
				grids [timeslices - 1].add (s.getcurrentx(), s.getcurrenty(), s.getlabel ());
			}
		}
	}

	// print reservation table
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

	// alerts all students that a student is idling and repaths them if a conflict is about to happen
	public void alertIdle(Vector2 idlecoords, int time) {
		replan = true;
		replancoords = idlecoords;
		replantime = time;
	}
	
	// Update is called once per frame
	void Update () {

		tick++;
		if (tick > maxtick) {
			tick = 1;
			updateGrid ();
			// decrement every blocked value in the table
			for (int i = 0; i < 20; i++) {
				for (int j = 0; j < 32; j++) {
					if (blocked [i, j] > 0) {
						blocked [i, j] = blocked [i, j] - 1;
					}
				}
			}
		}

		List<Astar> toend = new List<Astar> ();
		List<Astar> tostart = new List<Astar> ();

		// Call the update of each student and add them to the start, end, or leave them based on the returned flag
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

		if (replan) {

			replan = false;

			List<Astar> studentstoend = new List<Astar> ();
			foreach (Astar s in studentlist) {
				List<Vector2> path = s.getPath ();
				int pathindex = s.getPathIndex ();
				for (int i = pathindex; i <= pathindex + replantime; i++) {
					if (i >= path.Count) {
						break;
					}
					// idle is in path
					if (s.isPathing() && path [i] == replancoords) {
						s.replan ();
						toend.Add (s);
						break;
					}
				}
			}
			// Add replanned nodes to end of student list
			foreach (Astar a in studentstoend) {
				studentlist.Remove (a);
				studentlist.AddLast (a);
			}

		}

	}
}
