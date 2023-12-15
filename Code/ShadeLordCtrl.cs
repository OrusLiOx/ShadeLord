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
	private IHitEffectReciever hitEffect;
	private ExtraDamageable extDmg;

	// properties
	private GameObject head, title;
	private List<Action> atts;
	private int[] hpMarkers = { 50,50,50,50,300};//{ 400, 450, 300, 750, 2200 };
	private System.Random rand;
	
	private Attacks attacks;

	// trackers
	private Queue<GameObject> spawned;
	private Queue<GameObject> tendrils;

	private bool triggeredRocks;
	private int phase;

	// SETTING STUFF UP
	void Awake()
	{
		// generic unity stuff
		anim = gameObject.GetComponent<Animator>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();//*/

		// hollow knight stuff
		player = HeroController.instance.gameObject;//*/
		hc = HeroController.instance;
		gameObject.AddComponent<SpriteFlash>();

		// properties
		rand = new System.Random();
		head = GameObject.Find("ShadeLord/BeamOrigin/Head");
		title = GameObject.Find("Start");

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
		//hitEffect = gameObject.AddComponent<EnemyHitEffectsShade>();
		GameObject.Find("Start/Wall").transform.localPosition = new Vector3(0, -36.4f, 0);
		health = gameObject.AddComponent<HealthManager>();
		extDmg = gameObject.AddComponent<ExtraDamageable>();

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
		triggeredRocks = false;

		gameObject.layer = 11;

		// copy collector fields
		GameObject refObj = ShadeLord.ShadeLord.GameObjects["Collector"];

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
			}
		}
	}

	private void OnDeath()
	{
		gameObject.GetComponent<Attacks>().Stop();
		StopAllCoroutines();
		Death();
	}
	private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
	{
		Modding.Logger.Log(self.hp + " " + hitinstance.DamageDealt);
		// deal hit then check phase
		orig(self, hitinstance);
		//hitEffect.RecieveHitEffect(hitinstance.Direction);
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
				atts = new List<Action>() { attacks.Spikes };
				break;
			case 3: // platform phase
				atts = new List<Action>()
				{
					attacks.Dash,
					attacks.SweepBeam,
					attacks.CrossSlash,
					attacks.FaceSpikes,
					attacks.AimBeam
				};
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
		GameObject[] allObjects = FindObjectsOfType<GameObject>();
		GameObject hud = GameObject.Find("Hud Canvas");
		IEnumerator Spawn()
		{
			ShadeLord.ShadeLord.PlayMusic(attacks.sounds["Silence"]);
			GameObject.Find("Start").transform.SetPosition3D(0, 3.5f, 38f);
			hud.transform.SetPositionX(hud.transform.GetPositionX()+100);
			GameObject wall = GameObject.Find("Start/Wall");
			SpriteRenderer wallSprite = GameObject.Find("Start/Wall/Black").GetComponent<SpriteRenderer>();
			SpriteRenderer titleSprite = GameObject.Find("Start/Title").GetComponent<SpriteRenderer>();
			Color c = new Color(0,0,0,.5f);

			// BG ANIMATION
			yield return new WaitForSeconds(.1f);


			// APPEAR CLOSER
			// lock movemnet
			if (player.transform.GetPositionX() > transform.GetPositionX())
				HeroController.instance.FaceLeft();
			else
				HeroController.instance.FaceRight();
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR ENTER", false);

			// black screen
			wall.transform.localPosition = new Vector3(0,0f,0);
			yield return new WaitForSeconds(1/6f);
			c = new Color(0, 0, 0, 1 / 7f);
			while (wallSprite.color.a > 0)
			{
				wallSprite.color -= c;
				yield return new WaitForSeconds(1 / 30f);
			}
			// roar animation
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

			// text appear, then leave
			ShadeLord.ShadeLord.PlayMusic(attacks.sounds["ShadeLord_Theme"]);
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
			title.SetActive(false);
			FSMUtility.SendEventToGameObject(HeroController.instance.gameObject, "ROAR EXIT", false);
			yield return new WaitForSeconds(1f);
			co = StartCoroutine(AttackChoice());
		}
		
		foreach (GameObject obj in allObjects)
		{
			Modding.Logger.Log(obj.name + " | " + obj.layer +" | "+ obj.transform.GetPositionZ());
			if (obj.name == "white_palace_particles" || obj.name == "default_particles")
				obj.SetActive(false);
			else if (obj.name == "Hud Canvas")
				hud = obj;
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
				GameObject.Find("Terrain/Area1"),
				GameObject.Find("Terrain/Area1/Floor"),
				GameObject.Find("Terrain/Area1/LeftWall"),
				GameObject.Find("Terrain/Area1/RightWall")
			};
			breakTerrain(GameObject.Find("Terrain/Area1"));

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
			breakTerrain(GameObject.Find("Terrain/Area2/Mid"));
			GameObject.Find("Terrain/VoidHazardTemp").SetActive(false);

			atts = new List<Action>() { attacks.SweepBeam };

			// Wait till reach end section
			yield return new WaitWhile(()=> player.transform.GetPositionX()<208);
			GameObject.Find("Terrain/Area3/CameraLock").SetActive(true);

			GameObject[] GOs = { GameObject.Find("Terrain/Descend/Section2/Plat1"), GameObject.Find("Terrain/Descend/Section2/Plat2")};
			breakTerrain(GameObject.Find("Terrain/Descend/Section2"));
		}
		StartCoroutine(ToEnd());
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

	private void breakTerrain(GameObject go)
	{
		float c = 1/20f;
		Color scale = new Color(c, c, c, 0);
		StartCoroutine(breakTerrain(go));

		IEnumerator breakTerrain(GameObject go)
		{
			SpriteRenderer sprite = go.GetComponentInChildren<SpriteRenderer>();

			foreach (SpriteRenderer s in go.GetComponentsInChildren<SpriteRenderer>())
			{
				StartCoroutine(fade(s));
			}

			// fade to black
			yield return new WaitForSeconds(5f);

			go.SetActive(false);

			// rock particles
			if (!triggeredRocks)
			{
				triggeredRocks = true;
				foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksLeft").GetComponentsInChildren<Rigidbody2D>())
				{
					rb.gameObject.AddComponent<RockParticle>();
				}
				foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksRight").GetComponentsInChildren<Rigidbody2D>())
				{
					rb.gameObject.AddComponent<RockParticle>();
				}
				foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksFloor").GetComponentsInChildren<Rigidbody2D>())
				{
					rb.gameObject.AddComponent<RockParticle>();
				}
			}//*/
		}
		IEnumerator fade(SpriteRenderer sprite)
		{
			// fade to black
			while (sprite.color.r > 0)
			{
				sprite.color -= scale;

				yield return new WaitForSeconds(.1f);
			}
		}
	}

	// ACTIONS
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
		yield return new WaitForSeconds(1f);

		// Repeat
		co = StartCoroutine(AttackChoice());//*/
	}
}