using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.game;


public class UserGUI : MonoBehaviour
{
	private IUserAction action;
	private float width, height;
	private string countDownTitle;
//	private CountDown mycount;

	void Start()
	{
		countDownTitle = "Start";
		action = SSDirector.getInstance().currentScenceController as IUserAction;
	}

	float castw(float scale)
	{
		return (Screen.width - width) / scale;
	}

	float casth(float scale)
	{
		return (Screen.height - height) / scale;
	}

	void OnGUI()
	{
		width = Screen.width / 12;
		height = Screen.height / 12;

		//倒计时
		GUI.Button(new Rect(10, 130, 80, 30), "Time: "+((RoundController)SSDirector.getInstance().currentScenceController).count.ToString());
		//回合数
		GUI.Button(new Rect(10, 40, 80, 30), "Round "+((RoundController)SSDirector.getInstance().currentScenceController).getRound());
		//分数
		GUI.Button(new Rect(10, 70, 80, 30), "Score "+((RoundController)SSDirector.getInstance().currentScenceController).scoreRecorder.getScore().ToString());

		if (GUI.Button (new Rect (10, 100, 80, 30), "Restart")) {
			SSDirector.getInstance ().currentScenceController.Restart ();
		}

		if (SSDirector.getInstance().currentScenceController.state != State.WIN && SSDirector.getInstance().currentScenceController.state != State.LOSE
			&& GUI.Button(new Rect(10, 10, 80, 30), countDownTitle))
		{
			if (countDownTitle == "Start") {
				countDownTitle = "Pause";
//				while(count >= 0){
//					Gametext.text = count.ToString ();
//					StartCoroutine (StartCoutine ());
//				}
//				Gametext.text = "";
				SSDirector.getInstance().currentScenceController.Resume();
			}
			else if (countDownTitle == "Continue")
			{
				//恢复场景
				countDownTitle = "Pause";
				SSDirector.getInstance().currentScenceController.Resume();
			}
			else
			{
				//暂停场景
				countDownTitle = "Continue";
				SSDirector.getInstance().currentScenceController.Pause();
			}
		}

		if (SSDirector.getInstance().currentScenceController.state == State.WIN)//胜利
		{
			if (GUI.Button(new Rect(castw(2f), casth(6f), width, height), "Win!"))
			{
				//选择重来
				SSDirector.getInstance().currentScenceController.Restart();
//				mycount.timer = 3;
			}
		}
		else if (SSDirector.getInstance().currentScenceController.state == State.LOSE)//失败
		{
			if (GUI.Button(new Rect(castw(2f), casth(6f), width, height), "Lose!"))
			{
				SSDirector.getInstance().currentScenceController.Restart();
//				mycount.timer = 3;
			}
		}
	}
//	IEnumerator StartCoutine()  
//	{  
//
//		yield return new WaitForSeconds(1f);
//		count--;
//	} 
	void Update()
	{
		//监测用户射击
		action.shoot();
	}

}