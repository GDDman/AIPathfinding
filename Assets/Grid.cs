using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {

	public int[,] grid;

	public Grid() {
		GameObject map = GameObject.Find ("Map");
		Map m = (Map) map.GetComponent ("Map");
		grid = new int[20, 32];
		int[,] g = m.getGrid ();
		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 32; j++) {
				grid [i, j] = g [i, j];
			}
		}
	}

	public void add(int i, int j, int num) {
		if (grid [i, j] == 0) {
			grid [i, j] = num;
		}
	}

	public int getgrid(int i, int j) {
		return grid [i, j];
	}

}
