using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour {

	public static GameController gamecontroller;
	public Sprite[] fish = new Sprite[13];
	public Image fishimg;
	public GameObject txt_notice;

	void Awake(){
		gamecontroller = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//改鱼
	public void ChangeFish(){
		int id = Random.Range (0, 13);
		fishimg.GetComponent<Image>().sprite = fish[id];
	}

	//显示提示
	public void ShowNotice(string noticetxt){
		GameObject notice = Instantiate (txt_notice);
		notice.transform.SetParent(GameObject.Find ("UI_Notice").transform);
		notice.GetComponent<Text> ().text = noticetxt;
		Debug.Log (notice.GetComponent<Text> ().text);
	}

	//显示各种TIPS
	public void DownExp(){
		ShowNotice ("test");
	}
	public void UpExp(){
		ShowNotice ("test1");
	}

}
