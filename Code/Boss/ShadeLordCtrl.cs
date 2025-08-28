using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UObject = UnityEngine.Object;
using ShadeLord.Setup;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Audio;

class ShadeLordCtrl : MonoBehaviour
{
	private int actionState = -1;
	// generic unity stuff
	public Animator anim;
	public BoxCollider2D boxCol;
	public ParticleSystem particles;
	public ParticleSystem.ShapeModule particleEm;
	private Coroutine co;

	// hollow knight stuff
	private GameObject player;
	private HeroController hc;
	public HealthManager health;
	private GameObject hitEffect;
	private ExtraDamageable extDmg;

	// properties
	private GameObject head, title;
	private List<Action> atts;
	public int[] hpMarkers = { 50,50,50,50,300};
	//public int[] hpMarkers = { 400, 450, 300, 750, 281 };
	private System.Random rand;
	
	public Attacks attacks;
	private SLHelper helper;

    private GameObject area1CamLock = GameObject.Find("Terrain/Area2/CameraLock");
    private GameObject transCamLock = GameObject.Find("Terrain/ToArea3/CameraLock");

	public int phase;

	// setting up stuff
	void Awake()
	{
		// generic unity stuff
		anim = gameObject.GetComponent<Animator>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();
		particles = GameObject.Find("HitParticles").GetComponent<ParticleSystem>();
		particleEm = particles.shape;

		// hollow knight stuff
		player = HeroController.instance.gameObject;
		hc = HeroController.instance;

		// properties
		rand = new System.Random();
		head = GameObject.Find("ShadeLord/BeamOrigin/Head");
		title = GameObject.Find("Start");

		attacks = gameObject.AddComponent<Attacks>();
		attacks.target = player;
		attacks.Hide();
		
		atts = new List<Action>()
		{
			attacks.TendrilBurst,
			attacks.Dash,
			attacks.AimBeam,
			attacks.CrossSlash,
			attacks.Spikes
		};

		helper = gameObject.AddComponent<SLHelper>();
    } 
	void Start()
	{
		hitEffect  = GameObject.Find("VoidParticle");
		GameObject.Find("Start/Wall").transform.localPosition = new Vector3(0, -36.4f, 0);
		health = gameObject.AddComponent<HealthManager>();
		extDmg = gameObject.AddComponent<ExtraDamageable>();
		GameObject.Find("Halo").AddComponent<Spin>();
		AssignValues();
		attacks.Phase(phase);

		Spawn();
		//FastSpawn();
	}
	private void AssignValues()
	{
		// health
		int maxHp = 0;
		foreach (int phaseHp in hpMarkers)
		{
			maxHp += phaseHp;
		}
		health.hp = maxHp;

		hpMarkers[0] = maxHp - hpMarkers[0];
		hpMarkers[1] = hpMarkers[0] - hpMarkers[1];
		hpMarkers[2] = hpMarkers[1] - hpMarkers[2];
		hpMarkers[3] = hpMarkers[2] - hpMarkers[3];
		hpMarkers[4] = 0;

		phase = 0;

		gameObject.layer = 11;

		// copy collector fields
		GameObject refObj = ShadeLord.Setup.ShadeLord.GameObjects["Collector"];

		// health manager
		HealthManager refHP = refObj.GetComponent<HealthManager>();

		foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name.Contains("Prefab")))
		{
			fi.SetValue(health, fi.GetValue(refHP));
		}

		// extra damageable
		ExtraDamageable extraDamageable = refObj.GetComponent<ExtraDamageable>();
		foreach (FieldInfo fi in typeof(ExtraDamageable).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
		{
			fi.SetValue(extDmg, fi.GetValue(extraDamageable));
		}

		// set up attacks
		DamageHero dh;
		foreach (Transform t in gameObject.transform)
		{
			if (t.name == "ShadeLord") continue;
			if (t.GetComponent<SpriteRenderer>() != null)
			{
				t.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
			}
			dh = t.gameObject.AddComponent<DamageHero>();
			dh.damageDealt = 2;

			switch (t.name)
			{
				case "BeamOrigin":
					foreach (Transform child in t.transform)
					{
						if (child.name == "Offset")
						{
							foreach (Transform beam in child.transform)
							{
								dh = beam.gameObject.AddComponent<DamageHero>();
								dh.damageDealt = 2;
							}
						}
					}
					break;
				case "CrossSlash":
					foreach (Transform slash in t.transform)
					{
						dh = slash.gameObject.AddComponent<DamageHero>();
						dh.damageDealt = 2;
					}
					break;
			}
		}
	}
	 
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			Modding.Logger.Log("Shade Lord Position: " + transform.position);
			Modding.Logger.Log("Hero Position: " + player.transform.position);
			Modding.Logger.Log("Hero State: " + HeroController.instance.hero_state);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad0))
			actionState = -1;
		else if (Input.GetKeyDown(KeyCode.Keypad1))
			actionState = 0;
		else if (Input.GetKeyDown(KeyCode.Keypad2))
			actionState = 1;
		else if (Input.GetKeyDown(KeyCode.Keypad3))
			actionState = 2;
		else if (Input.GetKeyDown(KeyCode.Keypad4))
			actionState = 3;
		else if (Input.GetKeyDown(KeyCode.Keypad5))
			actionState = 4;
		else if (Input.GetKeyDown(KeyCode.Keypad6))
			actionState = 5;
		else if (Input.GetKeyDown(KeyCode.Keypad7))
			actionState = 6;
		else if (Input.GetKeyDown(KeyCode.Keypad8))
			actionState = 7;
		else if (Input.GetKeyDown(KeyCode.Keypad8))
			actionState = 8;

    }
	// damage stuff
	private void OnDeath()
	{
		gameObject.GetComponent<Attacks>().Stop();
		StopAllCoroutines();
        StartCoroutine(Death());

        IEnumerator Death()
        {
            attacks.playSound("ScreamLong");

            // die
            //anim.Play("Death");
            boxCol.enabled = false;
            GameObject particleObj = GameObject.Find("SpewParticles");
            particleObj.transform.position = transform.position + new Vector3(0, -2, 0);
            ParticleSystem particles = particleObj.GetComponent<ParticleSystem>();
            ParticleSystem.EmissionModule emission = particles.emission;
            particles.Play();
            anim.Play("Roar");
            attacks.playSound("ScreamLong");
            yield return new WaitForSeconds(3f);

            GameObject.Find("Gradient").transform.position = transform.position + new Vector3(0, -2, 0);
            emission.rateOverTime = 0f;
            yield return new WaitForSeconds(.5f);
            Color c = new Color(0, 0, 0, 1 / 30f);
            SpriteRenderer sprite = GameObject.Find("Gradient").GetComponent<SpriteRenderer>();
            sprite.color = Color.black;
            while (sprite.color.a > 0)
            {
                sprite.color -= c;
                yield return new WaitForSeconds(1 / 30f);
            }
            yield return new WaitForSeconds(3f);
            /*
			StatueCreator.WonFight = true;
			var bsc = SceneLoader.SceneController.GetComponent<BossSceneController>();
			GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
			PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
			transitionsFSM.SetState("Out Statue");
			bsc.DoDreamReturn();//*/

            ShadeLord.Setup.ShadeLord.DreamDelayed();
            Destroy(gameObject);
        }
    }
	// on take damage moved to ShadeLord.cs
	private void SpawnHitEffect(float dir)
	{
		// spawn void particles (16 frames)
		StartCoroutine(flicker());
		for (float k = 0; k < 15; k++)
		{
			GameObject particle = Instantiate(hitEffect);
			particle.transform.SetPosition2D(player.transform.position);
			float s = UnityEngine.Random.Range(.1f, .2f);
			particle.transform.SetScaleX(s);
			particle.transform.SetScaleY(s);
			float range = 5f, spread = 5f,
				f1 = 20f + UnityEngine.Random.Range(-1 * range, range), f2 = UnityEngine.Random.Range(-1 * spread, spread);
			switch (dir)
			{
				case 0f:
					particle.GetComponent<Rigidbody2D>().velocity = new Vector2(f1, f2);
					break;
				case 90f:
					particle.GetComponent<Rigidbody2D>().velocity = new Vector2(f2, f1);
					break;
				case 180f:
					particle.GetComponent<Rigidbody2D>().velocity = new Vector2(-1* f1, f2);
					break;
				case 270f:
					particle.GetComponent<Rigidbody2D>().velocity = new Vector2(f2, -1*f1);
					break;
			}
			StartCoroutine(die(particle));
		}
		IEnumerator flicker()
		{
			gameObject.GetComponent<SpriteRenderer>().color = Color.black;
			yield return new WaitForSeconds(.2f);
			for (int k = 0; k < 10; k++)
			{
				float c = .1f * k;
				gameObject.GetComponent<SpriteRenderer>().color = new Color(c,c,c);
				yield return new WaitForSeconds(1 / 60f);
			}
			gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		}
	}

	// Phase changes
	public void nextPhase()
	{
		phase++;
		Modding.Logger.Log(phase);
		switch (phase)
		{
			case 1:
				StopCoroutine(co);
				co = StartCoroutine(phase2());
				break;
			case 2: // spam phase
				atts = new List<Action>() { attacks.Spikes };
				break;
			case 3: // platform phase
				ToPlatform();
				break;
			case 4: // descend phase
				ToEnd();
				break;
		}
		if (health.hp < hpMarkers[phase])
		{
			nextPhase();
		}
		else
		{
			attacks.Phase(phase);
			Modding.Logger.Log("Shade Lord Phase: " + phase);
		}

		IEnumerator phase2()
		{
			if (health.hp < hpMarkers[2])
			{
				Modding.Logger.Log("cancel void circle transition");
				yield break;
			}
			Modding.Logger.Log("void circle next");
			atts.Add(attacks.VoidCircles);

			yield return new WaitWhile(() => attacks.isAttacking());
			if (phase == 1)
			{
				yield return new WaitForSeconds(1f);
				attacks.VoidCircles();
				yield return new WaitWhile(() => attacks.isAttacking());
			}
			co = StartCoroutine(AttackChoice());
		}
	}

	private void FastSpawn()
	{
		GameObject[] allObjects = FindObjectsOfType<GameObject>();
		IEnumerator Spawn()
		{
			ParticleSystem.EmissionModule emission = GameObject.Find("BackgroundParticles").GetComponent<ParticleSystem>().emission;
			transform.SetPositionY(-2f);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["Silence"]);
			GameObject.Find("Start").transform.SetPosition3D(0, 3.5f, 38f);
			GameObject wall = GameObject.Find("Start/Wall");
			SpriteRenderer wallSprite = GameObject.Find("Start/Wall/Black").GetComponent<SpriteRenderer>();
			SpriteRenderer titleSprite = GameObject.Find("Start/Title").GetComponent<SpriteRenderer>();
			Color c = new Color(0, 0, 0, .5f);

			emission.rateOverTime = 10f;
			GameObject.Find("BackgroundParticles").GetComponent<ParticleSystem>().Play();

			// set stuff before reveal
			GameObject.Find("ShadeLord/Tendrils").GetComponent<PolygonCollider2D>().enabled = false;

			// reveal

			GameObject.Find("ShadeLord/Tendrils").SetActive(false);
			transform.SetPositionY(-2f);

			// text appear, then leave
			emission.rateOverTime = 10f;
			GameObject.Find("ShadeLordMusic").GetComponent<AudioSource>().Play();

            // hud appear

            // GO
            GameObject.Find("Terrain/CameraLock").SetActive(false);
            title.SetActive(false);
			yield return new WaitForSeconds(1f);
			co = StartCoroutine(AttackChoice());
		}

		List<GameObject> startTendrils = new List<GameObject>();
		List<GameObject> voidTendrils = new List<GameObject>();
		bool foundAudioGroup = false;
		foreach (GameObject obj in allObjects)
		{
			if (obj.name == "white_palace_particles" || obj.name == "default_particles")
				obj.SetActive(false);
			else if (obj.name.Contains("ShortTendril"))
				voidTendrils.Add(obj);
			else if (obj.name.Contains("StartTendril"))
				startTendrils.Add(obj);
            else if (!foundAudioGroup && obj.GetComponent<AudioSource>() != null)
            {
                if (obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.name == "Music")
                {
					Modding.Logger.Log(obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.name);
                    AudioMixerGroup group = obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.FindMatchingGroups(string.Empty)[1];
                    GameObject.Find("ShadeLordMusic").GetComponent<AudioSource>().outputAudioMixerGroup = group;
                    GameObject.Find("VoidAmbience").GetComponent<AudioSource>().outputAudioMixerGroup = group;
					foundAudioGroup = true;
                }
            }//*/
        }
		helper.randomizeAnimStart(voidTendrils, "ShortAnim", 12);
		helper.randomizeAnimStart(startTendrils, "TendrilWiggle", 8);
		StartCoroutine(Spawn());
	}
	private void Spawn()
	{
		GameObject[] allObjects = FindObjectsOfType<GameObject>();
		GameObject hud = GameObject.Find("Hud Canvas");
		IEnumerator Spawn()
		{
			GameObject godseeker = GameObject.Find("GodseekerHolder/Godseeker");
			godseeker.SetActive(false);
			transCamLock.SetActive(false);
			area1CamLock.SetActive(false);

            ParticleSystem.EmissionModule emission = GameObject.Find("BackgroundParticles").GetComponent<ParticleSystem>().emission;
			transform.SetPositionY(-2f);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["Silence"]);
			GameObject.Find("Start").transform.SetPosition3D(0, 3.5f, 38f);
			hud.transform.SetPositionX(hud.transform.GetPositionX()+100);
			GameObject wall = GameObject.Find("Start/Wall");
			SpriteRenderer wallSprite = GameObject.Find("Start/Wall/Black").GetComponent<SpriteRenderer>();
			SpriteRenderer titleSprite = GameObject.Find("Start/Title").GetComponent<SpriteRenderer>();
			Color c = new Color(0,0,0,.5f);

			emission.rateOverTime = 10f;
			GameObject.Find("BackgroundParticles").GetComponent<ParticleSystem>().Play();

			// START ANIMATION
			SpriteRenderer blackout = GameObject.Find("Terrain/BlackSquare").GetComponent<SpriteRenderer>();
			yield return new WaitForSeconds(3f);
            emission.rateOverTime = 100f;
			transform.SetPosition3D(23.3f, 80f, 60f);
			transform.SetScaleMatching(1f);
			GetComponent<SpriteRenderer>().color = Color.black;
			attacks.arrive(80f);
            GameObject tendrils = GameObject.Find("ShadeLord/Tendrils");
			tendrils.SetActive(false);

            c = new Color(0, 0, 0, 1 / 360f);
            while (blackout.color.a < .35)
            {
                blackout.color += c;
                yield return new WaitForSeconds(1 / 30f);
            }
			emission.rateOverTime = 200f;
            while (blackout.color.a < .6)
            {
                blackout.color += c;
                yield return new WaitForSeconds(1 / 30f);
            }

            // APPEAR CLOSER / SCREAM
            // lock movemnet
            if (player.transform.GetPositionX() > transform.GetPositionX())
				HeroController.instance.FaceLeft();
			else
				HeroController.instance.FaceRight();
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR ENTER", false);

			// black screen
			wall.transform.localPosition = new Vector3(0,0f,0);

			// set stuff before reveal
			transform.SetPosition3D(23.5f, 75.23f, .1f);
			transform.SetScaleMatching(1f);
            GetComponent<SpriteRenderer>().color = Color.white;
            anim.Play("Roar");
			attacks.playSound("ScreamLong");
			tendrils.SetActive(true);
            tendrils.GetComponent<PolygonCollider2D>().enabled = false;
			blackout.color = new Color(0,0,0,0);

            // reveal
            yield return new WaitForSeconds(1/6f);
			c = new Color(0, 0, 0, 1 / 7f);
			while (wallSprite.color.a > 0)
			{
				wallSprite.color -= c;
				yield return new WaitForSeconds(1 / 30f);
			}
			yield return new WaitForSeconds(3f);

			// TITLE CARD
			// wall rise
			wall.transform.localPosition = new Vector3(0, -33.1f, 0);
			wallSprite.color = new Color(0,0,0,1);
			while (wall.transform.localPosition.y < 0f)
			{
				wall.transform.localPosition = new Vector3(0, wall.transform.localPosition.y + 7f, 0);
				yield return new WaitForSeconds(1/30f);
			}
			wall.transform.localPosition = new Vector3(0, 0f, 0);

            // hide lord
            tendrils.SetActive(false);
			transform.SetPositionY(-2f);

			// text appear, then leave
			emission.rateOverTime = 0f;
			godseeker.SetActive(true);
			GameObject.Find("GodseekerHolder/GodseekerSpawn").SetActive(false);
            GameObject.Find("ShadeLordMusic").GetComponent<AudioSource>().Play();
            while (titleSprite.color.a<1)
			{
				titleSprite.color += c;
				yield return new WaitForSeconds(1/30f);
			}

			yield return new WaitForSeconds(3f);

			c = new Color(0, 0, 0, 1/13f);
			while (titleSprite.color.a > 0)
			{
				titleSprite.color -= c;
				yield return new WaitForSeconds(1 / 30f);
			}
			// black box disappear
			c = new Color(0, 0, 0, 1 / 7f);
			while (wallSprite.color.a > 0)
			{
				wallSprite.color -= c;
				yield return new WaitForSeconds(1 / 30f);
			}

			// hud appear
			hud.transform.SetScaleX(.9f);
			hud.transform.SetScaleY(.9f);
			hud.transform.SetPositionX(hud.transform.GetPositionX() - 100);

			for(int i =1; i<= 5;i++)
			{
				float scale = .9f+(.1f/6)*i;
				hud.transform.SetScaleX(scale);
				hud.transform.SetScaleY(scale);
				yield return new WaitForSeconds(1/60f);
			}
			hud.transform.SetScaleX(1);
			hud.transform.SetScaleY(1);

			// GO
			GameObject.Find("Terrain/CameraLock").SetActive(false);
			area1CamLock.SetActive(true);
			title.SetActive(false);

			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR EXIT", false);
			yield return new WaitForSeconds(1f);
			co = StartCoroutine(AttackChoice());
		}
		
		List<GameObject> startTendrils = new List<GameObject>();
		List<GameObject> voidTendrils = new List<GameObject>();
		bool foundAudioGroup = false;
		foreach (GameObject obj in allObjects)
		{
			if (obj.name == "white_palace_particles" || obj.name == "default_particles")
				obj.SetActive(false);
			else if (obj.name == "Hud Canvas")
				hud = obj;
			else if (obj.name.Contains("ShortTendril"))
				voidTendrils.Add(obj);
			else if (obj.name.Contains("StartTendril"))
				startTendrils.Add(obj);
            else if (!foundAudioGroup && obj.GetComponent<AudioSource>() != null)
            {
                if (obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.name == "Music")
                {
                    Modding.Logger.Log(obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.name);
                    AudioMixerGroup group = obj.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.FindMatchingGroups(string.Empty)[1];
                    GameObject.Find("ShadeLordMusic").GetComponent<AudioSource>().outputAudioMixerGroup = group;
                    GameObject.Find("VoidAmbience").GetComponent<AudioSource>().outputAudioMixerGroup = group;
                    foundAudioGroup = true;
                }
            }
        }
		helper.randomizeAnimStart(voidTendrils, "ShortAnim", 12);
		helper.randomizeAnimStart(startTendrils, "TendrilWiggle", 8);
		StartCoroutine(Spawn());
	}
	private void ToPlatform()
	{
		IEnumerator ToPlatform()
		{
			// stop all
			StopCoroutine(co);
			attacks.Stop();
		
			// lord animation
			attacks.playSound("ScreamLong");
			attacks.leave();
            Vector3 pos = transform.position + new Vector3(0, -2, 0);
            GameObject particles = GameObject.Find("VanishParticles");
            particles.transform.position = pos;
            particles.GetComponent<ParticleSystem>().Play();

			StartCoroutine(darkBurst());
            // wait a bit
            yield return new WaitForSeconds(2f);

			// terrain breaks
            breakTerrain(GameObject.Find("Terrain/Area1"), GameObject.Find("Terrain/RocksFloor"));
			// Wait a bit then start attacking again
			yield return new WaitForSeconds(1f);

            helper.abyssArrive();
            yield return new WaitForSeconds(4f);
			
			atts = new List<Action>()
			{
				attacks.VoidCircles,
				attacks.TendrilBurst,
				attacks.Dash,
				attacks.AimBeam,
				attacks.CrossSlash
			};

			co = StartCoroutine(AttackChoice());
		}
		StartCoroutine(ToPlatform());
	}
	private void ToEnd()
	{
		IEnumerator ToEnd()
		{
            atts = new List<Action>() {};
            GameObject.Find("Terrain/Area2/Respawn").SetActive(false);

            // wait till current attack done
            yield return new WaitWhile(attacks.isAttacking);
            ParticleSystem.EmissionModule emission = GameObject.Find("BackgroundParticles").GetComponent<ParticleSystem>().emission;
			emission.rateOverTime = 50f;
            GameObject cameraLock = GameObject.Find("Terrain/Area3/CameraLock");
            cameraLock.SetActive(false);
			GameObject breakSound = GameObject.Find("Terrain/BreakAudio");
			breakSound.transform.SetPosition2D(18.18f, 73.48f);
			breakSound.GetComponent<AudioSource>().minDistance = 8f;
			breakTerrain(GameObject.Find("Terrain/Area2/RightWall"), GameObject.Find("Terrain/RocksRight"));
            StopCoroutine(co);

            GetComponent<BoxCollider2D>().enabled = false;
            GameObject.Find("Terrain/ToArea3").transform.SetPositionY(56.04f);

            yield return new WaitForSeconds(1f);

            // terrain break 1
            area1CamLock.SetActive(false);
			transCamLock.SetActive(true);
            helper.abyssToEnd();

            // terrain break 2
            yield return new WaitForSeconds(3f);
            //attacks.VoidCircles();

            atts = new List<Action>() { attacks.AimBeam };
            StartCoroutine(FadeMusic());
            co = StartCoroutine(AttackChoice());
            float xTrigger = GameObject.Find("Terrain/Area3").transform.GetPositionX() - 5f;
			// Wait till reach end section
			yield return new WaitWhile(() => player.transform.GetPositionX()<xTrigger);
            cameraLock.SetActive(true);
            transCamLock.SetActive(false);

            helper.moveX(GameObject.Find("AbyssWallLeft").transform, cameraLock.transform.GetPositionX() - 15f, 1);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

            atts = new List<Action>() { attacks.VoidCircles };
            attacks.Stop();
		}
		IEnumerator FadeMusic()
		{
			AudioSource music = GameObject.Find("ShadeLordMusic").GetComponent<AudioSource>();
			AudioSource ambience = GameObject.Find("VoidAmbience").GetComponent<AudioSource>();
			ambience.volume = 0;
			ambience.Play();
            while (music.volume >0)
			{
				music.volume -= 1f/100f;
				ambience.volume += 1f / 100f;
				yield return new WaitForSeconds(1/10f);
			}
		}

        StartCoroutine(ToEnd());
	}
	private void Vanish()
	{
		GetComponent<BoxCollider2D>().enabled = false;

		GameObject vanish = GameObject.Find("VanishParticles");
		vanish.GetComponent<ParticleSystem>().Play();
		vanish.transform.position = transform.position;
		vanish.transform.SetPositionY(particles.transform.position.y + -5.080002f);

	}

	private void breakTerrain(GameObject go, GameObject rocks)
	{
		float c = 1/120f;
		Color scale = new Color(c, c, c, 0);
		StartCoroutine(breakTerrain(go));

		IEnumerator breakTerrain(GameObject go)
		{
			// fade to black
			foreach (SpriteRenderer s in go.GetComponentsInChildren<SpriteRenderer>())
			{
				StartCoroutine(fade(s));
			}

			yield return new WaitForSeconds(3f);
			go.SetActive(false);
			AudioSource audio = GameObject.Find("Terrain/BreakAudio").GetComponent<AudioSource>();
            audio.outputAudioMixerGroup = player.GetComponent<AudioSource>().outputAudioMixerGroup;
            audio.Play();

            // rock particles
			helper.launchRocks(rocks);
		}
		IEnumerator fade(SpriteRenderer sprite)
		{
			// fade to black
			while (sprite.color.r > 0)
			{
				sprite.color -= scale;

				yield return new WaitForSeconds(1/60f);
			}
		}
	}
	private void fadeFromBlack(SpriteRenderer sr)
	{
		float c = 1 / 40f;
		Color scale = new Color(c, c, c);
		StartCoroutine(fade(sr));
		IEnumerator fade(SpriteRenderer sprite)
		{
			// fade to black
			while (sprite.color.r < 1)
			{
				sprite.color += scale;

				yield return new WaitForSeconds(.05f);
			}
		}
	}
	private void fadeToBlack(SpriteRenderer sr, float c, float limit)
	{
		Color scale = new Color(c, c, c,0);
		StartCoroutine(fade(sr));
		IEnumerator fade(SpriteRenderer sprite)
		{
			// fade to black
			while (sprite.color.r >limit/255)
			{
				sprite.color -= scale;

				yield return new WaitForSeconds(.05f);
			}
		}
	}

	// actions
	private IEnumerator AttackChoice()
	{
        // Pick attack
        Action curr;
		int i;
		if (actionState == -1)
			i = rand.Next(0, atts.Count);
		else
			i = actionState % atts.Count;

        curr = atts[i];
		curr.Invoke();
		// Wait till last attack is done
		yield return new WaitWhile(() => attacks.isAttacking());
		// delay between attacks
		if (phase != 4)
			yield return new WaitForSeconds(.3f);
		// Repeat
		co = StartCoroutine(AttackChoice());
	}

	// helper
	IEnumerator die(GameObject particle)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(10 / 60f, 15 / 60f));
		helper.fadeTo(particle.GetComponent<SpriteRenderer>(), new Color(1, 1, 1, 0), 2f);
		yield return new WaitForSeconds(2f);
		Destroy(particle);
	}

	public IEnumerator darkBurst()
	{
        GameObject.Find("Gradient").transform.position = transform.position + new Vector3(0,-5,0);
        Color c = new Color(0, 0, 0, 1 / 30f);
        SpriteRenderer sprite = GameObject.Find("Gradient").GetComponent<SpriteRenderer>();
        sprite.color = Color.black;
		yield return new WaitForSeconds(.5f);
        while (sprite.color.a > 0)
        {
            sprite.color -= c;
            yield return new WaitForSeconds(1 / 30f);
        }
    }
}