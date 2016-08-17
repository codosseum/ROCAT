﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO; //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System; //Exception
using System.Text;
using MiniJSON;
//using CityCreater;

public class CameraMove : MonoBehaviour {
	const float SPEED = 0.1f;
	public Canvas canvas;
    public Canvas canvas2;
	public Text file_name;
    public Text block_name;
	private string src_txt = "";
	public CityCreater cc;
	public string ta;
	public Color preColor;
	public Material viewMaterial;
	private bool view_src;

    public bool isControlAvailable = false;
    private bool isMouseAvailable = true;
	private Building selectedBuilding;
    private Block selectedBlock;

	private float rotationY = 0f;
	private const float CAMERA_SPEED = 500f;
	private const float CAMERA_CONTROL_SENSITIVITY = 3F;
	private const float MIN_ROTATION_Y = -30F;
	private const float MAX_ROTATION_Y = 30F;

	// Use this for initialization
	void Start () {
		view_src = false;
		cc = GameObject.Find ("CityCreater").GetComponent<CityCreater> ();
		foreach( Transform child in canvas.transform){
			file_name = child.gameObject.GetComponent<Text>();
            //block_name = child.gameObject.GetComponent<Text>();
            file_name.text = "";
            //block_name.text = "";
		}
        foreach (Transform child in canvas2.transform)
        {
            block_name = child.gameObject.GetComponent<Text>();
            block_name.text = "";
        }

    }

	void Update ()
	{
        if (!isControlAvailable) { return; }
        ControlByKeyboard();
		ControlByMouse();
	}

	private void ControlByKeyboard()
	{
		Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
		Vector3 velocity = Vector3.zero;

		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			velocity += transform.forward * CAMERA_SPEED;
		}
		if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			velocity += transform.forward * CAMERA_SPEED * -1;
		}

		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			velocity += transform.right *  CAMERA_SPEED * -1;
		}
		if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			velocity += transform.right * CAMERA_SPEED;
		}

		rigidBody.velocity = velocity;

		if (Input.GetKey(KeyCode.Space))
		{
			rigidBody.velocity = transform.up * CAMERA_SPEED * (Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
		}

		if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
		{
			isMouseAvailable = !isMouseAvailable;
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			view_src = !view_src;
		}
	}

	private void ControlByMouse()
	{
		Building building = GetRaycastHitBuilding();
        Block block = GetRaycastHitBlock();
        HighlighMouseOverBuilding(building);
        HighlighMouseOverBlock(block);

        if (Input.GetMouseButtonDown(0))
		{
			MouseClicked(building);
		}

		if (!isMouseAvailable) {return;}

		float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * CAMERA_CONTROL_SENSITIVITY;
		rotationY += Input.GetAxis("Mouse Y") * CAMERA_CONTROL_SENSITIVITY;
		rotationY = Mathf.Clamp(rotationY, MIN_ROTATION_Y, MAX_ROTATION_Y);
		transform.localEulerAngles = new Vector3(rotationY * -1, rotationX, 0);
	}

	private void MouseClicked(Building building)
	{
        if (building == null) { return; }
        string path = SearchPathFromFileName(building.transform.name);
        #if UNITY_EDITOR
                Debug.Log(path);
        #else
			        Application.ExternalCall("OnBuildingClick", path);
        #endif

        /*
		if (building == null) {return;}
		string path = SearchPathFromFileName(building.transform.name);
		src_txt = ReadFile(path);
        */
    }

    private void HighlighMouseOverBuilding(Building building)
	{
		if (building != null)
		{
			if (selectedBuilding == null || selectedBuilding != building){
				file_name.text = building.transform.name;

				if (selectedBuilding)
				{
					selectedBuilding.Deselected();
				}

				selectedBuilding = building;
				selectedBuilding.Selected();
			}
		}
		else
		{
			if (selectedBuilding)
			{
				selectedBuilding.Deselected();
				selectedBuilding = null;
			}
			file_name.text = "";
		}
	}

    private void HighlighMouseOverBlock(Block block)
    {
        if (block != null)
        {
            if (selectedBlock == null || selectedBlock != block)
            {
                block_name.text = block.transform.name;

                if (selectedBlock)
                {
                    selectedBlock.Deselected();
                }

                selectedBlock = block;
                selectedBlock.Selected();
            }
        }
        else
        {
            if (selectedBlock)
            {
                selectedBlock.Deselected();
                selectedBlock = null;
            }
            block_name.text = "";
        }
    }

    private Building GetRaycastHitBuilding()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 10000)) {
			Building hitBuilding = hit.transform.GetComponent<Building>();
            MouseClicked(hitBuilding);
			return hitBuilding;
		}

		return null;
	}

    private Block GetRaycastHitBlock()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Block hitBlock = hit.transform.GetComponent<Block>();
            return hitBlock;
        }

        return null;
    }

	void OnGUI()
	{
		if(view_src)
			//src_txt = GUI.TextArea (new Rect (5, 5, Screen.width-10, Screen.height-100), src_txt);
			src_txt = GUI.TextArea (new Rect (5, 5, Screen.width-10, Screen.height-100), src_txt.Substring(0,Math.Min(10000,src_txt.Length)));

	}

    /*
	string ReadFile(string path){
		string st = "";
		try {
		// FileReadTest.txtファイルを読み込む
		FileInfo fi = new FileInfo(path);


			// 一行毎読み込み
			using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8)){
				st = sr.ReadToEnd();
			}
		} catch (Exception e){
			// 改行コード
			st += SetDefaultText();
		}

		return st;
	}
    */

	string SearchPathFromFileName(string file_name){
		string path = "";
		IList buildings = cc.GetCity()["buildings"] as IList;
		foreach (Dictionary<string,object> building in buildings) {
			if(building["name"].ToString() == file_name){
				path = building["path"].ToString();
			}
		}
		return path;

	}

	// 改行コード処理
	string SetDefaultText(){
		return "cant read\n";
	}
}
