using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaceManager.Cars;

/// <summary>
/// Base class game controller.
/// </summary>
public class GameController :MonoBehaviour
{
	[SerializeField] KeyCode NextCarKey = KeyCode.N;
	[SerializeField] UnityEngine.UI.Button NextCarButton;
	public static GameController Instance;
	public static Car PlayerCar { get { return Instance.m_PlayerCar; } }
	public static bool RaceIsStarted { get { return true; } }
	public static bool RaceIsEnded { get { return false; } }

	Car m_PlayerCar;
	List<Car> Cars = new List<Car>();
	int CurrentCarIndex = 0;

	protected virtual void Awake ()
	{

		Instance = this;

		//Find all cars in current game.
		Cars.AddRange (GameObject.FindObjectsOfType<Car> ());
		Cars = Cars.OrderBy (c => c.name).ToList();

		foreach (var car in Cars)
		{
			var userControl = car.GetComponent<UserControl>();
			var audioListener = car.GetComponent<AudioListener>();

			if (userControl == null)
			{
				userControl = car.gameObject.AddComponent<UserControl> ();
			}

			if (audioListener == null)
			{
				audioListener = car.gameObject.AddComponent<AudioListener> ();
			}

			userControl.enabled = false;
			audioListener.enabled = false;
		}

		m_PlayerCar = Cars[0];
		m_PlayerCar.GetComponent<UserControl> ().enabled = true;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = true;

		if (NextCarButton)
        {
			NextCarButton.onClick.AddListener (NextCar);
		}
	}

	void Update () 
	{ 
		if (Input.GetKeyDown (NextCarKey))
		{
			NextCar ();
		}

	}

	private void NextCar ()
	{
		m_PlayerCar.GetComponent<UserControl> ().enabled = false;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = false;

		CurrentCarIndex = MathExtentions.LoopClamp (CurrentCarIndex + 1, 0, Cars.Count);

		m_PlayerCar = Cars[CurrentCarIndex];
		m_PlayerCar.GetComponent<UserControl> ().enabled = true;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = true;
	}
}
