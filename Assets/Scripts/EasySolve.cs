﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasySolve : MonoBehaviour {

	public KMBombModule module;
	public KMSelectable button;

	// Use this for initialization
	void Start () {
		button.OnInteract += delegate {
			module.HandlePass();
			return false;
		};
	}
}
