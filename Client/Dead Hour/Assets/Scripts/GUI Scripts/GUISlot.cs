using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GUISlot : MonoBehaviour {

	Image currentImage;
	Inventory inventory;
	bool hasItem = false;
	void Awake() {
		currentImage = GetComponent<Image> ();
		currentImage.color = new Color (1, 1, 1, 0.5f);
		inventory = FindObjectOfType<Inventory> ();
	}
	public void OnMouseEnter() {
		if (!Cursor.visible)
			return;
		currentImage.color = new Color (1, 1, 1, 1);
	}

	public void OnMouseLeave() {
		if (hasItem || !Cursor.visible)
			return;
		currentImage.color = new Color (1, 1, 1, 0.5f);
	}

	public void OnMouseClicked() {
		//send clicking to the inventory
		if (!Cursor.visible)
			return;
		string number = name.Split (' ')[1];
		int num = int.Parse (number);
		if (Input.GetMouseButtonDown (0))
			inventory.SlotClicked (num);
		else if (Input.GetMouseButtonDown (1))
			inventory.Dropitem (num);
	}

	public void SetImage(Texture2D image) {
		if (image != null) {
			currentImage.sprite = Sprite.Create(image, new Rect(0,0, image.width, image.height), Vector2.zero);
			currentImage.color = new Color (1, 1, 1, 1);
			hasItem = true;
		}
	}

	public void RemoveImage() {
		if (currentImage != null) {
			currentImage.sprite = null;
			currentImage.color = new Color (1, 1, 1, 0.5f);
		}
	}
}
