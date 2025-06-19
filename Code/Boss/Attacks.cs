/*/

Script to execute and stop Shade Lord's attacks

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Attacks : MonoBehaviour
{
	public GameObject target;
	private GameObject halo;

	// Helper variables
	private float xEdge, xCenter, yDef;
	private int sweepPos;
	private bool attacking, wait, forward, infiniteSpike, lastPhase, fireOnce, platPhase;
	private GameObject parent;

	// Unity Components
	private BoxCollider2D col;
	private Animator anim;
	private Rigidbody2D rig;
	private AudioSource aud;

	// Dicts for easy access to sound clips and objects needed for attacks
	private Dictionary<string, GameObject> atts;
	public Dictionary<string, AudioClip> sounds;
	private GameObject[] emitters = { null,null,null};

	// set variables upon awake
	private void Awake()
	{
		// Helper variables
		xEdge = 18; xCenter = 23.5f; yDef = 75.23f;
		sweepPos = 0;
		attacking = false;
		wait = false;
		infiniteSpike = false;
		lastPhase = false;
		platPhase = false;
		fireOnce = false;
		parent = GameObject.Find("HoldAttacks");
		halo = GameObject.Find("ShadeLord/Halo");

		// Unity Components
		col = GetComponent<BoxCollider2D>();
		anim = GetComponent<Animator>();
		rig = GetComponent<Rigidbody2D>();
		aud = GetComponent<AudioSource>();

		target = HeroController.instance.gameObject;
		aud.outputAudioMixerGroup = target.GetComponent<AudioSource>().outputAudioMixerGroup;

		// Load dicts
		atts = new Dictionary<string, GameObject>();
		sounds = new Dictionary<string, AudioClip>();

		// put scripts on attacks
		foreach (Transform t in gameObject.transform)
		{
			switch (t.name)
			{
				case "Spike":
					t.gameObject.AddComponent<Spike>();
					foreach (Transform spike in t.transform)
						spike.gameObject.AddComponent<Spike>();
					break;
				case "BurstSpike":
					t.gameObject.AddComponent<FaceSpike>();
					break;
				case "VoidBurst":
					t.gameObject.AddComponent<VoidBurst>();
					break;
				case "BeamOrigin":
					t.Find("Offset").gameObject.AddComponent<Beam>();
					break;
				case "VoidCircle":
					t.gameObject.AddComponent<VoidCircle>();
					t.gameObject.AddComponent<Spin>();
					break;
			}
		}

		// Get audio clips
		foreach (AudioSource s in GameObject.Find("ShadeLord/SFX").GetComponents<AudioSource>())
		{
			sounds.Add(s.clip.name, s.clip);
		}
		GameObject.Find("ShadeLord/BeamOrigin/Offset").GetComponent<Beam>().SetSounds(sounds["BeamCharge"], sounds["BeamBlast"]);

        // Get attacks
        atts.Add("Beam", GameObject.Find("ShadeLord/BeamOrigin/Offset"));
        atts["Beam"].SetActive(false);
        List<string> names = new List<string> { "Dash", "CrossSlash", "Sweep", "BurstSpike", "Spike", "BeamOrigin", "VoidBurst", "VoidCircle", "TendrilWindup" };
		foreach (string n in names)
		{
			atts.Add(n, GameObject.Find("ShadeLord/" + n));
			atts[n].SetActive(false);
		}
		atts.Add("TendrilBurst", GameObject.Find("ShadeLord/Tendrils"));
		atts["TendrilBurst"].GetComponent<BoxCollider2D>().enabled = false;

		atts.Add("DashTelegraph", GameObject.Find("DashTelegraphParticles"));
		for (int i = 0; i < 3; i++)
		{
			emitters[i] = Instantiate(atts["DashTelegraph"]);
		}
		atts["DashTelegraph"].SetActive(false);
    }

	// Attacks
	// sharp shadow from one side of the stage to the other
	public void Dash()
	{
		attacking = true;
		forward = false;
		StartCoroutine(Dash());

		// dash towards player location 3 times
		IEnumerator Dash()
		{
			float dir = 0, x, y = 0;
			
			atts["DashTelegraph"].SetActive(true);

			// start on side closer to player
			bool goright = target.transform.position.x < xCenter;
            if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge - 30);
				dir = 0;
				x = -1;
			}
			else
			{

				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge + 30);
				dir = (float)Math.PI;
				x = 1;
			}
			arrive();
			yield return new WaitWhile(() => wait);
			halo.SetActive(false);

			// setup
			anim.Play("DashLoop");
			transform.SetPositionY(68.04f);
			y = 0;
			halo.transform.localPosition = new Vector2(11.23f, 0.04f);
			col.offset = new Vector2(6, -.07f);
			col.size = new Vector2(19, 2.36f);

			// go
			for (int i = 0; i < 3; i++)
			{
				// calc angles and stuff
				if (x == 0)
					x = .001f;
				dir = (float)Math.Atan((y / x)) * 180.0f / (float)Mathf.PI;//*
				if (x > 0)
				{
					transform.SetScaleX(-1);
				}
				else
					transform.SetScaleX(1);
				transform.SetRotationZ(dir);


				// telegraph
				//emitters[i].transform.position = new Vector3(xCenter-xEdge, yDef+4.5f, 0);
				//emitters[i].transform.position = (new Vector3(xCenter, yDef + 2, 0) + transform.position)/2;
				//emitters[i].transform.position = transform.position;
				
				emitters[i].transform.position = new Vector2(
					Mathf.Clamp(transform.GetPositionX(), xCenter-xEdge-5, xCenter + xEdge+5), 
					Mathf.Clamp(transform.GetPositionY(), yDef-10f, yDef+4.5f));//*/
				emitters[i].SetActive(true);
				ParticleSystem.EmissionModule em = emitters[i].GetComponent<ParticleSystem>().emission;
				ParticleSystem.ShapeModule shape = emitters[i].GetComponent<ParticleSystem>().shape;
				
				
				if (x > 0)
				{
					shape.rotation = new Vector3(0, -90-dir, 0);
				}
				else
				{
					shape.rotation = new Vector3(0, 90-dir, 0);
				}
				//shape.rotation = new Vector3(0, 180, 0);

				em.rateOverTime = 100f;
				yield return new WaitForSeconds(1f);
				em.rateOverTime = 0f;

				// dash
				playSound("DashStart");
				atts["Dash"].SetActive(true);
				rig.velocity = new Vector2(x, y).normalized * -70f;


				yield return new WaitForSeconds(1f);


				// dash finish
				atts["Dash"].SetActive(false);
				rig.velocity = new Vector2(0f, 0f);

				goright = !goright;

				Vector2 newPos = new Vector2(0,yDef+4.5f);
				if (goright)
				{
					//transform.SetPositionX(xCenter - xEdge - 15);
					newPos.x = xCenter - xEdge-5;
					//x = -2 * xEdge;
				}
				else
				{
					//transform.SetPositionX(xCenter + xEdge + 15);

					newPos.x = xCenter + xEdge+5;
					//x = 2 * xEdge;
				}

				x = newPos.x - target.transform.GetPositionX();
				y = newPos.y - target.transform.GetPositionY();
				
				transform.SetPosition2D(newPos + (new Vector2(x, y).normalized * 16.6f));
			}

			// end
			halo.transform.localPosition = new Vector2(0f, -2.62f);
			transform.SetRotationZ(0);

			transform.SetScaleX(1);
			Hide();
			attacking = false;
		}
	}
	// Facing the camera, slash in an x in front of self, then sweeping slash across the floor
	public void CrossSlash()
	{
		attacking = true;
		forward = true;
		StartCoroutine(CrossSlash());
		IEnumerator CrossSlash()
		{
			// go to player
			float x = target.transform.position.x, aDelay, sDelay;
			if (x > (xCenter + xEdge - 5f))
				x = xCenter + xEdge - 5f;
			else if (x < (xCenter - xEdge + 5f))
				x = xCenter - xEdge + 5f;
			transform.SetPositionX(x);
			arrive();

			yield return new WaitUntil(() => !wait);

			// cross slash windup
			anim.Play("CrossSlashCharge1");

			aDelay = 6; sDelay = 2.95f;
			yield return new WaitForSeconds(sDelay / 12f);

			// cross slash
			playSound("CrossSlashAttack1");

			yield return new WaitForSeconds((aDelay - sDelay) / 12f);

			anim.Play("CrossSlashAttack1");
			atts["CrossSlash"].SetActive(true);

			yield return new WaitForSeconds(2 / 12f);

			atts["CrossSlash"].SetActive(false);

			yield return new WaitForSeconds(1 / 12f);

			// sweep windup
			if (target.transform.position.x < transform.position.x)
				transform.localScale = new Vector3(-1, 1, 1);

			anim.Play("CrossSlashCharge2");
			aDelay = 6.5f; sDelay = 3.45f;
			yield return new WaitForSeconds(sDelay / 12f);

			// sweep
			playSound("CrossSlashAttack2");

			yield return new WaitForSeconds((aDelay - sDelay) / 12f);

			anim.Play("CrossSlashAttack2");
			atts["Sweep"].SetActive(true);

			yield return new WaitForSeconds(2 / 12f);

			atts["Sweep"].SetActive(false);

			yield return new WaitForSeconds(1 / 12f);

			// end
			anim.Play("CrossSlashEnd");
			yield return new WaitForSeconds(2 / 12f);

			leave();
			yield return new WaitUntil(() => !wait);

			attacking = false;
		}
	}
	// Spikes shoot out of the floor
	public void Spikes()
	{
		attacking = true;
		forward = true;
		StartCoroutine(Spikes());

		IEnumerator Spikes()
		{
			// go to random location
			if (infiniteSpike)
				transform.SetPositionX(xCenter);
			else
				transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			Modding.Logger.Log(xCenter - xEdge + 4);
			arrive();
			yield return new WaitUntil(() => !wait);

            // first set
            playSound("BeamCharge");
            anim.Play("SpikeWindup");
			// generate spikes
			float offset = xCenter - xEdge + UnityEngine.Random.Range(0, 2.4f);
			yield return new WaitForSeconds(.7f);
			//playSound("Scream");
			anim.Play("SpikeLoop");

            //yield return new WaitForSeconds(.5f);
            /*
			playSound("BeamCharge");
			while (offset < xCenter + xEdge)
			{
				GameObject s = Instantiate(atts["Spike"], parent.transform);
				s.SetActive(true);
				s.transform.SetPosition2D(offset, 66.42f);
				offset += 2f;
			}
			yield return new WaitForSeconds(.7f);
			// spikes go up
			//playSound("Scream");
			
			yield return new WaitForSeconds(1f + 3/12f);//*/

            // fire 3 sets of spikes with random offset
            int i = 3;
			float randOffset = UnityEngine.Random.Range(0, 1f);
            while (i > 0 || infiniteSpike)
			{
				// generate spikes
				offset = xCenter - xEdge + i % 2 + randOffset;//UnityEngine.Random.Range(0, 2.4f);

				
				while (offset < xCenter + xEdge)
				{
					GameObject s = Instantiate(atts["Spike"], parent.transform);
					s.SetActive(true);
					s.transform.SetPosition2D(offset, 66.42f);
					offset += 2f;
				}
				yield return new WaitForSeconds(Spike.spawnTime);
				// spikes go up
				playSound("SpikeUpLower");
				yield return new WaitForSeconds(Spike.upTime + Spike.activeTime + Spike.downTime + .1f);
				i--;
			}

            anim.Play("SpikeRetract");
            yield return new WaitForSeconds(.333f);

            // end
            leave();
			yield return new WaitUntil(() => !wait);
			
			attacking = false;
		}
	}
	// fire several spikes out of its face
	public void FaceSpikes()
	{
		attacking = true;
		forward = true;
		StartCoroutine(FaceSpikes());

		IEnumerator FaceSpikes()
		{
			// go to random location
			int seg = 5;
			float offset = UnityEngine.Random.Range(0, 360f / seg);
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - 14, xCenter + 14));
			arrive();
			yield return new WaitUntil(() => !wait);
			eyes(false);
			yield return new WaitUntil(() => !wait);

			// fire 4 sets of face spikes
			playSound("BeamCharge");
			for (int i = 0; i < 4; i++)
			{
				for (int k = 0; k < seg; k++)
				{
					GameObject s = Instantiate(atts["BurstSpike"], parent.transform);
					s.SetActive(true);
					s.transform.SetPosition3D(transform.position.x, transform.position.y - 2.22f, atts["BurstSpike"].transform.position.z);
					s.transform.Rotate(new Vector3(0, 0, offset + k * 360f / seg));
				}
				yield return new WaitForSeconds(.7f);
				playSound("SpikeUpLower");
				offset += UnityEngine.Random.Range(10, (360f / seg) - 10);

				yield return new WaitForSeconds(.3f);
			}

			// end
			yield return new WaitForSeconds(2f);
			eyes(true);
			yield return new WaitUntil(() => !wait);
			leave();//*/
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	// tendrils emerge from the sides of the boss
	public void TendrilBurst()
	{
		attacking = true;
		forward = true;
		StartCoroutine(TendrilBurst());
		IEnumerator TendrilBurst()
		{
			// setup
			transform.SetPositionX(xCenter);
			if(platPhase)
				transform.SetPositionX(target.transform.GetPositionX());
			arrive();

			yield return new WaitUntil(() => !wait);

			// windup
			GameObject tendrils = atts["TendrilBurst"];
			tendrils.transform.SetScaleX(.1f);
			tendrils.transform.SetScaleY(.1f);
            anim.Play("NeutralSquint");

            //anim.Play("NeutralClose");
            atts["TendrilWindup"].SetActive(true);
			Transform windup = atts["TendrilWindup"].transform.GetChild(1);
			for (float f = .5f; f < 1.5f; f += .2f)
			{
				windup.SetScaleX(f);

				yield return new WaitForSeconds(1 / 12f);
			}
			windup.SetScaleX(1.5f);
			yield return new WaitForSeconds(.5f);

			// attack
			float increment = .9f/(12*.3f);
			tendrils.SetActive(true);
			anim.Play("NeutralOpen");
			//playSound("Scream");
			playSound("TendrilsEmerge");
			playSound("TendrilWhip");
			for (float s = .1f; s < 1; s += increment)
			{
				tendrils.transform.SetScaleX(s);
				tendrils.transform.SetScaleY(s);
				yield return new WaitForSeconds(1/12f);
			}

			tendrils.GetComponent<PolygonCollider2D>().enabled = true;
			yield return new WaitForSeconds(1f);
			tendrils.GetComponent<PolygonCollider2D>().enabled = false;

			anim.Play("NeutralIdle");
			for (float s = 1f; s > 0; s -= increment)
			{
				tendrils.transform.SetScaleX(s);
				tendrils.transform.SetScaleY(s);
				windup.SetScaleX(1.5f*s);
				yield return new WaitForSeconds(1 / 12f);
			}
			yield return new WaitForSeconds(.3f);
			atts["TendrilWindup"].SetActive(false);
			tendrils.SetActive(false);
			// end
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	// pure vessel focus
	public void VoidCircles()
	{
		attacking = true;
		forward = true;
		if(lastPhase)
			StartCoroutine(SpamCircles());
		else
			StartCoroutine(VoidCircles());

		IEnumerator VoidCircles()
		{
			// setup
			halo.GetComponent<SpriteRenderer>().enabled = false;
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			if (lastPhase)
			{
				switch (UnityEngine.Random.Range(0,3))
				{
					case 0:
						transform.SetPositionX(xCenter);
						break;
					case 1:
						transform.SetPositionX(xCenter - xEdge + 4);
						break;
					case 2:
						transform.SetPositionX(xCenter + xEdge - 4);
						break;
				}
			}
			arrive();
			yield return new WaitWhile(() => wait);
			playSound("BeamCharge");
			anim.Play("RoarWait");
			// spawn circle on self
			GameObject obj = Instantiate(atts["VoidCircle"], parent.transform);
			obj.GetComponent<VoidCircle>().size = 1f;
			obj.transform.SetPosition3D(transform.GetPositionX(), transform.GetPositionY() - 2.37f, transform.GetPositionZ()+.001f);
			obj.SetActive(true);
			obj.GetComponent<VoidCircle>().Appear();

			yield return new WaitForSeconds(1f);

			anim.Play("Roar");
			playSound("Scream");
			playSound("BeamBlast");
			obj.GetComponent<VoidCircle>().Fire();
			//yield return new WaitForSeconds(.5f);

			// pick random points to spawn
			float curX = xCenter - xEdge-2.5f + UnityEngine.Random.Range(5f,9f);
			while (curX< xCenter + xEdge)
			{
				StartCoroutine(MakeCircle(curX, UnityEngine.Random.Range(66f, 76f), .7f));
				yield return new WaitForSeconds(.07f);
				curX += UnityEngine.Random.Range(5f, 9f);
			}
			yield return new WaitForSeconds(.5f);
			anim.Play("NeutralIdle");
			yield return new WaitForSeconds(.5f);

			// end
			leave();
			yield return new WaitUntil(() => !wait);
			halo.GetComponent<SpriteRenderer>().enabled = true;
			attacking = false;
		}
		IEnumerator SpamCircles()
		{
			float tileSize = 10;
			int xMax = 5;
			int yMax = 5;
			for (int n = 0; n < xMax; n++)
			{
				for (int m = 0; m < yMax; m++)
				{
					float x = target.transform.position.x + n*tileSize - xMax/2 * tileSize + UnityEngine.Random.Range(-5f, 5f);
					float y = target.transform.position.y + m*tileSize - yMax/2 * tileSize + UnityEngine.Random.Range(-5f, 5f);
					StartCoroutine(MakeCircle(x, y, 1f));
				}
			}

			yield return new WaitForSeconds(1f);
			StartCoroutine(SpamCircles());
		}
		IEnumerator MakeCircle(float x, float y, float wait)
		{
			GameObject obj = Instantiate(atts["VoidCircle"], parent.transform);
			obj.transform.SetPosition2D(x, y);
			obj.SetActive(true);
			obj.GetComponent<VoidCircle>().size = .3f;
			obj.GetComponent<VoidCircle>().Appear();

			yield return new WaitForSeconds(wait);

			obj.GetComponent<VoidCircle>().Fire();
			//playSound("BeamBlast");
		}
	}
	// void beams
	public void VoidBeams()
	{
		attacking = true;
		forward = true;
		StartCoroutine(VoidBeams());
		IEnumerator VoidBeams()
		{
			yield return new WaitForSeconds(1f);
		}
	}
	// fire beam at player that remains in place while several vertical beams sequentially fire at the player 
	public void AimBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(AimBeam());

		IEnumerator AimBeam()
		{
			List<GameObject> beams = new List<GameObject>();
			// go to side further away from player
			bool goright = target.transform.position.x > xCenter;
            GameObject beam = atts["BeamOrigin"];
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge + 2.5f);
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge - 2.5f);
			}

            arrive();
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			anim.Play("Body");
			head.enabled = true;

			// FIRE MAIN BEAM
			// targeting
			float deg = getAngle(atts["Beam"].transform, target);

			deg = (Math.Min(Math.Max(deg, -35), 17));
			if (!goright)
			{
				//-35, 17
				deg *= -1;
			}

			// charge
			//playSound("BeamCharge");
			atts["BeamOrigin"].SetActive(true);
			beam.transform.SetRotationZ(deg);
			
			beam = Instantiate(atts["Beam"], parent.transform, true);
			beam.SetActive(true);
			beam.GetComponent<Beam>().go(4f, true);

            yield return new WaitForSeconds(1f);

			// fire vertical beams
			for (int i = 0; i < 3; i++)
			{
				// fire
				spawnVerticalBeam(target.transform.GetPositionX());
				yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(5/12f);
            atts["BeamOrigin"].SetActive(false);
			// end
			eyes(true);
            yield return new WaitForSeconds(7/12f);
            //yield return new WaitUntil(() => !wait);
            leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	private void spawnVerticalBeam(float x)
    {
        GameObject beam = Instantiate(atts["Beam"], parent.transform);

		beam.transform.SetRotationZ(90f);
		beam.transform.SetPositionY(yDef - 18f);

		beam.transform.SetPositionX(x);

		beam.SetActive(true);
        beam.gameObject.GetComponent<Beam>().go(1.5f, false);
	}

	// UNUSED
	// spawn several orbs that explode into crosses
	public void VoidBurst()
	{
		attacking = true;
		forward = true;
		StartCoroutine(VoidBurst());

		IEnumerator VoidBurst()
		{
			// pick rand location
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			arrive();
			yield return new WaitUntil(() => !wait);

			// spawn orbs
			//float rotation = UnityEngine.Random.Range(0f,359f);
			for (int k = (int)(xEdge / 6); k > 0; k--)
			{
				// pick random location
				float x = UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4),
					  y = UnityEngine.Random.Range(yDef - 7, yDef + 3),
					  rotation = UnityEngine.Random.Range(0f, 359f);
				StartCoroutine(SpawnVoidBurst(x, y, 0, rotation, 1.5f));
				StartCoroutine(SpawnVoidBurst(x, y, .005f, rotation + 45, 1.5f));
			}


			yield return new WaitForSeconds(4f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;

		}
		IEnumerator SpawnVoidBurst(float x, float y, float z, float r, float wait)
		{
			List<GameObject> burst = new List<GameObject>();
			for (int i = 0; i < 4; i++)
				burst.Add(Instantiate(atts["VoidBurst"], parent.transform));

			for (int i = 0; i < 4; i++)
			{
				burst[i].transform.SetPosition3D(x, y, z);
				burst[i].transform.SetRotationZ(r + 90 * i);
				burst[i].SetActive(true);
			}
			yield return new WaitForSeconds(wait);

			for (int i = 0; i < 4; i++)
			{
				burst[i].GetComponent<VoidBurst>().Fire();
			}
			playSound("BeamBlast");
		}
	}
	// fire beam that sweeps across the screen
	public void SweepBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(SweepBeam());

		IEnumerator SweepBeam()
		{
			// pick location
			bool goright = true;
			if (lastPhase)
			{
				int i = UnityEngine.Random.Range(0, 2);
				if (i >= sweepPos)
					i++;
				sweepPos = i;
				switch (i)
				{
					case 0: // center
						transform.SetPositionX(xCenter);
						goright = UnityEngine.Random.Range(0, 2) == 0;
						break;
					case 1: // on left
						transform.SetPositionX(xCenter - xEdge + 9);
						goright = true;
						break;
					case 2: // on right
						transform.SetPositionX(xCenter + xEdge - 9);
						goright = false;
						break;
				}
			}
			else if (platPhase)
			{
				transform.SetPositionX(xCenter);
				goright = target.transform.position.x > xCenter;
			}
			else
			{
				goright = UnityEngine.Random.Range(0, 1f) > .5f;
				if (Math.Abs(target.transform.position.x - xCenter) > xEdge + 15)
				{
					goright = target.transform.position.x > xCenter;
				}


				if (goright)
				{
					transform.SetPositionX(xCenter - xEdge + 5);
				}
				else
				{
					transform.SetPositionX(xCenter + xEdge - 5);
				}
			}

			if (!goright)
				transform.localScale = new Vector3(-1, 1, 1);
			else
				transform.localScale = new Vector3(1, 1, 1);

			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			arrive();
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			// charge
			beam.SetRotationZ(0f);
			anim.Play("SweepBeamCharge");
			playSound("BeamCharge");
			atts["BeamOrigin"].SetActive(true);
			yield return new WaitForSeconds(1f);

			// fire
			playSound("BeamBlast");
			aud.clip = sounds["BeamLoop"];
			aud.loop = true;
			anim.Play("Body");
			head.enabled = true;

			// rotation
			float speed = -120f / 90f;

			for (float f = 0f; f >= -60f; f += speed)
			{
				beam.SetRotationZ(f);
				yield return new WaitForSeconds(1 / 60f);
			}

			// end
			atts["BeamOrigin"].SetActive(false);
			aud.Stop();
			aud.loop = false;
			eyes(true);
			yield return new WaitUntil(() => !wait);

			yield return new WaitForSeconds(3 / 12f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	// Misc Public functions 
	public void Stop()
	{
		StopAllCoroutines();
		foreach (Transform c in parent.GetComponentsInChildren<Transform>())
		{
			if (!c.gameObject.name.Equals(parent.name))
			{
				Destroy(c.gameObject);
			}
		}
		foreach (GameObject g in atts.Values)
		{
			g.SetActive(false);
		}
		attacking = false;
		//Hide();
	}
	public void Hide()
	{
		anim.Play("Nothing");
		col.enabled = false;
	}
	public void Phase(int phase)
	{
		switch (phase)
		{
			case 0:
				xEdge = 18;
				xCenter = 23.5f;
				yDef = 75.23f;

				infiniteSpike = false;
				lastPhase = false;
				break;
			case 2:
				infiniteSpike = true;
				break;
			case 3:
				infiniteSpike = false;
				//platPhase = true;
				//xEdge = 43.5f;
				break;
			case 4:
				xEdge = 17.71f; 
				xCenter = GameObject.Find("Terrain/Area3").transform.GetPositionX();
				//yDef = 14f;

				lastPhase = true;
				platPhase = false;
				fireOnce = true;
				break;
		}
	}

	public bool isAttacking()
	{
		return attacking;
	}

	// Common Coroutines
	private void eyes(bool open)
	{
		StartCoroutine(eyes(open));
		IEnumerator eyes(bool open)
		{
			wait = true;
			if (open)
			{
				if (forward)
					anim.Play("NeutralOpen");
				else
					anim.Play("SideOpen");
			}
			else
			{
				if (forward)
					anim.Play("NeutralClose");
				else
					anim.Play("SideClose");
			}
			yield return new WaitForSeconds(3 / 12f);
			wait = false;
		}
	}
	private void leave()
	{
		StartCoroutine(leave());
		IEnumerator leave()
		{
			wait = true;
			GetComponent<BoxCollider2D>().enabled = false;
            SpriteRenderer haloSprite = halo.GetComponent<SpriteRenderer>();
            if (forward)
            {
                anim.Play("NeutralDisappear");
            }
            else
            {
                anim.Play("SideDisappear");
            }
            int iters = 10;
            for (int i = iters-1; i >= 0; i--)
            {
                haloSprite.color = new Color(1, 1, 1, 1 * i / (float)iters);
                yield return new WaitForSeconds(1 / 60f);
            }
            yield return new WaitForSeconds(1 / 12f);
            wait = false;
		}
	}
	private void arrive()
	{
		StartCoroutine(arriveRoutine(yDef));
	}
	private void arrive(float height)
	{
		StartCoroutine(arriveRoutine(height));
	}
	private IEnumerator arriveRoutine(float max)
	{
		wait = true;
		Modding.Logger.Log("arrive");
        resetHitBox();
        /*
		transform.SetPositionY(max-25f);
		halo.SetActive(true);
		if (forward)
		{
			anim.Play("NeutralUp");
		}
		else
		{
			anim.Play("SideUp");
		}
		rig.velocity = new Vector2(0f, 40f);
		yield return new WaitUntil(() => transform.position.y > max - 5);
		if (forward)
		{
			anim.Play("NeutralArrive");
		}
		else
		{
			anim.Play("SideArrive");
		}
		yield return new WaitUntil(() => transform.position.y > max);
		rig.velocity = new Vector2(0f, 0f);
		transform.SetPositionY(max);//*/
        SpriteRenderer haloSprite = halo.GetComponent<SpriteRenderer>();
        halo.SetActive(true);
        haloSprite.color = new Color(1, 1, 1, 0);
        transform.SetPositionY(max);

        if (forward)
        {
            anim.Play("NeutralAppear");
        }
        else
        {
            anim.Play("SideAppear");
        }//*/

        int iters = 10;
        for (int i = 1; i <= iters; i++)
		{
            haloSprite.color = new Color(1, 1, 1, i/ (float)iters);
            yield return new WaitForSeconds(1 / 60f);
		}
		yield return new WaitForSeconds(1/12f);
        GetComponent<BoxCollider2D>().enabled = true;
        wait = false;
	}

	// helpers
	private void rotate(Transform t, float r)
	{
		t.Rotate(new Vector3(0, 0, t.rotation.z + r));
	}
	public void playSound(string clip)
	{
		//aud.clip = sounds[clip];
		aud.GetComponent<AudioSource>().PlayOneShot(sounds[clip]);
	}
	private void resetHitBox()
	{
		col.size = new Vector2(3.86f, 4f);
		col.offset = new Vector2(0f, -1.96f);
		col.enabled = true;
	}

	private float getAngle(Transform from, Transform to)
	{
		float x = (transform.position.x) - (to.transform.position.x);
		float y = (transform.position.y) - (to.transform.position.y+.5f);
		return (float)(Math.Atan((y / x)) * 180.0 / Math.PI);
	}
	private float getAngle(GameObject from, GameObject to)
	{
		return getAngle(from.transform, to.transform);
	}
	private float getAngle(Transform from, GameObject to)
	{
		return getAngle(from, to.transform);
	}
	private float getAngle(GameObject from, Transform to)
	{
		return getAngle(from.transform, to);
	}
}
