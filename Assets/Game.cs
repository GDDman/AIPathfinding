using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	int timeslices = 10;
	public int tick = 1;
	public int maxtick = 6;
	int[,] originalgrid;
	public int speed = 10;
	//int[][,] timegrid = new int[timeslices][20, 32]; 

	ArrayList studentlist = new ArrayList();

	void Start () {

		GameObject map = GameObject.Find ("Map");
		Map m = (Map)map.GetComponent ("Map");
		originalgrid = m.getGrid ();

		GameObject students = GameObject.Find ("Students");
		foreach (Transform student in students.transform) {
			Astar a = (Astar) student.GetComponent ("Astar");
			studentlist.Add (a);
		}
		tick = 1;

	}
	
	// Update is called once per frame
	void Update () {

		tick++;
		if (tick > maxtick)
			tick = 1;

		foreach (Astar student in studentlist) {
			try {
				student.updateStudent (this, Time.deltaTime);
			}
			catch (System.Exception e) {
				print(e);
			}
		}

	}
}
