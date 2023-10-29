using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UObject = UnityEngine.Object;

class ShadeCtrl : MonoBehaviour
{
	// generic unity stuff
	private Animator anim;
	private SpriteRenderer spriteRender;
	private Rigidbody2D rigBod;
	private BoxCollider2D boxCol;
	private Camera cam;
	private Coroutine co;

	// hollow knight stuff
	private GameObject player;
	private HealthManager health;
	private EnemyHitEffectsShade hitEffect;
	private ExtraDamageable extDmg;

	// properties
	private List<Action> atts;
	private int[] hpMarkers = { 400, 450, 300, 750, 2200 };
	private System.Random rand;
	
	private Attacks attacks;
	// trackers
	private Queue<GameObject> spawned;
	private Queue<GameObject> tendrils;

	private int phase;

	// SETTING STUFF UP
	void Awake()
	{
		// generic unity stuff
		anim = gameObject.GetComponent<Animator>();
		spriteRender = gameObject.GetComponent<SpriteRenderer>();
		rigBod = gameObject.GetComponent<Rigidbody2D>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();//*/

		// hollow knight stuff
		player = HeroController.instance.gameObject;
		health = gameObject.AddComponent<HealthManager>();
		hitEffect = gameObject.AddComponent<EnemyHitEffectsShade>();
		extDmg = gameObject.AddComponent<ExtraDamageable>();//*/

		gameObject.AddComponent<SpriteFlash>();

		// properties
		rand = new System.Random();

		attacks = gameObject.AddComponent<Attacks>();
		attacks.target = player;
		attacks.Hide();
		atts = new List<Action>()
		{
			attacks.Dash,
			attacks.SweepBeam,
			attacks.CrossSlash,
			attacks.FaceSpikes,
			attacks.Spikes,
			attacks.AimBeam
		};//*/

		// trackers
		tendrils = new Queue<GameObject>();
	}
	void Start()
	{
		AssignValues();
		health.OnDeath += OnDeath;
		On.HealthManager.TakeDamage += OnTakeDamage;
		attacks.Phase(phase);
		
		Spawn();
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

		gameObject.layer = 11;

		GameObject refObj = ShadeLord.ShadeLord.GameObjects["Collector"];
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
		foreach (Transform t in gameObject.transform)
		{
			if (t.name == "ShadeLord") continue;
			if (t.GetComponent<SpriteRenderer>() != null)
			{
				t.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
			}
			t.gameObject.AddComponent<DamageHero>();

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
				case "BeamOrigin":
					foreach (Transform child in t.transform)
					{
						Modding.Logger.Log(child.name);
						if (child.name == "Offset")
						{
							foreach (Transform beam in child.transform)
							{
								beam.gameObject.AddComponent<Beam>();
								beam.gameObject.AddComponent<DamageHero>();
							}
						}
					}
					break;
				case "CrossSlash":
					foreach (Transform slash in t.transform)
					{
						slash.gameObject.AddComponent<DamageHero>();
					}
					break;
			}
		}
	}

	private void OnDeath()
	{
		gameObject.GetComponent<Attacks>().
		StopAllCoroutines();
		Death();
	}
	private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
	{
		// deal hit then check phase
		orig(self, hitinstance);
		Modding.Logger.Log(health.hp);
		if (health.hp < hpMarkers[phase])
		{
			nextPhase();
		}//*/
	}

	// Phase changes
	private void nextPhase()
	{
		phase++;
		switch (phase)
		{
			case 1: // add lingering spikes
				//((Action)Tendrils).Invoke();
				break;
			case 2: // spam phase
				break;
			case 3: // platform phase
				attacks.Stop();
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
	}
	private void Spawn()
	{
		IEnumerator Spawn()
		{
			yield return new WaitForSeconds(5f);
			co = StartCoroutine(AttackChoice());
		}
		StartCoroutine(Spawn());
	}
	private void ToPlatform()
	{
		IEnumerator ToPlatform()
		{
			// stop all and disapear into void particles
			StopCoroutine(co);
			attacks.Stop();

			// wait a bit
			yield return new WaitForSeconds(.5f);

			// void particles raise from floor and terrain breaks
			GameObject[] GOs =
			{
				GameObject.Find("Terrain/Area1/Floor"),
				GameObject.Find("Terrain/Area1/LeftWall"),
				GameObject.Find("Terrain/Area1/RightWall")
			};
			breakTerrain(GOs);

			// Wait a bit then start attacking again
			yield return new WaitForSeconds(5f);
			atts.Remove(attacks.Spikes);

			co = StartCoroutine(AttackChoice());
		}
		StartCoroutine(ToPlatform());
	}
	private void ToEnd()
	{
		IEnumerator ToEnd()
		{
			atts = new List<Action>() { };

			GameObject[] go = { GameObject.Find("Terrain/Area2/Mid") };
			breakTerrain(go);
			GameObject.Find("Terrain/VoidHazardTemp").SetActive(false);

			atts = new List<Action>() { attacks.SweepBeam };

			// Wait till reach end section
			yield return new WaitWhile(()=> player.transform.GetPositionX()<208);
			GameObject.Find("Terrain/Area3/CameraLock").SetActive(true);

			GameObject[] GOs = { GameObject.Find("Terrain/Descend/Section2/Plat1"), GameObject.Find("Terrain/Descend/Section2/Plat2")};
			breakTerrain(GOs);
		}
		StartCoroutine(ToEnd());
	}

	private void breakTerrain(GameObject[] GOs)
	{
		foreach(GameObject go in GOs)
			StartCoroutine(breakTerrain(go));

		IEnumerator breakTerrain(GameObject go)
		{
			float c = .02f;
			Color scale = new Color(c, c, c, 0);
			SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();

			// fade to black
			while (sprite.color.r > 0)
			{
				sprite.color -= scale;

				yield return new WaitForSeconds(.02f);
			}

			yield return new WaitForSeconds(2f);
			go.SetActive(false);
		}
	}

	// ACTIONS
	private IEnumerator AttackChoice()
	{
		// Pick attack
		Action curr;
		curr = atts[rand.Next(0, atts.Count)];
		curr.Invoke();

		// Wait till last attack is done
		yield return new WaitWhile(() => attacks.attacking);//*/
		
		// delay between attacks
		yield return new WaitForSeconds(1f);

		// Repeat
		co = StartCoroutine(AttackChoice());//*/
	}

	private void Death()
	{
		IEnumerator Death() 
		{
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
			yield return new WaitForSeconds(.2f);
			yield return new WaitForSeconds(1f);

			ShadeLord.StatueCreator.WonFight = true;
			var bsc = ShadeLord.SceneLoader.SceneController.GetComponent<BossSceneController>();
			GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
			PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
			transitionsFSM.SetState("Out Statue");
			yield return new WaitForSeconds(1);
			bsc.DoDreamReturn();

			Destroy(gameObject);
		}

		StartCoroutine(Death());
	}
}