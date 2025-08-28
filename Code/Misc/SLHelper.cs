/*/

Things i didnt feel like making a whole class for

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using GlobalEnums;


class SLHelper : MonoBehaviour
{
	Coroutine moveWallRoutine;
	// randomize animation times
	public void randomizeAnimStart(List<GameObject> objs, string anim, int frames)
	{
		IEnumerator waitThenPlay(GameObject obj, float wait)
		{
			yield return new WaitForSeconds(wait);
			obj.GetComponent<Animator>().Play(anim);
		}
		foreach (GameObject obj in objs)
		{
			StartCoroutine(waitThenPlay(obj, UnityEngine.Random.Range(0, frames) / 12f));
		}
	}

	// rock particles
	public void launchRocks(GameObject rockContainer)
	{
		foreach (Rigidbody2D rb in rockContainer.GetComponentsInChildren<Rigidbody2D>())
		{
			StartCoroutine(launchRock(rb));
		}
	}

	IEnumerator launchRock(Rigidbody2D rig)
	{
		rig.gravityScale = .7f;
		rig.velocity = new Vector2(UnityEngine.Random.Range(0, 30f) - 15f, UnityEngine.Random.Range(0, 10f) - 5f);
		yield return new WaitForSeconds(10f);
		Destroy(rig.gameObject);
	}

	public void fadeTo(SpriteRenderer sprite, Color target, float time)
	{
		StartCoroutine(fadeTo());
		IEnumerator fadeTo()
		{
			float frames = time * 30f;
			float
				r = (target.r - sprite.color.r) / frames,
				g = (target.g - sprite.color.g) / frames,
				b = (target.b - sprite.color.b) / frames,
				a = (target.a - sprite.color.a) / frames;
			float[] addVals = { 0, 0, 0, 0 };
			float[] subVals = { 0, 0, 0, 0 };

			if (r < 0)
				subVals[0] = r * -1;
			else
				addVals[0] = r * 1;

			if (g < 0)
				subVals[1] = g * -1;
			else
				addVals[1] = g * 1;

			if (b < 0)
				subVals[2] = b * -1;
			else
				addVals[2] = b * 1;

			if (a < 0)
				subVals[3] = a * -1;
			else
				addVals[3] = a * 1;

			Color add = new Color(addVals[0], addVals[1], addVals[2], addVals[3]),
				sub = new Color(subVals[0], subVals[1], subVals[2], subVals[3]);

			for (int i = 0; i < frames; i++)
			{
				sprite.color += add;
				sprite.color -= sub;

				yield return new WaitForSeconds(1 / 30f);
			}
			sprite.color = target;
		}
	}

	public Coroutine moveX(Transform t, float dest, float time)
	{
		IEnumerator move()
		{
			float move = (dest - t.position.x)/ (time*60);
			for (int i = 0; i < time * 60; i++)
			{
				t.SetPositionX(t.position.x + move);
				yield return new WaitForSeconds(1 / 60f);
			}

            t.SetPositionX(dest);
        }
		return StartCoroutine(move());
	}
    public Coroutine moveY(Transform t, float dest, float time)
    {
        IEnumerator move()
        {
            float move = (dest - t.position.y) / (time * 30);
            for (int i = 0; i < time * 30; i++)
            {
                t.SetPositionY(t.position.y + move);
                yield return new WaitForSeconds(1 / 30f);
            }
            t.SetPositionY(dest);
        }
        return StartCoroutine(move());
    }

    // abyss effect stuff
    public void abyssArrive()
	{
		float time = 3;
		moveX(GameObject.Find("AbyssWallLeft").transform, 3, time);
		moveX(GameObject.Find("AbyssWallRight").transform, 44, time);
		moveY(GameObject.Find("AbyssFloor").transform, 64, time);
	}

	public void abyssToEnd()
	{
        HeroController player = HeroController.instance;
        IEnumerator leftWallToEnd()
		{
			GameObject abyssWall = GameObject.Find("AbyssWallLeft");
            GameObject plat = GameObject.Find("Terrain/ToArea3/Plat (1)");
            moveWallRoutine = moveX(abyssWall.transform, plat.transform.position.x - 5f, 20);
            PlayerData.instance.SetVector3("hazardRespawnLocation", new Vector3(plat.transform.position.x, plat.transform.position.y + 1f));

            for (int i = 1; i <= 19; i++)
			{
				plat = GameObject.Find("Terrain/ToArea3/Plat (" + i + ")");
                Vector3 platPos = plat.transform.position;

				yield return new WaitWhile(() => (
				player.gameObject.transform.position.x < platPos.x - plat.GetComponent<BoxCollider2D>().size.x / 2f) ||
				(player.hero_state != ActorStates.idle && player.hero_state != ActorStates.running)
				);

                if (moveWallRoutine != null)
					StopCoroutine(moveWallRoutine);
                moveWallRoutine = moveX(abyssWall.transform, platPos.x - 5f, 1);
                PlayerData.instance.SetVector3("hazardRespawnLocation", new Vector3(platPos.x, platPos.y+1f));

				if (player.gameObject.transform.position.x >= GameObject.Find("Terrain/Area3").transform.GetPositionX() - 5f)
					break;
            }
		}
		IEnumerator AbyssFloorLoad()
		{
            GameObject abyssFloor1 = GameObject.Find("AbyssFloor");
			GameObject abyssFloor2;

			for (int i = 1; i <= 4; i++)
			{
                abyssFloor2 = Instantiate(abyssFloor1);
				abyssFloor2.transform.SetPositionX(25f + 55 * i);
                yield return new WaitWhile(() => (player.transform.position.x < 25f + 55f*i));
				Destroy(abyssFloor1);
				abyssFloor1 = abyssFloor2;
            }
        }
        GameObject particles = GameObject.Find("BackgroundParticles");
        particles.transform.SetParent(GameObject.Find("AbyssFloor").transform, true);

        StartCoroutine(leftWallToEnd());
		StartCoroutine(AbyssFloorLoad());
		moveX(GameObject.Find("AbyssWallRight").transform, GameObject.Find("Terrain/Area3/CameraLock").transform.GetPositionX()+15f, 3f);
    }
}