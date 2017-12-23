using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Xml;
using System.IO;
using UnityEngine.EventSystems;

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
		0,
		50,
	};

	// 问题与答案
	public static ArrayList QAndAList = new ArrayList();

	public static PlayerController playercontroller;
	public static Animator ani;

	public int[] Attrs;						//定义宠物相关属性

	DateTime saveTime; 						//其它属性自然增减时间戳
	DateTime saveTimeExp; 					//成长值自然增减时间戳
	float fUpdateinteval = 8.0f;			//饥饿心情的变化间隔；
	float fUpdateintevalExp = 60.0f;		//成长的变化间隔；
	float toIdleTime = 10.0f; 				//切换时段待机的时长；
	float idleTime = 0.0f;					//用于判断当前停止操作的时间；
	int nor = 0;							//用于随机播放待机动作；

	int iFoodValue = 15;						//每次喂食的饥饿度变化；
	int iHappyValue = 10;					//每次点击的心情值变化；


	Vector3 targetPos;						//移动目标点
	float speed = 0.03f;					//移动速度
	bool bMoving = false;					//是否处于移动状态，避免移动状态下反复切换为自己

	public Text levelnum;					//等级UI
	public Text foodnum;					//食物量UI
	public Text expnum;						//成长值UI
	public Text hungrynum;					//饱食度UI  代码里为饥饿度，显示的时候用100去减
	public Text happynum;					//心情值


	public Image qbg;						//问答界面
	public Text ques;						//问题文本
	public Text answ;						//答题文本
	public InputField answfield;			//输入框
	public int nQuesIdx;					//问题序号
	bool bAnswer = true;					//是否回答了问题

	public bool bInit = false;


	//获得实例
	void Awake(){
		playercontroller = this;
	}

	//初始化
	void Start () {
		StartCoroutine (DownloadQuestionFile ());
		ani = this.GetComponent<Animator> ();
		targetPos = transform.position; 
		idleTime = toIdleTime;
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
		UpdateIdle ();

	}

	//----------------------------------------------------------------------以下是初始化属性，模拟程序关闭期间的属性变化------------------------------------------------------------------------

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

	//----------------------------------------------------------------------以下是各属性变化及UI刷新------------------------------------------------------------------------

	//成长值变化及等级变化
	void ChangeExp(int nExp){
		if(Attrs [(int)enAttribute.Hungry] >= 70) {
			Attrs [(int)enAttribute.Exp] -= nExp;
			if (Attrs [(int)enAttribute.Exp] < Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10){
				Attrs [(int)enAttribute.Exp] = Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10;
				//Attrs [(int)enAttribute.Level]--;
			}
		}
		if (Attrs [(int)enAttribute.Hungry] < 70 && Attrs [(int)enAttribute.Happy] >= 70) {
			Attrs [(int)enAttribute.Exp] += 2 * nExp;
			if (Attrs [(int)enAttribute.Exp] > Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] + 1) / 2 * 10) {
				Attrs [(int)enAttribute.Level]++;
			}
		}
		if (Attrs [(int)enAttribute.Hungry] < 70 && Attrs [(int)enAttribute.Happy] < 70) {
			Attrs [(int)enAttribute.Exp] += nExp;
			if (Attrs [(int)enAttribute.Exp] > Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] + 1) / 2 * 10) {
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


	//刷新UI
	void UpdateUI() {
		foodnum.text = string.Format("{0:D3}", Attrs [(int)enAttribute.Food]);
		levelnum.text = Attrs [(int)enAttribute.Level].ToString () + " 级";
		expnum.text = string.Format ("{0:N0}", Attrs [(int)enAttribute.Exp] - Attrs [(int)enAttribute.Level] * (Attrs [(int)enAttribute.Level] - 1) / 2 * 10) + " / " + string.Format("{0:N0}",Attrs [(int)enAttribute.Level] * 10);
		hungrynum.text = string.Format("{0:D3}", 100 - Attrs [(int)enAttribute.Hungry]) + " / 100";
		happynum.text = string.Format("{0:D3}", Attrs [(int)enAttribute.Happy]) + " / 100";
	}

	//显示状态
	public void ShowZTP(string ztp){
		GameController.gamecontroller.ShowZT (ztp);
	}


	//----------------------------------------------------------------------以下是待机状态相关------------------------------------------------------------------------

	//置为初始待机状态（在移动、点击、答题、喂食行为时均先调用）
	void DoIdle(){
		StopMove ();
		GetComponent<SpriteRenderer> ().flipX = false;
		ani.SetTrigger ("toIdle");
		bMoving = false;
		idleTime = 0.0f;
		nor = 0;
		GameController.gamecontroller.HideZT ();
		//Debug.Log ("idle");
	}


	//刷新待机动作
	void UpdateIdle(){
		ani.SetBool("bSad",(Attrs [(int)enAttribute.Happy] < 30));
		ani.SetBool("bHungry",(Attrs [(int)enAttribute.Hungry] >= 70 ));
		if (Attrs [(int)enAttribute.Happy] < 30 && !ani.GetCurrentAnimatorStateInfo (0).IsTag ("sad")) {
			ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 2));
			ani.SetTrigger ("toBeSad");
			return;
		}
		if (Attrs [(int)enAttribute.Hungry] >= 70 && !ani.GetCurrentAnimatorStateInfo (0).IsTag ("hungry")) {
			ani.SetTrigger ("toBeHungry");
			return;
		}
		if (ani.GetCurrentAnimatorStateInfo (0).IsTag ("idle")) {

			idleTime += Time.deltaTime;
			if (idleTime >= toIdleTime) {
				InitIdle (DateTime.Now);
				return;
			}
			if (nor == 0) {
				nor = UnityEngine.Random.Range (3, (int) toIdleTime + 3);
				//Debug.Log (nor);
			}
			if (idleTime >= nor) {
				ani.SetInteger ("ranAnim", UnityEngine.Random.Range (1, 8));
				//Debug.Log(ani.GetInteger("ranAnim"));
				ani.SetTrigger ("toIdleNormal");
				nor = (int) toIdleTime + 5;
			}
		}

	}
	//触发时间待机动作
	void InitIdle(DateTime now){
		ani.SetInteger ("iHour", now.Hour);
		ani.SetTrigger ("toIdleTime");
	} 
		

	//----------------------------------------------------------------------以下是用户行为之移动------------------------------------------------------------------------

	//移动到某坐标
	public void MoveToPos(Vector3 Vpos) {
		if (!bMoving) {
			DoIdle ();
			ani.SetTrigger ("toMove");
		}

		Vpos.z = 0.0f;
		targetPos = Vpos;
		bMoving = true;


		//Debug.Log ("Move");
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
	//停止移动
	void StopMove(){
		targetPos = transform.position;
	}

	//----------------------------------------------------------------------以下是用户行为之点击------------------------------------------------------------------------

	//被点击
	void OnMouseDown() {
		Vector3 pos;
		if (Input.touchCount > 0) 
		{
			pos = Input.GetTouch(0).position;
		} 
		else
		{
			pos = Input.mousePosition;
		}
		if(!IsPointerOverGameObject(pos))  {
			ChangeHappy (-iHappyValue);
			DoIdle ();
			if (Attrs [(int)enAttribute.Happy] >= 30) {
				ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 6));
				ani.SetTrigger ("toBeClickHappy");
			}
			if (Attrs [(int)enAttribute.Happy] < 30) {
				ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 4));
				ani.SetTrigger ("toBeClickSad");
			}
			GameController.gamecontroller.ShowNotice ("心情 +" + iHappyValue.ToString());
		}
	}

	//----------------------------------------------------------------------以下是用户行为之喂食和获取食物------------------------------------------------------------------------

	//喂食
	public void OnEat(){
		if (Attrs [(int)enAttribute.Happy] >= 30) {
			if (Attrs [(int)enAttribute.Food] >= 1) {
				ChangeHungry (-iFoodValue);
				ChangeFood (-1);
				ani.SetTrigger ("toEat");
				DoIdle ();
				GameController.gamecontroller.ShowNotice ("饱食度 +" + iFoodValue.ToString());
			} else {
				GameController.gamecontroller.ShowNotice ("食物不足啦！！");
			}
		}else{
			GameController.gamecontroller.ShowNotice ("不高兴！不吃饭！");
		}

	}

	//获取食物
	public void OnGetFood(){
		qbg.gameObject.SetActive (true);
		if (bAnswer) {
			nQuesIdx = RandQuestion ();
			string[] QandA = (string[])QAndAList[nQuesIdx];
			ques.text = QandA [0];
			answfield.text = "";
			bAnswer = false;
		}

		GameController.gamecontroller.ChangeFish ();
	}

	//回答后处理
	public void OnAnswer(){
		bAnswer = true;
		CloseUI ();
		DoIdle ();
		if(IsAnswerRight(nQuesIdx,answ.text)){
			ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 6));
			ani.SetTrigger ("toRight");
			ChangeFood (1);
			GameController.gamecontroller.ShowNotice ("回答正确！食物 +1");
		}else{
			ani.SetInteger ("ranAnim", UnityEngine.Random.Range (0, 6));
			ani.SetTrigger ("toWrong");
			ChangeHappy (20);
			GameController.gamecontroller.ShowNotice ("回答错误！心情 -20");
		}
	}

	//只是关闭界面
	public void CloseUI(){
		qbg.gameObject.SetActive (false);
	}
		
		


	// 随机问题序号
	public int RandQuestion() {
		return UnityEngine.Random.Range (0, QAndAList.Count);
	}

	// 判断回答是否正确
	public bool IsAnswerRight(int nQueIdx, string strAnswer) {
		string[] QandA = (string[])QAndAList[nQueIdx];
		if (strAnswer == "我想初始化"){
			InitAttr ();
			return false;
		}
		if (QandA[1] == strAnswer)
		{
			return true;
		}
		return false;
	}



	//-------------------------------------------------------------------以下是文件读取和初始化相关----------------------------------------------------

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

	//加载问题
	public bool LoadQuestion()
	{
		string strFileName = Application.persistentDataPath + "/question.txt";
		string[] strLines;
		if (File.Exists (strFileName)) 
		{
			strLines = File.ReadAllLines(strFileName);
		} 
		else
		{
			TextAsset textAsset = Resources.Load("question") as TextAsset;
			strLines = textAsset.text.Split ("\r\n".ToCharArray());
		}
		string[] aryLine;
		for (int i = 0; i < strLines.Length - 1; i++) 
		{
			aryLine = strLines[i].Split('\t');
			QAndAList.Add (aryLine);
		}
		return false;
	}

	public IEnumerator DownloadQuestionFile()
	{
		WWW www = new WWW("http://ttry.gz01.bdysite.com/question.txt");
		yield return www;
		if (www.isDone)
		{
			string strFileName = Application.persistentDataPath + "/question.txt";
			FileInfo file = new FileInfo(strFileName);
			byte[] bytes = www.bytes;
			Stream stream;
			stream = file.Create();
			stream.Write(bytes, 0, bytes.Length);
			stream.Close();
			stream.Dispose();
			LoadQuestion ();
		}
	}

	//-------------------------------------------------------------------以下是UI防穿透----------------------------------------------------

	public bool IsPointerOverGameObject(Vector2 screenPosition)  
	{  
		//实例化点击事件  
		PointerEventData eventDataCurrentPosition = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);  
		//将点击位置的屏幕坐标赋值给点击事件  
		eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);  

		List<RaycastResult> results = new List<RaycastResult>();  
		//向点击处发射射线  
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);  

		return results.Count > 0;  
	} 
}
