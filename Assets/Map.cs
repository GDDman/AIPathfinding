using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

	// these are tiles of size 0.5x0.5 (0,0) is top left corner, 0 = empty, 1 = obstruction, 2 = prof;
	int[,] grid;

	// Use this for initialization
	void Start () {

		grid = new int[20, 32];

		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 32; j++) {
				grid [i, j] = 0;	
			}
		}

		// left corners
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 7; j++) {
				if (!(i >= 4 && j >= 4)) 
					grid [i, j] = 1;	
			}
		}
		grid [8, 3] = 1;
		grid [9, 3] = 1;
		grid [3, 7] = 1;
		grid [3, 8] = 1;

		grid [11, 3] = 1;
		grid [12, 3] = 1;
		grid [16, 7] = 1;
		grid [16, 8] = 1;

		for (int i = 13; i < 20; i++) {
			for (int j = 0; j < 7; j++) {
				if (!(i < 16 && j > 3))
					grid [i, j] = 1; 
			}
		}

		// middles 
		grid [3, 10] = 1;
		grid [3, 11] = 1;

		for (int i = 0; i < 4; i++) {
			for (int j = 12; j < 20; j++) {
				grid [i, j] = 1;
			}
		}

		grid [3, 20] = 1;
		grid [3, 21] = 1;

		grid [16, 10] = 1;
		grid [16, 11] = 1;

		for (int i = 16; i < 20; i++) {
			for (int j = 12; j < 20; j++) {
				grid [i, j] = 1;
			}
		}

		grid [16, 20] = 1;
		grid [16, 21] = 1;

		// Right corners

		grid [3, 23] = 1;
		grid [3, 24] = 1;
		grid [7, 28] = 1;
		grid [8, 28] = 1;

		for (int i = 0; i < 7; i++) {
			for (int j = 25; j < 32; j++) {
				if (!(i >= 4 && j < 28)) 
					grid [i, j] = 1;	
			}
		}

		grid [10, 28] = 1;
		grid [11, 28] = 1;
		grid [16, 23] = 1;
		grid [16, 24] = 1;

		for (int i = 12; i < 20; i++) {
			for (int j = 25; j < 32; j++) {
				if (!(i < 16 && j < 28))
					grid [i, j] = 1; 
			}
		}

		// pillars

		grid [5, 21] = 1;
		grid [5, 22] = 1;
		grid [5, 23] = 1;

		grid [5, 8] = 1;
		grid [5, 9] = 1;
		grid [5, 10] = 1;

		grid [14, 21] = 1;
		grid [14, 22] = 1;
		grid [14, 23] = 1;

		grid [14, 8] = 1;
		grid [14, 9] = 1;
		grid [14, 10] = 1;

		grid [9, 5] = 1;
		grid [10, 5] = 1;
		grid [11, 5] = 1;

		grid [8, 26] = 1;
		grid [9, 26] = 1;
		grid [10, 26] = 1;

		// profs
		grid [2, 7] = 2;
		grid [2, 20] = 2;
		grid [7, 29] = 2;
		grid [17, 24] = 2;
		grid [17, 11] = 2;
		grid [12, 2] = 2;
	
		printGrid ();

	}

	public int[,] getGrid() {
		return grid;
	}

	void printGrid() {
		for (int i = 0; i < 20; i++) {
			string s = "";
			for (int j = 0; j < 32; j++) {
				if (grid [i, j] != 0)
					s += (grid [i, j] + " ");
				else 
					s += "0 ";
			}
			System.Console.WriteLine (s);
		}
	}
		
	// Update is called once per frame
	void Update () {
		
	}
}
