using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Xml;
using System.IO;

enum enAttribute
{
	Level = 0,		//等级			能够解锁新动作？
	Exp,			//成长值		成长值增加到一定程度会升级 心情好会增长得快一些，心情不好会慢
	Hungry,			//饥饿度		饥饿度高于一定值后，成长会反而下降
	Food,			//食物量		需要答题获得，喂食行为 可降低饥饿度
	Happy,			//心情值		随时间慢慢降低，点击有助于心情值提升 心情过低会无法喂食
};

public class PlayerController : MonoBehaviour {
	//属性初始化
	static int[] InitData = {
		1,
		0,
		50,
		10,
		100,
	};

	// 问题与答案
	public static ArrayList QAndAList = new ArrayList();

	public static PlayerController playercontroller;
	public static Animator ani;

	public int[] Attrs;						//定义宠物相关属性

	DateTime saveTime; 						//其它属性自然增减时间戳
	DateTime saveTimeExp; 					//成长值自然增减时间戳
	private float fUpdateinteval = 0.3f;	//饥饿心情的变化间隔；
	private float fUpdateintevalExp = 1.0f;	//成长的变化间隔；

	private int iFoodValue = 10;			//每次喂食的饥饿度变化；
	private int iHappyValue = 10;			//每次点击的心情值变化；


	Vector3 targetPos;						//移动目标点
	private float speed = 0.03f;			//移动速度
	private bool bMoving = false;			//是否处于移动状态，避免移动状态下反复切换为自己

	public Text foodnum;					//食物量UI
	public Text expnum;						//成长值UI
	public Text hungrynum;					//饱食度UI  代码里为饥饿度，显示的时候用100去减
	public Text happynum;					//心情值

	public bool bInit = false;


	//获得实例
	void Awake(){
		playercontroller = this;
	}

	//初始化
	void Start () {
		LoadQuestion ();
		ani = this.GetComponent<Animator> ();
		targetPos = transform.position; 
		InitIdle (DateTime.Now);
		Attrs = new int[Enum.GetValues(typeof(enAttribute)).GetLength(0)];
		if (!LoadData ()) {
			InitAttr ();
		} else {
			InitCalc ();
		}
		bInit = true;
	}
	
	//Update
	void Update () {
		if (!bInit) {
			return;
		}
		DateTime time_now = DateTime.Now; //取得现在的时间  
		TimeSpan ts1 = time_now - saveTime; //判断差值
		if (ts1.TotalMinutes >= fUpdateinteval) {
			saveTime = saveTime.AddMinutes(fUpdateinteval);
			ChangeHungry(1);
			ChangeHappy (1);
		}
		TimeSpan ts2 = time_now - saveTimeExp; //判断差值
		if (ts2.TotalMinutes >= fUpdateintevalExp) {
			saveTimeExp = saveTimeExp.AddMinutes(fUpdateintevalExp);
			ChangeExp(1);
		}
		UpdateWalk ();
		UpdateUI ();

	}

	void InitCalc () {
		DateTime time_now = DateTime.Now; //取得现在的时间  
		DateTime time_while = saveTime;
		TimeSpan ts = time_now - time_while; //判断差值
		while (ts.TotalMinutes > 0)
		{
			TimeSpan ts1 = time_while - saveTime; //判断差值
			if (ts1.TotalMinutes >= fUpdateinteval) {
				saveTime = saveTime.AddMinutes(fUpdateinteval);
				ChangeHungry(1);
				ChangeHappy (1);
			}
			TimeSpan ts2 = time_while - saveTimeExp; //判断差值
			if (ts2.TotalMinutes >= fUpdateintevalExp) {
				saveTimeExp = saveTimeExp.AddMinutes(fUpdateintevalExp);
				ChangeExp(1);
			}
			time_while = time_while.AddMinutes (fUpdateinteval);
			ts = time_now - time_while;
		}
	}

