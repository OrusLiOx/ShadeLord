/*/
 
Controls Shade Lord behavior (phases, attacks, spawning, dying)

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UObject = UnityEngine.Object;
using ShadeLord.Setup;

class ShadeLordCtrl : MonoBehaviour
{
	// generic unity stuff
	private Animator anim;
	private BoxCollider2D boxCol;
	private Coroutine co;

	// hollow knight stuff
	private GameObject player;
	private HeroController hc;
	private HealthManager health;
	private GameObject hitEffect;
	private ExtraDamageable extDmg;

	// properties
	private GameObject head, title;
	private List<Action> atts;
	//private int[] hpMarkers = { 50,50,50,50,300};
	private int[] hpMarkers = { 400, 450, 300, 750, 2200 };
	private System.Random rand;
	
	private Attacks attacks;
	private SLHelper helper;
	private VoidParticleSpawner vpSpawner;

	// trackers
	private Queue<GameObject> spawned;
	private Queue<GameObject> tendrils;

	private bool triggeredRocks;
	private int phase;

	// setting up stuff
	void Awake()
	{
		// generic unity stuff
		anim = gameObject.GetComponent<Animator>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();//*/

		// hollow knight stuff
		player = HeroController.instance.gameObject;//*/
		hc = HeroController.instance;

		// properties
		rand = new System.Random();
		head = GameObject.Find("ShadeLord/BeamOrigin/Head");
		title = GameObject.Find("Start");

		attacks = gameObject.AddComponent<Attacks>();
		attacks.target = player;
		attacks.Hide();

		/*
		atts = new List<Action>()
		{
			attacks.TendrilBurst,
			attacks.Dash,
			attacks.SweepBeam,
			attacks.CrossSlash,
			attacks.FaceSpikes,
			attacks.Spikes
		};//*/

		//*
		atts = new List<Action>()
		{
			attacks.AimBeam
		};//*/

		helper = gameObject.AddComponent<SLHelper>();

		vpSpawner = gameObject.AddComponent<VoidParticleSpawner>();

		// trackers
		tendrils = new Queue<GameObject>();
	}
	void Start()
	{
		hitEffect  = GameObject.Find("VoidParticle");
		GameObject.Find("Start/Wall").transform.localPosition = new Vector3(0, -36.4f, 0);
		health = gameObject.AddComponent<HealthManager>();
		extDmg = gameObject.AddComponent<ExtraDamageable>();
		GameObject.Find("Halo").AddComponent<Spin>();
		AssignValues();
		health.OnDeath += OnDeath;
		On.HealthManager.TakeDamage += OnTakeDamage;
		attacks.Phase(phase);

		//Spawn();
		FastSpawn();
	}
	private void AssignValues()
	{
		// health
		health.hp = hpMarkers[4];

		hpMarkers[0] = hpMarkers[4] - hpMarkers[0];
		hpMarkers[1] = hpMarkers[0] - hpMarkers[1];
		hpMarkers[2] = hpMarkers[1] - hpMarkers[2];
		hpMarkers[3] = hpMarkers[2] - hpMarkers[3];
		hpMarkers[4] = 0;//*/

		phase = 0;
		triggeredRocks = false;

		gameObject.layer = 11;

		// copy collector fields
		GameObject refObj = ShadeLord.Setup.ShadeLord.GameObjects["Collector"];

		// health manager
		HealthManager refHP = refObj.GetComponent<HealthManager>();

		foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name.Contains("Prefab")))
		{
			fi.SetValue(health, fi.GetValue(refHP));
		}//*/

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
				case "Spike":
					t.gameObject.AddComponent<Spike>();
					foreach(Transform spike in t.transform)
						spike.gameObject.AddComponent<Spike>();
					break;
				case "BurstSpike":
					t.gameObject.AddComponent<FaceSpike>();
					break;
				case "VoidBurst":
					t.gameObject.AddComponent<VoidBurst>();
					break;
				case "BeamOrigin":
					foreach (Transform child in t.transform)
					{
						if (child.name == "Offset")
						{
							foreach (Transform beam in child.transform)
							{
								beam.gameObject.AddComponent<Beam>();
								dh = beam.gameObject.AddComponent<DamageHero>();
								dh.damageDealt = 2;
							}
						}
					}
					break;
				case "CrossSlash":
					foreach (Transform slash in t.transform)
					{
						dh =slash.gameObject.AddComponent<DamageHero>();
						dh.damageDealt = 2;
					}
					break;
				case "VoidCircle":
					t.gameObject.AddComponent<VoidCircle>();
					t.gameObject.AddComponent<Spin>();
					break;
			}
		}
	}

	// damage stuff
	private void OnDeath()
	{
		gameObject.GetComponent<Attacks>().Stop();
		StopAllCoroutines();
		Death();
	}
	private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
	{
		Modding.Logger.Log(self.hp + " " + hitinstance.DamageDealt+ " " + hitinstance.Direction);
		// deal hit then check phase
		orig(self, hitinstance);
		SpawnHitEffect(hitinstance.Direction);
		if (health.hp < hpMarkers[phase])
		{
			nextPhase();
		}//*/
	}
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
			for (int k = 0; k < 10; k++)
			{
				float c = .1f * k;
				gameObject.GetComponent<SpriteRenderer>().color = new Color(c,c,c);
				yield return new WaitForSeconds(1 / 60f);
			}
			gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		}
	}//*/

	// Phase changes
	private void nextPhase()
	{
		phase++;
		switch (phase)
		{
			case 1:
				atts.Add(attacks.VoidCircles);
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
			atts.Add(attacks.VoidCircles);

			yield return new WaitWhile(() => attacks.isAttacking());//*/
			yield return new WaitForSeconds(1f);
			attacks.VoidCircles();
			yield return new WaitWhile(() => attacks.isAttacking());
			co = StartCoroutine(AttackChoice());
		}
	}

	private void FastSpawn()
	{
		GameObject[] allObjects = FindObjectsOfType<GameObject>();
		GameObject hud = GameObject.Find("Hud Canvas");
		IEnumerator Spawn()
		{
			transform.SetPositionY(-2f);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["Silence"]);
			vpSpawner.set(55, 145, 64);
			vpSpawner.setActive(true);
			GameObject.Find("Start").transform.SetPosition3D(0, 3.5f, 38f);
			hud.transform.SetPositionX(hud.transform.GetPositionX() + 100);
			GameObject wall = GameObject.Find("Start/Wall");
			SpriteRenderer wallSprite = GameObject.Find("Start/Wall/Black").GetComponent<SpriteRenderer>();
			SpriteRenderer titleSprite = GameObject.Find("Start/Title").GetComponent<SpriteRenderer>();
			Color c = new Color(0, 0, 0, .5f);
			GameObject[] camLocks = {
				GameObject.Find("Terrain/Area1/CameraLock"),
				GameObject.Find("Terrain/Area2/CameraLock")
			};
			//foreach (GameObject go in camLocks)
			//	go.SetActive(false);

			/*/ START ANIMATION
			//*/

			yield return new WaitForSeconds(1f);

			// APPEAR CLOSER / SCREAM
			// lock movemnet
			if (player.transform.GetPositionX() > transform.GetPositionX())
				HeroController.instance.FaceLeft();
			else
				HeroController.instance.FaceRight();
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR ENTER", false);

			// black screen
			wall.transform.localPosition = new Vector3(0, 0f, 0);

			// set stuff before reveal
			vpSpawner.setDensity(30);
			transform.SetPosition2D(100f, 75.23f);
			anim.Play("NeutralSquint");
			attacks.playSound("ScreamLong");
			GameObject.Find("ShadeLord/Tendrils").GetComponent<PolygonCollider2D>().enabled = false;

			// reveal
			yield return new WaitForSeconds(1 / 6f);
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
			wallSprite.color = new Color(0, 0, 0, 1);
			while (wall.transform.localPosition.y < 0f)
			{
				wall.transform.localPosition = new Vector3(0, wall.transform.localPosition.y + 7f, 0);
				yield return new WaitForSeconds(1 / 30f);
			}
			wall.transform.localPosition = new Vector3(0, 0f, 0);

			// hide lord
			GameObject.Find("ShadeLord/Tendrils").SetActive(false);
			transform.SetPositionY(-2f);

			// text appear, then leave
			vpSpawner.setDensity(1);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["ShadeLord_Theme"]);
			while (titleSprite.color.a < 1)
			{
				titleSprite.color += c;
				yield return new WaitForSeconds(1 / 30f);
			}


			c = new Color(0, 0, 0, 1 / 13f);
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

			for (int i = 1; i <= 5; i++)
			{
				float scale = .9f + (.1f / 6) * i;
				hud.transform.SetScaleX(scale);
				hud.transform.SetScaleY(scale);
				yield return new WaitForSeconds(1 / 60f);
			}
			hud.transform.SetScaleX(1);
			hud.transform.SetScaleY(1);

			// GO
			title.SetActive(false);
			GameObject.Find("Terrain/CameraLock").SetActive(false);
			//foreach (GameObject go in camLocks)
			//	go.SetActive(true);
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR EXIT", false);
			yield return new WaitForSeconds(1f);
			co = StartCoroutine(AttackChoice());
		}

		List<GameObject> startTendrils = new List<GameObject>();
		List<GameObject> voidTendrils = new List<GameObject>();
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
			GameObject obj1 = GameObject.Find("Terrain/Area2/RespawnL");
			GameObject obj2 = GameObject.Find("Terrain/Area2/RespawnR");
			obj1.SetActive(false);
			obj2.SetActive(false);
			transform.SetPositionY(-2f);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["Silence"]);
			vpSpawner.set(55, 145, 64);
			vpSpawner.setActive(true);
			GameObject.Find("Start").transform.SetPosition3D(0, 3.5f, 38f);
			hud.transform.SetPositionX(hud.transform.GetPositionX()+100);
			GameObject wall = GameObject.Find("Start/Wall");
			SpriteRenderer wallSprite = GameObject.Find("Start/Wall/Black").GetComponent<SpriteRenderer>();
			SpriteRenderer titleSprite = GameObject.Find("Start/Title").GetComponent<SpriteRenderer>();
			Color c = new Color(0,0,0,.5f);
			GameObject[] camLocks = {
				GameObject.Find("Terrain/Area1/CameraLock"),
				GameObject.Find("Terrain/Area2/CameraLock")
			};
			foreach (GameObject go in camLocks)
				go.SetActive(false);

			GameObject obj3 = GameObject.Find("Terrain/CameraLock");
			obj3.transform.SetPositionZ(0);
			obj3.SetActive(false);
			yield return new WaitForSeconds(.1f);
			obj3.SetActive(true);

			// START ANIMATION
			yield return new WaitForSeconds(3f);
			SpriteRenderer[] bg = {
				GameObject.Find("Terrain/Area1/Floor/Sprite").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Area2/Mid/Sprite").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Area2/Right/Sprite").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Area2/Left/Sprite").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Background/Start/Bg_Start_3").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Background/Start/Bg_Start_2").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Background/Start/Bg_Start_1").GetComponent<SpriteRenderer>(),
				GameObject.Find("Terrain/Background/Start/Bg_Start_0").GetComponent<SpriteRenderer>()
			};
			vpSpawner.setDensity(15);
			for (int i = 0; i< 4; i++)
				fadeToBlack(bg[i], 1 / 60f, 0);
			for (int i = 4; i < 8; i++)
				fadeToBlack(bg[i], 1 / (60f+i*5), (i-4)*25);

			yield return new WaitForSeconds(7f);

			// APPEAR CLOSER / SCREAM
			// lock movemnet
			if (player.transform.GetPositionX() > transform.GetPositionX())
				HeroController.instance.FaceLeft();
			else
				HeroController.instance.FaceRight();
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR ENTER", false);

			// black screen
			wall.transform.localPosition = new Vector3(0,0f,0);
			foreach (SpriteRenderer s in bg)
				s.color = new Color(.5f,.5f,.5f);

			// set stuff before reveal
			vpSpawner.setDensity(30);
			transform.SetPosition2D(100f, 75.23f);
			anim.Play("NeutralSquint");
			attacks.playSound("ScreamLong");
			GameObject.Find("ShadeLord/Tendrils").GetComponent<PolygonCollider2D>().enabled = false;

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
			foreach (SpriteRenderer s in bg)
				s.color = new Color(1f, 1f, 1f);
			GameObject.Find("ShadeLord/Tendrils").SetActive(false);
			transform.SetPositionY(-2f);

			// text appear, then leave
			vpSpawner.setDensity(1);
			ShadeLord.Setup.ShadeLord.PlayMusic(attacks.sounds["ShadeLord_Theme"]);
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
			obj1.SetActive(true);
			obj2.SetActive(true);
			title.SetActive(false);
			obj3.SetActive(false);
			foreach (GameObject go in camLocks)
				go.SetActive(true);
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR EXIT", false);
			yield return new WaitForSeconds(1f);
			co = StartCoroutine(AttackChoice());
		}
		
		List<GameObject> startTendrils = new List<GameObject>();
		List<GameObject> voidTendrils = new List<GameObject>();
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
		}
		helper.randomizeAnimStart(voidTendrils, "ShortAnim", 12);
		helper.randomizeAnimStart(startTendrils, "TendrilWiggle", 8);
		StartCoroutine(Spawn());
	}
	private void ToPlatform()
	{
		IEnumerator ToPlatform()
		{
			// stop all and disapear into void particles
			StopCoroutine(co);
			attacks.Stop();
			
			// explode into void particles
			attacks.playSound("Scream");

			helper.fadeTo(gameObject.GetComponent<SpriteRenderer>(), new Color(1, 1, 1, 0), .3f);
			helper.fadeTo(GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>(), new Color(1,1,1,0), .3f);

			GetComponent<BoxCollider2D>().enabled = false;

			// spawn a bunch of particles 
			for (float k = 0; k < 100; k++)
			{
				GameObject particle = Instantiate(hitEffect);
				// set position
				particle.transform.SetPositionX(transform.position.x + UnityEngine.Random.Range(-3f, 3f));
				particle.transform.SetPositionY(transform.position.y + UnityEngine.Random.Range(-2.37f,-11f));

				// set scale
				float s = UnityEngine.Random.Range(.1f, .2f);
				particle.transform.SetScaleX(s);
				particle.transform.SetScaleY(s);

				// launch 
				float range = 5f;
				particle.GetComponent<Rigidbody2D>().velocity = new Vector2(UnityEngine.Random.Range(-1 * range, range), UnityEngine.Random.Range(-1 * range, range));

				// destroy
				StartCoroutine(die(particle));
			}

			// wait a bit
			vpSpawner.setDensity(5f);
			yield return new WaitForSeconds(.5f);

			// void particles raise from floor and terrain breaks
			breakTerrain(GameObject.Find("Terrain/Area1"));
			
			// Wait a bit then start attacking again
			yield return new WaitForSeconds(2.5f);
			vpSpawner.setDensity(1f);
			yield return new WaitForSeconds(4f);
			
			transform.SetPositionY(0f);
			GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			
			atts = new List<Action>()
			{
				attacks.VoidCircles,
				attacks.TendrilBurst,
				attacks.Dash,
				attacks.SweepBeam,
				attacks.CrossSlash,
				attacks.FaceSpikes
			};

			co = StartCoroutine(AttackChoice());
		}
		StartCoroutine(ToPlatform());
	}
	private void ToEnd()
	{
		IEnumerator ToEnd()
		{
			atts = new List<Action>() { };
			GameObject cameraLock = GameObject.Find("Terrain/Area3/CameraLock");
			GameObject leftWall = GameObject.Find("Terrain/Area3/WallL");
			cameraLock.SetActive(false);
			leftWall.SetActive(false);

			// wait till current attack done
			yield return new WaitWhile(attacks.isAttacking);
			StopCoroutine(co);
			// explode into particles
			helper.fadeTo(gameObject.GetComponent<SpriteRenderer>(), new Color(1, 1, 1, 0), .3f);
			helper.fadeTo(GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>(), new Color(1, 1, 1, 0), .3f);

			GetComponent<BoxCollider2D>().enabled = false;

			for (float k = 0; k < 100; k++)
			{
				GameObject particle = Instantiate(hitEffect);
				// set position
				particle.transform.SetPositionX(transform.position.x + UnityEngine.Random.Range(-3f, 3f));
				particle.transform.SetPositionY(transform.position.y + UnityEngine.Random.Range(-2.37f, -11f));

				// set scale
				float s = UnityEngine.Random.Range(.1f, .2f);
				particle.transform.SetScaleX(s);
				particle.transform.SetScaleY(s);

				// launch 
				float range = 3f;
				particle.GetComponent<Rigidbody2D>().velocity = new Vector2(UnityEngine.Random.Range(-1 * range, range), UnityEngine.Random.Range(5f, 3f));

				// destroy
				StartCoroutine(die(particle));
			}
			// terrain break 1
			//GameObject.Find("Terrain/Area2/CameraLock").SetActive(false);
			GameObject[] go = { GameObject.Find("Terrain/Area2/Mid") };
			breakTerrain(GameObject.Find("Terrain/Area2/Mid"));
			GameObject hazard = GameObject.Find("Terrain/VoidHazardTemp");
			
			yield return new WaitForSeconds(5f);
			hazard.GetComponent<Rigidbody2D>().gravityScale = .2f;
			
			// start spamming
			attacks.VoidCircles();

			// terrain break 2
			yield return new WaitForSeconds(5f);
			hazard.SetActive(false);
			breakTerrain(GameObject.Find("Terrain/Area2/EdgeLeft"));
			breakTerrain(GameObject.Find("Terrain/Area2/EdgeRight"));
			breakTerrain(GameObject.Find("Terrain/Area2/Left (1)"));
			breakTerrain(GameObject.Find("Terrain/Area2/Right (1)"));

			atts = new List<Action>() { attacks.SweepBeam };

			// Wait till reach end section
			yield return new WaitWhile(()=> player.transform.GetPositionX()<210);

			GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

			cameraLock.SetActive(true);
			leftWall.SetActive(true);
			attacks.Stop();
			yield return new WaitForSeconds(3f);
			co = StartCoroutine(AttackChoice());

			GameObject[] GOs = { GameObject.Find("Terrain/Descend/Section2/Plat1"), GameObject.Find("Terrain/Descend/Section2/Plat2")};
			breakTerrain(GameObject.Find("Terrain/Descend/Section2"));
		}
		StartCoroutine(ToEnd());
	}
	private void Death()
	{
		IEnumerator Death()
		{
			attacks.playSound("ScreamLong");
			// remove spawned attacks
			foreach (GameObject obj in spawned)
			{
				Destroy(obj);
			}
			foreach (GameObject obj in tendrils)
			{
				Destroy(obj);
			}
			// die
			anim.Play("Death");
			boxCol.enabled = false;
			yield return null;
			yield return new WaitForSeconds(1.2f);

			StatueCreator.WonFight = true;
			var bsc = SceneLoader.SceneController.GetComponent<BossSceneController>();
			GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
			PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
			transitionsFSM.SetState("Out Statue");
			yield return new WaitForSeconds(1);
			bsc.DoDreamReturn();

			Destroy(gameObject);
		}

		StartCoroutine(Death());
	}

	private void breakTerrain(GameObject go)
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

			yield return new WaitForSeconds(5f);
			vpSpawner.setDensity(1f);
			go.SetActive(false);

			// rock particles
			if (!triggeredRocks)
			{
				triggeredRocks = true;
				helper.launchRocks();
			}//*/
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
		int i = rand.Next(0, atts.Count);
		curr = atts[i];
		curr.Invoke();
		// Wait till last attack is done
		yield return new WaitWhile(() => attacks.isAttacking());//*/

		// delay between attacks
		if(phase != 4)
			yield return new WaitForSeconds(1f);
		// Repeat

		co = StartCoroutine(AttackChoice());//*/
	}

	// helper
	IEnumerator die(GameObject particle)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(10 / 60f, 15 / 60f));
		helper.fadeTo(particle.GetComponent<SpriteRenderer>(), new Color(1, 1, 1, 0), 2f);
		yield return new WaitForSeconds(2f);
		Destroy(particle);
	}

}