using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Script : MonoBehaviour {

    public Slider blood_slider;
    public GameObject wizardGO;
    public GameObject menu;
    public GameObject levelLength;
    public GameObject levelDifficulty;

    Wizard wizard;
    TerrainGenerator generator;

    private void Awake()
    {
        wizard = wizardGO.GetComponent<Wizard>();
        generator = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
    }

    private void OnEnable()
    {
        var info = generator.InfoNextLevel();
        levelLength.GetComponent<TextMeshProUGUI>().text = info.Item1;
        levelDifficulty.GetComponent<TextMeshProUGUI>().text = info.Item2;
    }

    // Update is called once per frame
    void Update () {
        blood_slider.value = wizard.health;
	}

    public void StartLevel()
    {
        menu.SetActive(false);
        wizardGO.transform.position = new Vector3(1, 2, 0);
        wizardGO.SetActive(true);
        generator.GenerateNextLevel();
    }
}