	//成长值变化及等级变化
	void ChangeExp(int nExp){
		if(Attrs [(int)enAttribute.Hungry] >= 80) {
			Attrs [(int)enAttribute.Exp] -= nExp;
			if (Attrs [(int)enAttribute.Exp] < Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10){
				Attrs [(int)enAttribute.Exp] = Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10;
			}
		}
		if (Attrs [(int)enAttribute.Hungry] < 80 && Attrs [(int)enAttribute.Happy] >= 50) {
			Attrs [(int)enAttribute.Exp] += 2 * nExp;
			if (Attrs [(int)enAttribute.Exp] > Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] + 1) / 2 * 10) {
				Attrs [(int)enAttribute.Level]++;
			}
		}
		if (Attrs [(int)enAttribute.Hungry] < 80 && Attrs [(int)enAttribute.Happy] < 50) {
			Attrs [(int)enAttribute.Exp] += nExp;
			if (Attrs [(int)enAttribute.Exp] > Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10) {
				Attrs [(int)enAttribute.Level]++;
			}
		}
		SaveData ();
	}


	//饥饿度变化
	void ChangeHungry(int nhungry){
		Attrs [(int)enAttribute.Hungry] += nhungry;
		if (Attrs [(int)enAttribute.Hungry] > 100) {
			Attrs [(int)enAttribute.Hungry] = 100;
		}
		if (Attrs [(int)enAttribute.Hungry] < 0) {
			Attrs [(int)enAttribute.Hungry] = 0;
		}
		SaveData ();

	}


	//食物量变化
	void ChangeFood(int nfood){
		Attrs [(int)enAttribute.Food] += nfood;
		SaveData ();
	}

	//心情值变化
	void ChangeHappy(int nhappy){
		Attrs [(int)enAttribute.Happy] -= nhappy;
		if (Attrs [(int)enAttribute.Happy] > 100) {
			Attrs [(int)enAttribute.Happy] = 100;
		}
		if (Attrs [(int)enAttribute.Happy] < 0) {
			Attrs [(int)enAttribute.Happy] = 0;
		}
		SaveData ();
	}



	//被点击
	void OnMouseDown(){
		DoIdle ();
		if (Attrs [(int)enAttribute.Happy] >= 50) {
			ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 6));
			ani.SetTrigger ("toBeClickHappy");
		}
		if (Attrs [(int)enAttribute.Happy] < 50) {
			ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 4));
			ani.SetTrigger ("toBeClickSad");
		}
		ChangeHappy (-iHappyValue);
		//TODO:心情增加提示

	}
		

	//移动到某坐标
	public void MoveToPos(Vector3 Vpos) {
		if (!bMoving) {
			DoIdle ();
			ani.SetTrigger ("toMove");
		}

		Vpos.z = 0.0f;
		targetPos = Vpos;
		bMoving = true;


		Debug.Log ("Move");
	}

	//刷新移动
	void UpdateWalk() {
		
		Vector3 posoffset = targetPos - transform.position;
		if (Math.Abs (posoffset.x) > 0.1 || Math.Abs (posoffset.y) > 0.1) {
			posoffset.Normalize ();
			transform.position += posoffset * speed;
			if (posoffset.x < 0) {
				GetComponent<SpriteRenderer> ().flipX = true;
			} else {
				GetComponent<SpriteRenderer> ().flipX = false;
			}
		}
		else {
			if (bMoving) {
				DoIdle ();
			}
		}
	}
	//刷新UI
	void UpdateUI() {
		foodnum.text = string.Format("{0:D3}", Attrs [(int)enAttribute.Food]);
		expnum.text = string.Format("{0:D4}", Attrs [(int)enAttribute.Exp]);
		hungrynum.text = string.Format("{0:D3}", 100 - Attrs [(int)enAttribute.Hungry]);
		happynum.text = string.Format("{0:D3}", Attrs [(int)enAttribute.Happy]);
	}

	//初始化待机动作
	void InitIdle(DateTime now){
		ani.SetInteger ("iHour", now.Hour);
		ani.SetTrigger ("toIdleTime");
	} 

	//停止移动
	void StopMove(){
		targetPos = transform.position;
	}

	//喂食
	public void OnEat(){
		if (Attrs [(int)enAttribute.Happy] >= 50) {
			if (Attrs [(int)enAttribute.Food] >= 1) {
				DoIdle ();
				ani.SetTrigger ("toEat");
				ChangeHungry (-iFoodValue);
				ChangeFood (-1);
			} else {
				//TODO:食物不足提示；
			}
		}else{
			//TODO:心情不好不想吃饭提示；
		}

	}
	public void OnGetFood(){
		Attrs [(int)enAttribute.Food]++;
	}

	// 随机问题序号
	public int RandQuestion() {
		return UnityEngine.Random.Range (0, QAndAList.Count);
	}

	// 判断回答是否正确
	public bool IsAnswerRight(int nQueIdx, string strAnswer) {
		string[] QandA = (string[])QAndAList[nQueIdx];
		if (QandA[1] == strAnswer)
		{
			return true;
		}
		return false;
	}


	//置为待机状态（在任何变化状态时均先调用）
	void DoIdle(){
		StopMove ();
		GetComponent<SpriteRenderer> ().flipX = false;
		ani.SetTrigger ("toIdle");
		bMoving = false;
		Debug.Log ("idle");
		//TODO:判断待机状态函数；
	}

	//置为随机待机状态


	// 初始化属性
	public void InitAttr()
	{
		Attrs = new int[Enum.GetValues(typeof(enAttribute)).GetLength(0)];
		foreach (int i in Enum.GetValues(typeof(enAttribute))) {
			Attrs [i] = InitData[i];
		}

		saveTime = DateTime.Now;
		saveTimeExp = DateTime.Now;
		SaveData ();
	}

	// 保存数据到文件
	public void SaveData()
	{
		string path = Application.persistentDataPath + "/data.xml";
		XmlDocument xml = new XmlDocument();
		XmlElement root = xml.CreateElement("root");
		xml.AppendChild(root);
		XmlElement data = xml.CreateElement("data");
		root.AppendChild(data);
		foreach (int i in Enum.GetValues(typeof(enAttribute)))
		{
			string strName = Enum.GetName(typeof(enAttribute), i);
			data.SetAttribute(strName, Convert.ToString(Attrs[i]));

		}
		data.SetAttribute("Time", saveTime.ToString());
		data.SetAttribute("TimeExp", saveTimeExp.ToString());
		xml.Save(path);
	}

	// 文件读取数据
	public bool LoadData()
	{
		string strFileName = Application.persistentDataPath + "/data.xml";
		if (!File.Exists (strFileName))
			return false;
		XmlDocument xml = new XmlDocument();
		xml.Load(strFileName);
		XmlNodeList xmlNodeList = xml.SelectSingleNode("root").ChildNodes;
		foreach (XmlElement element in xmlNodeList) {
			foreach (int i in Enum.GetValues(typeof(enAttribute))) {
				string strName = Enum.GetName (typeof(enAttribute), i);
				Attrs [i] = int.Parse (element.GetAttribute(strName));
			}
			saveTime = Convert.ToDateTime(element.GetAttribute("Time"));
			saveTimeExp = Convert.ToDateTime (element.GetAttribute ("TimeExp"));
		}
		return true;
	}

	public bool LoadQuestion()
	{
		string strFileName = Application.dataPath + "/question.txt";
		FileStream fs = new FileStream(strFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
		//记录每次读取的一行记录
		string strLine = "";
		//记录每行记录中的各字段内容
		string[] aryLine;
		while ((strLine = sr.ReadLine()) != null)
		{
			aryLine = strLine.Split('\t');
			QAndAList.Add (aryLine);
		}
		sr.Close();
		fs.Close();
		return true;
	}
}
