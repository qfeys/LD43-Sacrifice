using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour {

    public GameObject wizard;
    public GameObject menu;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Wizard"))
        {
            wizard.SetActive(false);
            menu.SetActive(true);
            menu.transform.parent.GetComponent<UI_Script>().Refresh_menu();
        }
    }
}
