using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Com.game;

public enum State { WIN, LOSE, PAUSE, CONTINUE, START };

public class RoundController : MonoBehaviour, ISceneController, IUserAction{

	public DiskFactory diskFactory;
	public RoundActionManager actionManager;
	public ScoreRecorder scoreRecorder;
	private List<GameObject> disks;
	private int round;//第几个回合
	private GameObject shootAtSth;
	GameObject explosion;

	public int CoolTimes = 3;
	public Text GameText;//倒计时文本


	//游戏状态
	public State state { get; set; }

	//计时器, 每关60秒倒计时
	public int leaveSeconds;
	public int leaveSecond2;

	//用来计数，每秒自动发射一次飞碟
	public int count;

	IEnumerator DoCountDown()
	{
		while (leaveSeconds >= 0)
		{
			if (leaveSeconds >= 60) {
				GameText.text = (leaveSeconds - 60).ToString ();
			} else {
				GameText.text = "";
			}
			yield return new WaitForSeconds(1);
			leaveSeconds--;
		}
	}

	void Awake()
	{
		SSDirector director = SSDirector.getInstance();
		director.setFPS(60);
		director.currentScenceController = this;

		LoadResources();

		diskFactory = Singleton<DiskFactory>.Instance;
		scoreRecorder = Singleton<ScoreRecorder>.Instance;
		actionManager = Singleton<RoundActionManager>.Instance;

		leaveSeconds = 63;
		leaveSecond2 = 60;

		count = leaveSecond2;

		state = State.PAUSE;

		disks = new List<GameObject>();
	}


	void Start () {
		round = 1;//从第一关开始
		LoadResources();
	}

	void Update()
	{
		LaunchDisk();
		Judge();
		RecycleDisk();
	}

	public void LoadResources()
	{
		Camera.main.transform.position = new Vector3(0, 0, -15);
		explosion = Instantiate(Resources.Load("prefabs/ParticleSys"), new Vector3(-40, 0, 0), Quaternion.identity) as GameObject;

	}
	public int getRound(){
		return round;
	}
	public void shoot()//用户在游戏状态为开始或者继续时，才能左键射击
	{
		if (Input.GetMouseButtonDown(0) && (state == State.START || state == State.CONTINUE))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				if ((SSDirector.getInstance().currentScenceController.state == State.START || SSDirector.getInstance().currentScenceController.state == State.CONTINUE))
				{
					shootAtSth = hit.transform.gameObject;

					explosion.transform.position = hit.collider.gameObject.transform.position;
					explosion.GetComponent<Renderer>().material = hit.collider.gameObject.GetComponent<Renderer>().material;
					explosion.GetComponent<ParticleSystem>().Play();
				}
			}
		}
	}

	public void LaunchDisk()//每秒自动发射飞碟
	{
		if(count - leaveSeconds== 1)
		{
			count = leaveSeconds;
			GameObject disk = diskFactory.GetDisk(round);//从飞碟工厂得到飞碟
			Debug.Log(disk);
			disks.Add(disk);//飞碟进入场景
			actionManager.addRandomAction(disk);//让动作管理者设计轨迹
		}
	}

	public void RecycleDisk()//检查需不需要回收飞碟
	{
		for(int i = 0; i < disks.Count; i++)
		{
			if( disks[i].transform.position.z < -18)
			{
				diskFactory.FreeDisk(disks[i]);//让飞碟工厂回收
				disks.Remove(disks[i]);
			}
		}
	}



	public void Judge()//判断游戏状态，是否射中以及够不够分数进入下一回合
	{
		if(shootAtSth != null && shootAtSth.transform.tag == "Disk" && shootAtSth.activeInHierarchy)//射中飞碟
		{
			scoreRecorder.Record(shootAtSth);//计分
			diskFactory.FreeDisk(shootAtSth);//回收飞碟
			shootAtSth = null;//点击的物体重置为空，避免计分出错
		}

		if(scoreRecorder.getScore() > 500 * round)//每关500分才能进入下一关，重新倒数60秒
		{
			round++;
			leaveSeconds = count = 60;
		}

		if (round == 4) 
		{
			StopAllCoroutines();
			state = State.WIN;
		}
		else if (leaveSeconds == 0 && scoreRecorder.getScore() < 500 * round) //时间到，分数不够，输了
		{
			StopAllCoroutines();
			state = State.LOSE;
		} 
		else
			state = State.CONTINUE;

	}

	public void Pause()
	{
		state = State.PAUSE;
		CoolTimes = 3;
		StopAllCoroutines();
		for (int i = 0; i < disks.Count; i++)
		{
			disks[i].SetActive(false);//暂停后飞碟不可见
		}
	}
//	IEnumerator waitForOneSecond()
//	{
//		while (CoolTimes >= 0)
//		{
//			GameText.text = CoolTimes.ToString();
//			print("还剩" + CoolTimes);
//			yield return new WaitForSeconds(1);
//			CoolTimes--;
//		}
//		GameText.text = "";
//	}
//	public void myGameStart(){
//		StartCoroutine (waitForOneSecond ());
//	}
//	public void CountEnd(){
////		state = State.PAUSE;
////		CoolTimes = 3;
////		StopAllCoroutines();
////		for (int i = 0; i < disks.Count; i++)
////		{
////			disks[i].SetActive(false);//暂停后飞碟不可见
////		}
//	}
	public void Resume()
	{
		StartCoroutine(DoCountDown());         //开启协程计时
		state = State.CONTINUE;
		for (int i = 0; i < disks.Count; i++)
		{
			disks[i].SetActive(true);//恢复后飞碟可见
		}
	}

	public void Restart()
	{
		CoolTimes = 3;
		scoreRecorder.Reset();
		Application.LoadLevel(Application.loadedLevelName);
		SSDirector.getInstance().currentScenceController.state = State.START;
	}

}
