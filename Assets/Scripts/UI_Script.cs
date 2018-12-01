using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour {

    public Slider blood_slider;

    Wizard wizard;

	// Use this for initialization
	void Start () {
        wizard = GameObject.Find("Wizard").GetComponent<Wizard>();
	}
	
	// Update is called once per frame
	void Update () {
        blood_slider.value = wizard.health;
	}
}
