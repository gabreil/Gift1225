using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour {

	public static GameController gamecontroller;
	public Sprite[] fish = new Sprite[13];
	public Image fishimg;
	public GameObject txt_notice;
	public GameObject tipsUI;
	public Text tipsTxt;
	public GameObject ztUI;
	public Text ztTxt;


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
	}

	//显示状态
	public void ShowZT(string zttxt){
		ztUI.SetActive (true);
		ztTxt.text = zttxt;
		ztUI.transform.position = Camera.main.WorldToScreenPoint (PlayerController.playercontroller.transform.position) + new Vector3 (-20.0f, 270.0f, 0.0f);
	}

	public void HideZT(){
		ztUI.SetActive (false);
	}

	//----------------------------------------------------------------------以下是TIPS相关------------------------------------------------------------------------


	//显示各种TIPS
	void ShowTips(string tipstxt){
		tipsUI.SetActive (true);
		tipsTxt.text = tipstxt;
		//Debug.Log (tipstxt);
	}

	//隐藏TIPS
	public void HideTips(){
		tipsUI.SetActive (false);
	}


	public void ShowExpTips(){
		ShowTips ("正常状态经验每小时增长1点，心情好时加倍，饥饿时反而会降低~");
	}

	public void ShowHungryTips(){
		ShowTips ("可通过喂食食物提升。30以下为饥饿状态，会导致经验降低哦~");
	}

	public void ShowHappyTips(){
		ShowTips ("可通过点击野萌提升。30以下为不开心状态，70以上为心情好状态~");
	}

	public void ShowFoodTips(){
		ShowTips ("每次喂食消耗1个，不足时请点击获取食物按钮答题来获得。不许百度！");
	}

}
