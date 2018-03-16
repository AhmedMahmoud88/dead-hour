using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Item : MonoBehaviour {

	[Header("Item Type")]
	public Types.ItemTypes ItemType;

	[Header("Item Image")]
	public Texture2D ItemSprite;


	[Header("Item Name")]
	public string name;

	public Canvas itemCanvas;

	public bool IsPickedup = false;
	public int Id;
	// Use this for initialization
	protected virtual void Awake() {
		itemCanvas = GetComponentInChildren<Canvas> ();
		itemCanvas.enabled = false;
		DontDestroyOnLoad (this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other) {
		if (IsPickedup)
			return;
		if (other.tag == "Player") {
			itemCanvas.enabled = true;
			CharacterScript player = other.GetComponent<CharacterScript> ();
			player.AddPickupItem (this);
		}
	}

	void OnTriggerExit(Collider other) {
		if (IsPickedup)
			return;
		if(other.tag == "Player") {
			itemCanvas.enabled = false;
			CharacterScript player = other.GetComponent<CharacterScript> ();
			player.RemovePickupitem (this);
		}
	}
		
	public virtual void Use() {

	}
	public virtual void Pickup() {
		IsPickedup = true;
		itemCanvas.enabled = false;
		gameObject.SetActive (false);
	}

	public virtual void Equip() {

	}

	public virtual void UnEquip() {

	}

	public virtual void Remove() {

	}
		
}
