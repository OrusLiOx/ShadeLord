using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimations : MonoBehaviour
{
	private Attacks att;
	// Start is called before the first frame update
	void Start()
	{
		att = gameObject.GetComponent<Attacks>();
	}

	// Update is called once per frame
	void Update()
	{
		if (!att.attacking)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
				att.Dash();
			if (Input.GetKeyDown(KeyCode.Alpha2))
				att.CrossSlash();
			if (Input.GetKeyDown(KeyCode.Alpha3))
				att.FaceSpikes();
			if (Input.GetKeyDown(KeyCode.Alpha4))
				att.Spikes();
			if (Input.GetKeyDown(KeyCode.Alpha5))
				att.SweepBeam();
			if (Input.GetKeyDown(KeyCode.Alpha6))
				att.AimBeam();
		}
		if (Input.GetKeyDown(KeyCode.Space))
			att.Stop();

	}
}
