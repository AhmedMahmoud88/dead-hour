using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M4M1 : Weapon {

	protected override void Awake ()
	{
		base.Awake ();
		WeaponType = Types.WeaponTypes.Rifle;
		ClipAmmo = 32;
		MaxAmmo = 320;
	}
}

