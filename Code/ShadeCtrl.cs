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
	private float xEdge = 18, xCenter = 100;
	private List<(float x, float y)> tpPos;
	private float scale;
	private System.Random rand;
	
	private Attacks attacks;
	// trackers
	private Queue<GameObject> spawned;
	private Queue<GameObject> tendrils;

	private GameObject spike, tendril;

	private bool attacking, faceRight, faceLeft;
	private int phase;

	// SETTING STUFF UP
	void Awake()
	{
		//GameObject.Find("Terrain/Area1/Floor").SetActive(true);

		// generic unity stuff
		anim = gameObject.GetComponent<Animator>();
		spriteRender = gameObject.GetComponent<SpriteRenderer>();
		rigBod = gameObject.GetComponent<Rigidbody2D>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();

		// hollow knight stuff
		player = HeroController.instance.gameObject;
		health = gameObject.AddComponent<HealthManager>();
		hitEffect = gameObject.AddComponent<EnemyHitEffectsShade>();
		extDmg = gameObject.AddComponent<ExtraDamageable>();

		gameObject.AddComponent<SpriteFlash>();

		// properties
		rand = new System.Random();
		scale = transform.GetScaleX();

		// trackers
		spawned = new Queue<GameObject>();
		tendrils = new Queue<GameObject>();

	}
	void Start()
	{
		AssignValues();
		health.OnDeath += OnDeath;
		On.HealthManager.TakeDamage += OnTakeDamage;
		
		attacks = gameObject.GetComponent<Attacks>();
		spike = GameObject.Find("Spike").gameObject;
		spike.SetActive(false);
		tendril = GameObject.Find("Tendril").gameObject;
		tendril.SetActive(false);

		xEdge = attacks.xEdge;
		xCenter = attacks.xCenter;

		atts = new List<Action>()
		{
			Dash,
			CrossSlash,
			SweepBeam,
			AimBeam,
			Spikes
		};

		float y = gameObject.transform.GetPositionY();//110.75f;
		tpPos = new List<(float, float)>()
		{
			(21.4f, y),
			(29.33f, y),
			(38.47f, y)
		};
		
		co = StartCoroutine(AttackChoice());
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
		setHitBox();

		GameObject refObj = ShadeLord.ShadeLord.GameObjects["Collector"];
		// health manager
		HealthManager refHP = refObj.GetComponent<HealthManager>();

		foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name.Contains("Prefab")))
		{
			fi.SetValue(health, fi.GetValue(refHP));
		}
		// hit effects
		EnemyHitEffectsShade effects = refObj.GetComponent<EnemyHitEffectsShade>();
		foreach (FieldInfo fi in typeof(EnemyHitEffectsUninfected).GetFields(BindingFlags.Instance | BindingFlags.Public))
			fi.SetValue(hitEffect, fi.GetValue(effects));

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
			if (t.name == "Spike") continue;
			t.gameObject.AddComponent<DamageHero>();

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

	// FUNCTIONS
	private void nextPhase()
	{
		phase++;

		Modding.Logger.Log("phase: " + phase);
		switch (phase)
		{
			case 1: // add lingering spikes
				((Action)Tendrils).Invoke();
				break;
			case 2: // spam phase
				atts = new List<Action>() { Spikes };
				// set tendrils
				foreach (GameObject obj in tendrils)
					obj.GetComponent<TendrilCtrl>().Kill();
				//SpawnTendril(3);
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

	}
	private void ToPlatform()
	{
		IEnumerator ToPlatform()
		{
			yield return new WaitForSeconds(0f);

			StopCoroutine(co);
			ClearAttacks();

			// transition animation
			SpriteRenderer[] sprites = 
				{ 
					GameObject.Find("Terrain/Area1/Floor/Sprite").GetComponent<SpriteRenderer>(), 
					GameObject.Find("Terrain/Area1/LeftWall/Sprite").GetComponent<SpriteRenderer>(), 
					GameObject.Find("Terrain/Area1/RightWall/Sprite").GetComponent<SpriteRenderer>() 
				};
			yield return StartCoroutine(fade(sprites));
			// floor break
			GameObject.Find("Terrain/Area1").SetActive(false);

			// set attacks + go
			atts = new List<Action>()
			{
				Dash,
				SweepBeam,
				CrossSlash,
				AimBeam,
				Spikes
				//RapidSlash
			};
			/*
			float y = 58.8f;
			tpPos = new List<(float, float)>()
			{
				(21.4f, y),
				(29.33f, y),
				(38.47f, y)
			};

			
			float time = Time.time;

			GameObject camera = GameObject.Find("Terrain/Area2/CameraLockArea/CameraRange");
			yield return new WaitWhile(() => player.transform.GetPositionY() > camera.transform.GetPositionY() + camera.GetComponent<BoxCollider2D>().size.y);
			GameObject.Find("Terrain/Area2/CameraLockArea").SetActive(true);

			yield return new WaitWhile(() => Time.time < time + 5f);
			GameObject.Find("Terrain/Area2/Landing").SetActive(false);*/
			co = StartCoroutine(AttackChoice());
		}
		StartCoroutine(ToPlatform());
	}
	private void ToEnd()
	{
		IEnumerator ToEnd()
		{
			GameObject.Find("Terrain/VoidHazardTemp").SetActive(false);
			atts = new List<Action>() { };

			float c = .02f;
			Color scale = new Color(c, c, c, 0);
			SpriteRenderer[] sprite = { GameObject.Find("Terrain/Area2/Mid/Spirte").GetComponent<SpriteRenderer>() };
			yield return StartCoroutine(fade(sprite));

			GameObject.Find("Terrain/Area2/Mid").SetActive(false);

			atts = new List<Action>() { SweepBeam };
			/*
			tpPos = new List<(float, float)>()
			{
				(21f, 9.5f),
				(30f, 9.5f),
				(39f, 9.5f)
			};*/


			// Wait till reach end section
			yield return new WaitWhile(()=> player.transform.GetPositionX()<208);
			GameObject.Find("Terrain/Area3/CameraLock").SetActive(true);

			SpriteRenderer[] sprites = { GameObject.Find("Terrain/Descend/Section2/Plat1/Sprite").GetComponent<SpriteRenderer>(), GameObject.Find("Terrain/Descend/Section2/Plat2/Sprite").GetComponent<SpriteRenderer>()};
			yield return StartCoroutine(fade(sprites));

			GameObject.Find("Terrain/Descend/Section2/Plat1").SetActive(false);
			GameObject.Find("Terrain/Descend/Section2/Plat2").SetActive(false);
		}
		StartCoroutine(ToEnd());
	}

	private IEnumerator fade(SpriteRenderer[] sprites)
	{
		float c = .02f;
		Color scale = new Color(c, c, c, 0);
		for (int k = 0; k < 6; k++)
		{
			// fade to black
			while (sprites[0].color.r > 0)
			{
				foreach (SpriteRenderer sprite in sprites)
					sprite.color -= scale;

				yield return new WaitForSeconds(.01f);
			}
			// fade back
			if (k != 5)
			{
				while (sprites[0].color.r < 1)
				{
					foreach (SpriteRenderer sprite in sprites)
						sprite.color += scale;

					yield return new WaitForSeconds(.01f);
				}
				yield return new WaitForSeconds(.3f);
			}
		}
	}
	private void setHitBox()
	{
		boxCol.offset = new Vector2(0, .27f);
		boxCol.size = new Vector2(2.84f, 2.2f);
	}
	private GameObject spawnAttack(Transform obj)
	{
		GameObject inst = Instantiate(obj.gameObject);
		inst.transform.localScale = new Vector3(inst.transform.GetScaleX() * scale, inst.transform.GetScaleY() * scale);
		inst.transform.position = obj.position;
		spawned.Enqueue(inst);
		inst.SetActive(true);
		return inst;
	}

	// ACTIONS
	int c;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Modding.Logger.Log("Dash");
			c = 0;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Modding.Logger.Log("CrossSlash");
			c = 1;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Modding.Logger.Log("SweepBeam");
			c = 2;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			Modding.Logger.Log("AimBeam");
			c = 3;
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Modding.Logger.Log("Spikes");
			c = 4;
		}
	}
	private IEnumerator AttackChoice()
	{
		// Attack
		yield return new WaitForSeconds(1f);

		Action curr;
		curr = atts[c % atts.Count];//rand.Next(0, atts.Count)];
		curr.Invoke();

		// Wait till last attack is done
		yield return new WaitWhile(() => attacking);//*/

		// Repeat
		co = StartCoroutine(AttackChoice());//*/
	}

	// ATTACKS
	private void Dash() // start and end animation
	{
		IEnumerator Dash()
		{
		// setup
			attacking = true;
			float hold = transform.GetPositionY();
			int dir; // 1 = start on right, -1= start on left
			if (rand.Next(1, 3) == 1)
				dir = 1;
			else
				dir = -1;

			// set transform + collider
			gameObject.transform.SetScaleX(dir * scale);
			transform.SetPositionX(xCenter + dir * (xEdge - 2.5f));
			transform.SetPositionY(69f);

			boxCol.offset = new Vector2(-0.79f, -0.55f);
			boxCol.size = new Vector2(4.33f, 1.6f);

			// windup
			anim.Play("DashWindup");
			yield return new WaitForSeconds(3/12f);
			GameObject dash = spawnAttack(gameObject.transform.Find("Dash"));
			dash.transform.SetParent(gameObject.transform);
			yield return new WaitForSeconds(3 / 12f);

			// dash
			anim.Play("Dash");
			rigBod.velocity = new Vector2(-1*dir*40f, 0f);
			if (dir == 1)
			{
				yield return new WaitWhile(() => gameObject.transform.GetPositionX() > xCenter - xEdge);
			}
			else
			{
				yield return new WaitWhile(() => gameObject.transform.GetPositionX() < xCenter + xEdge);
			}

			// end
			spawned.Dequeue();
			Destroy(dash);
			anim.Play("DashEnd");
			rigBod.velocity = new Vector2(0f, 0f);
			yield return new WaitForSeconds(3/12f);
			boxCol.enabled = false;
			yield return new WaitForSeconds(3/12f);

			transform.SetScaleX(scale);
			setHitBox();
			transform.SetPositionY(hold);

			attacking = false;
		}

		StartCoroutine(Dash());
	}
	private void CrossSlash() 
	{
		IEnumerator CrossSlash()
		{
		// setup
			float offset = 1.94f;
			attacking = true;
			transform.SetPositionX(player.transform.GetPositionX()+ offset);
			boxCol.offset = new Vector2(-1.34f, .27f);

		// ATTACK 1
			anim.Play("CrossSlashWindup1");
			yield return new WaitForSeconds(2/12f);
			boxCol.enabled = true;
			yield return new WaitForSeconds(10 / 12f);
			// Attack
			anim.Play("CrossSlashAttack1");
			GameObject left = spawnAttack(gameObject.transform.Find("CrossSlash"));
			GameObject right = spawnAttack(gameObject.transform.Find("CrossSlash"));
			right.transform.SetRotationZ(30);
			
			// end attack
			yield return new WaitForSeconds(2/12f);
			Destroy(left);
			Destroy(right);
			spawned.Dequeue();
			spawned.Dequeue();

		// ATTACK 2
			int dir = 1;
			if (player.transform.GetPositionX() < transform.GetPositionX())
			{
				dir = -1;
			}
			transform.SetScaleX(scale * dir);
			anim.Play("CrossSlashWindup2");
			
			yield return new WaitForSeconds(12/12f);

			// attack
			anim.Play("CrossSlashAttack2");
			yield return new WaitForSeconds(1 / 12f);
			left = spawnAttack(gameObject.transform.Find("Sweep"));
			left.transform.SetRotationZ(0);
			yield return new WaitForSeconds(2/12f);
			
		// end attack
			Destroy(left);
			spawned.Dequeue();

			// end 
			anim.Play("CrossSlashEnd");
			yield return new WaitForSeconds(5/12f);
			boxCol.enabled = false;
			yield return new WaitForSeconds(3/12);
			transform.SetScaleX(scale);

			attacking = false;
		}

		StartCoroutine(CrossSlash());
	}

	private void SetBeamAnimation(GameObject beam, string name)
	{
		foreach (Animator anim in beam.GetComponentsInChildren<Animator>())
		{
			anim.Play(name);
		}
	}
	private void SetBeamActive(GameObject beam, bool active)
	{
		foreach (BoxCollider2D col in beam.GetComponentsInChildren<BoxCollider2D>())
		{
			col.enabled = active;
		}
	}
	private void SweepBeam()
	{
		IEnumerator SweepBeam()
		{
		// setup
			attacking = true;
			int rot;
			bool startRight = rand.Next(1, 3) == 1;
			if (startRight)
				rot = 0;
			else
			{
				gameObject.transform.SetScaleX(-1 * scale);
				rot = 180;
			}
			transform.SetPositionX(xCenter - xEdge + rand.Next((int)(xEdge * 8)) / 4.0f);

			// windup
			anim.Play("BeamWindup");
			yield return new WaitForSeconds(2/12f);
			boxCol.enabled = true;

			yield return new WaitForSeconds(10/12f);
			GameObject beam = spawnAttack(gameObject.transform.Find("Beam"));
			beam.transform.SetRotationZ(rot);
			SetBeamAnimation(beam, "BeamChannel");

			yield return new WaitForSeconds(3/12f +.5f);

		// Fire
			beam.GetComponent<BoxCollider2D>().enabled = true;
			SetBeamAnimation(beam, "BeamFire");
			anim.Play("BeamSweep");
			SetBeamActive(beam, true);

			yield return new WaitForSeconds(15/12f);
			// rotate
			int incr = 4;
			float wait = (12/12f)/(360), move=1.75f;
			if (startRight)
			{
				for (int k = -1; k >= -360; k-=incr)
				{
					Modding.Logger.Log(beam.transform.GetPositionX());
					if (k >= -180)
					{
						beam.transform.SetPositionX(beam.transform.GetPositionX()- move/(179f/incr));
					}
					else
					{
						beam.transform.SetPositionX(beam.transform.GetPositionX() + move / (180f / incr));
					}
					beam.transform.SetRotationZ(k);
					yield return new WaitForSeconds(wait);
				}
				beam.transform.SetRotationZ(-360);
			}
			else
			{
				for (int k = 181; k <= 540; k+= incr)
				{

					Modding.Logger.Log(beam.transform.GetPositionX());
					if (k <=360)
					{
						beam.transform.SetPositionX(beam.transform.GetPositionX() + move / (179f / incr));
					}
					else
					{
						beam.transform.SetPositionX(beam.transform.GetPositionX() - move / (180f / incr));
					}
					beam.transform.SetRotationZ(k);
					yield return new WaitForSeconds(wait);
				}

				beam.transform.SetRotationZ(540);
			}
			yield return new WaitForSeconds(.5f);

			// end
			anim.Play("BeamEnd");
			spawned.Dequeue();
			Destroy(beam);
			yield return new WaitForSeconds(2/12f);
			boxCol.enabled = false;
			yield return new WaitForSeconds(2/12f);
			transform.SetScaleX(scale);

			attacking = false;
		}

		StartCoroutine(SweepBeam());
	}
	private void AimBeam() // fire animation
	{
		IEnumerator AimBeam()
		{
			attacking = true;
			transform.SetPositionX(xCenter - xEdge + rand.Next((int)(xEdge * 8)) / 4.0f);
			anim.Play("BeamWindup");
			for (int k = 0; k < 4; k++)
			{
				if (k != 0)
					anim.Play("BeamAim");
				GameObject beam = spawnAttack(gameObject.transform.Find("Beam"));
				SetBeamAnimation(beam, "BeamChannel");
				float x = player.transform.GetPositionX() - beam.transform.GetPositionX();
				float y = player.transform.GetPositionY() - beam.transform.GetPositionY();
				double deg = Math.Atan(Math.Abs(y / x)) * 180.0 / Math.PI;
				if (x < 0)
					deg = 180 - deg;
				if (y < 0)
					deg = -1 * deg;

				beam.transform.SetRotationZ((float)deg);

				yield return new WaitForSeconds(.5f);
				SetBeamAnimation(beam, "BeamFire");
				beam.GetComponent<BoxCollider2D>().enabled = true;
				anim.Play("FireBeam");

				yield return new WaitForSeconds(.3f);
				Destroy(beam);
				spawned.Dequeue();


				yield return new WaitForSeconds(.2f);
			}
			attacking = false;
			yield return null;
		}

		StartCoroutine(AimBeam());
	}

	private void SpikeBurst()
	{
		IEnumerator SpikeBurst()
		{
			attacking = true;
			// channel animation
			yield return new WaitForSeconds(1);

			int offset = rand.Next(0,360);

			for (int k = 2; k >= 0; k--)
			{
				for (int j = 0; j < 5; j++)
				{
					ShootSpike(offset + j * 72, 1);//+k*.5f);
				}
				yield return new WaitForSeconds(.5f);
				offset += 24;
			}

		}
		IEnumerator ShootSpike(float rot, float time)
		{
			GameObject s = Instantiate(spike);
			s.transform.SetParent(gameObject.transform);
			//s.transform.SetPosition2D(-0.293335, 4.973333);
			s.transform.SetRotationZ(rot);
			yield return new WaitForSeconds(.5f);
			s.GetComponent<BoxCollider2D>().enabled = true;
			yield return new WaitForSeconds(time);
			// play return
			s.GetComponent<BoxCollider2D>().enabled = false;
		}

		StartCoroutine(SpikeBurst());
	}
	private void Spikes()
	{
		IEnumerator Spikes(int count)
		{
			HashSet<float> coords = new HashSet<float>();
			int coord;
			float offset = xCenter-xEdge + rand.Next(0, 15) / 10f;
			for (int k = rand.Next(0, 7); k < 20; k++)
			{
				do
				{
					coord = rand.Next(1, 25);
				} while (!coords.Add(coord));

				StartCoroutine(SpawnSpike(coord * (1.5f) + offset));
			}

			yield return new WaitForSeconds(1.75f);
			if (count == -1 && phase == 2)
				StartCoroutine(Spikes(-1));
			else if (count > 1)
				StartCoroutine(Spikes(count - 1));
			else
				attacking = false;
		}

		IEnumerator SpawnSpike(float posX)
		{
			GameObject s = Instantiate(spike);
			spawned.Enqueue(s);
			if(rand.Next(1, 3) == 1);
			s.transform.SetScaleX(-1);
			s.SetActive(true);
			Animator a = s.GetComponent<Animator>();
			a.Play("SpikeSpawn");
			s.transform.SetPositionX(posX);
			s.transform.SetPositionY(72.5015f);
			s.transform.SetScaleX(1.5f);
			s.transform.SetScaleY(1.5f);

			yield return new WaitForSeconds(6/12f);
			a.Play("SpikeFire");
			yield return new WaitForSeconds(4/12f);
			s.GetComponent<BoxCollider2D>().enabled = true;
			s.AddComponent<DamageHero>();
			yield return new WaitForSeconds(.75f);

			a.Play("SpikeEnd");
			yield return new WaitForSeconds(5/12f);
			Destroy(spawned.Dequeue());
		}

		attacking = true;
		boxCol.enabled = true;
		anim.Play("CrossSlashWindup1");
		if (phase == 2)
		{
			transform.SetPositionX(xCenter);
			StartCoroutine(Spikes(-1));
		}
		else
		{
			transform.SetPositionX(xCenter-xEdge + rand.Next(0, (int)(xEdge * 4)) / 4.0f);

			StartCoroutine(Spikes(3));
		}
	}

	private void Tendrils()
	{
		IEnumerator Tendrils(int n)
		{
			yield return new WaitWhile(() => phase != 1);

			if (n%2 == 1)
			{
				SpawnTendril(1);
			}
			else
			{
				SpawnTendril(2);
				SpawnTendril(2);
			}

			yield return new WaitForSeconds(rand.Next(0, 50) / 20.0f + 5f);

			if (phase == 1)
			{
				foreach (GameObject obj in tendrils)
					obj.GetComponent<TendrilCtrl>().Kill();
				StartCoroutine(Tendrils(n+1));
			}
		}
		StartCoroutine(Tendrils(rand.Next(1,3)));
	}
	private void SpawnTendril(int type)
	{
		float position = 0;
		int start = 1;
		bool infin = false;
		GameObject g = gameObject.transform.Find("Tendril1").gameObject;
		switch (type)
		{
			case 2: position = 10;
				start = -1;
				g = gameObject.transform.Find("Tendril2").gameObject;
				break;
			case 3: position = 14;
				start = -1;
				infin = true;
				g = gameObject.transform.Find("Tendril3").gameObject;
				break;
		}

		do
		{
			GameObject tendril = Instantiate(g);

			tendril.SetActive(true);
			tendril.AddComponent<TendrilCtrl>();
			tendril.GetComponent<TendrilCtrl>().infinite = infin;

			tendril.transform.SetPositionX(start*position+xCenter);
			tendril.transform.SetScaleX(start);

			tendrils.Enqueue(tendril);

			start += 2;
		} while (start <= 1);
	}

	// MISC
	private void Idle(float time)
	{
		IEnumerator Idle()
		{
			attacking = true;
			float dist = player.transform.position.x - transform.position.x;
			gameObject.transform.SetScaleX(scale);
			if (dist < -4)
			{
				if (!faceLeft)
				{
					//anim.Play("FaceLeft");
					faceLeft = true;
					yield return new WaitForSeconds(.2f);
				}
				anim.Play("IdleLeft");
				faceRight = false;
			}
			else if (dist > 4)
			{
				if (!faceRight)
				{
					//anim.Play("FaceRight");
					faceRight = true;
					yield return new WaitForSeconds(.2f);
				}
				anim.Play("IdleRight");
				faceLeft = false;
			}
			else
			{
				anim.Play("Idle");
				faceRight = false;
				faceLeft = false;
			}

			yield return new WaitForSeconds(.1f);
			if (Time.time < time)
				StartCoroutine(Idle());
			else
				attacking = false;
		}

		StartCoroutine(Idle());
	}
	private void ClearAttacks()
	{
		foreach (GameObject obj in tendrils)
			Destroy(obj);
		foreach (GameObject obj in spawned)
			Destroy(obj);
	}

	private void Teleport((float x, float y) coord)
	{
		IEnumerator Teleport()
		{
			transform.position = new Vector2(coord.x, coord.y);
			yield return null;
		}

		StartCoroutine(Teleport());
	}
	private void Teleport()
	{
		(float x, float y) coord = (tpPos[rand.Next(0, tpPos.Count)]);
		Teleport(coord);
	}
	private void Teleport(float x)
	{
		if (x < xCenter-xEdge)
			x = xCenter - xEdge;
		else if (x > xCenter + xEdge)
			x = xCenter + xEdge;
		(float x, float y) coord = (x, transform.GetPositionY());
		Teleport(coord);
	}

	private void Area1Fade()
	{
		IEnumerator fade()
		{
			SpriteRenderer[] sprites = { GameObject.Find("Terrain/Area1/Floor/Sprite").GetComponent<SpriteRenderer>(), GameObject.Find("Terrain/Area1/LeftWall/Sprite").GetComponent<SpriteRenderer>(), GameObject.Find("Terrain/Area1/RightWall/Sprite").GetComponent<SpriteRenderer>() };
			float c = .04f;
			Color scale = new Color(c, c, c, 0);
			for (int k = 0; k < 6; k++)
			{
				// fade to black
				while (sprites[0].color.r > 0)
				{
					sprites[0].color -= scale;
					sprites[1].color -= scale;
					sprites[2].color -= scale;

					yield return new WaitForSeconds(.01f);
				}
				// fade back
				if (k != 5)
					while (sprites[0].color.r < 1)
					{
						sprites[0].color += scale;
						sprites[1].color += scale;
						sprites[2].color += scale;

						yield return new WaitForSeconds(.01f);
					}
				yield return new WaitForSeconds(.3f);
			}
		}
		StartCoroutine(fade());
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
			attacking = false;
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