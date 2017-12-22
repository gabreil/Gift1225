﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BGMover : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown(){
		Vector3 pos;
		if (Input.touchCount > 0) 
		{
			pos = Input.GetTouch(0).position;
		} 
		else 
		{
			pos = Input.mousePosition;
		}
		if(!IsPointerOverGameObject(pos))  
		{  
			PlayerController.playercontroller.MoveToPos (Camera.main.ScreenToWorldPoint(pos));
		} 
	}
	
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
