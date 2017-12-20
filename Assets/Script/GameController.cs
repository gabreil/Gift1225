using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour {

	public static GameController gamecontroller;
	public Sprite[] fish = new Sprite[13];
	public Image fishimg;

	void Awake(){
		gamecontroller = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeFish(){
		int id = Random.Range (0, 13);
		fishimg.GetComponent<Image>().sprite = fish[id];
	}

	public void DownExp(){
		PlayerController.playercontroller.OnGetFood ();
	}
	public void UpExp(){
		PlayerController.playercontroller.CloseUI();
	}

}
