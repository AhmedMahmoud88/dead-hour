using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerCanvasController : MonoBehaviour {
	[Header("Player Weapon Info")]
	public GameObject WeaponPanel;
	public Text WeaponName;
	public Text WeaponAmmo;


	void Awake() {
		WeaponPanel.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void EnableWeaponInfo(bool enable) {
		WeaponPanel.SetActive (enable);
	}

	public void SetWeaponName(string name) {
		WeaponName.text = name;
	}

	public void SetWeaponAmmo(int clip, int maxAmmo) {
		WeaponAmmo.text = clip + "/" + maxAmmo;
	}
}
