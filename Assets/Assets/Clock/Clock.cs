using System;
using UnityEngine;

namespace ClockSample
{
	public class Clock : MonoBehaviour
	{
		public Transform handHours;
		public Transform handMinutes;
		public Transform handSeconds;

		public AudioSource tickAudio;
		public float tickInterval = 1f;

		private float timer;

		private void Update()
		{
			timer += Time.deltaTime;

			if (timer >= tickInterval)
			{
				timer = 0f;
				UpdateHands();
			}
		}

		void UpdateHands()
		{
			float handRotationHours   = DateTime.Now.Hour * 30;
			float handRotationMinutes = DateTime.Now.Minute * 6;
			float handRotationSeconds = DateTime.Now.Second * 6;

			if (handHours)
				handHours.localEulerAngles = new Vector3(0, 0, handRotationHours);

			if (handMinutes)
				handMinutes.localEulerAngles = new Vector3(0, 0, handRotationMinutes);

			if (handSeconds)
				handSeconds.localEulerAngles = new Vector3(0, 0, handRotationSeconds);

			if (tickAudio != null)
				tickAudio.PlayOneShot(tickAudio.clip);
		}
	}
}