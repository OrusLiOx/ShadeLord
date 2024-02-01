using System.Collections;
using UnityEngine;

public class VoidBurst : MonoBehaviour
{
	SpriteRenderer shadow;
	Animator anim;
	public void Start()
	{
		shadow = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		gameObject.GetComponent<BoxCollider2D>().enabled = false;
		anim = GetComponent<Animator>();
		StartCoroutine(appear());

		// appear
		IEnumerator appear()
		{
			Color gradient = new Color(0, 0, 0, .2f);
			shadow.color = new Color(0, 0, 0, 0);
			for (float i = .5f; i < 1; i += .1f)
			{
				transform.SetScaleX(i);
				transform.SetScaleY(i);

				shadow.color += gradient;

				yield return new WaitForSeconds(1/60f);
			}

			anim.Play("Flicker");
		}
	}

	public void Fire()
	{
		StartCoroutine(Fire());
		IEnumerator Fire()
		{
			// fire
			anim.Play("Fire");
			yield return new WaitForSeconds(3/12f);

			// attack active
			GetComponent<BoxCollider2D>().enabled = true;

			yield return new WaitForSeconds(5/12f);

			// end
			anim.Play("Retract");

			Color gradient = new Color(0, 0, 0, .2f);
			for (int i = 0; i < 5; i += 1)
			{
				shadow.color -= gradient;

				yield return new WaitForSeconds(1 / 60f);
			}

			yield return new WaitForSeconds(1/12f);
			GetComponent<BoxCollider2D>().enabled = false;
			Destroy(gameObject);
		}
	}
}