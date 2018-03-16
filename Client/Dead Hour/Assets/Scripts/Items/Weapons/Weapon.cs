using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {

	public Types.WeaponTypes WeaponType;
	protected int ClipAmmo = 12;
	protected int MaxAmmo = 120;

	public int CurrentClip = 0;
	public int CurrentAmmo = 0;
	protected override void Awake ()
	{
		base.Awake ();
	}
	// Update is called once per frame
	void Update () {
		
	}

	public virtual void Fire() {
		if (CurrentClip != 0) {
			//fire here
		}
	}

	public virtual void AddAmmo(int ammo) {
		int ammu = ammo;
		while (CurrentClip < ClipAmmo && ammu > 0) {
			CurrentClip += 1;
			ammu--;
		}
		if (ammu > 0) {
			CurrentAmmo += ammu;
		}
	}
}
