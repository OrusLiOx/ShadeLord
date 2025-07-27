using System.Collections;
using UnityEngine;

public class VoidCircle : MonoBehaviour
{
	Collider2D col;
	GameObject burst;
	SpriteRenderer outline;
	SpriteRenderer sprite;
	public float size = .3f;

	public void Start()
	{
		GetComponent<AudioSource>().outputAudioMixerGroup = HeroController.instance.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
	}
	public void Appear()
	{
		col = GetComponent<CircleCollider2D>();
		col.enabled = false;
		sprite = GetComponent<SpriteRenderer>();
		transform.GetChild(0).SetScaleX(1.01f);
		transform.GetChild(0).SetScaleY(1.01f);

		burst = transform.GetChild(1).gameObject;
		burst.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
		outline = transform.GetChild(0).GetComponent<SpriteRenderer>();

		StartCoroutine(Appear());
		IEnumerator Appear()
		{
			float incr = 60*.2f;
			for (float i = 0; i < incr; i+=1)
			{
				gameObject.transform.SetScaleX(size/incr * i);
				gameObject.transform.SetScaleY(size / incr * i);

				yield return new WaitForSeconds(1 / 60f);
			}
			gameObject.transform.SetScaleX(size);
			gameObject.transform.SetScaleY(size);
		}
	}

	public void Fire(bool playSound = true)
	{
		StartCoroutine(Fire());
		IEnumerator Fire()
		{
			SpriteRenderer burstSprite = burst.GetComponent<SpriteRenderer>();
			float incr = 60f*.1f;
			// burst appear
			for (float i = 0; i < incr; i +=1)
			{
				float c = .5f / incr * i;
				burstSprite.color = new Color(c, c, c, i/incr);
				yield return new WaitForSeconds(1/60f);
			}
			if (playSound)
				GetComponent<AudioSource>().Play();
			outline.enabled = false;
			col.enabled = true;
			while (burstSprite.color.a > 0)
			{
				yield return new WaitForSeconds(1 / 60f);
				burstSprite.color = new Color(0, 0, 0, burstSprite.color.a - (2 / 60f));

				float a = sprite.color.a - (3 / 60f);
				sprite.color = new Color(1, 1, 1, a);
				outline.color = new Color(1, 1, 1, a);
			}
			col.enabled = false;
			// burst fade
			while(sprite.color.a > 0)
			{
				float a = sprite.color.a - (3 / 60f);
				sprite.color = new Color(1, 1, 1, a);
				outline.color = new Color(1, 1, 1, a);
				yield return new WaitForSeconds(1 / 60f);
			}

			yield return new WaitForSeconds(5f);
			Destroy(gameObject);
		}
	}
}
