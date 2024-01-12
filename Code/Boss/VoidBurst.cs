using System.Collections;
using UnityEngine;

public class VoidBurst : MonoBehaviour
{
	public void Start()
	{
		gameObject.GetComponent<BoxCollider2D>().enabled = false;
		// create 3 clones of self as children
		for (int i = 1; i <= 2; i++)
		{
			GameObject child = Instantiate(gameObject, transform);
			Destroy(child.GetComponent<VoidBurst>());
			child.transform.localEulerAngles = new Vector3(0, 0, 180 / i);
			child.transform.position = transform.position;
		}
		StartCoroutine(appear());
		// appear
		IEnumerator appear()
		{
			
			for (float i = .5f; i < 1; i += .1f)
			{
				transform.SetScaleX(i);
				transform.SetScaleY(i);
				yield return new WaitForSeconds(1/60f);
			}

			foreach(Animator anim in GetComponentsInChildren<Animator>())
				anim.Play("Flicker");
		}
	}

	public void Fire()
	{
		StartCoroutine(Fire());
		IEnumerator Fire()
		{
			// fire
			foreach (Animator anim in GetComponentsInChildren<Animator>())
				anim.Play("Fire");
			yield return new WaitForSeconds(3/12f);

			// attack active
			foreach (BoxCollider2D col in GetComponentsInChildren<BoxCollider2D>())
				col.enabled = true;

			yield return new WaitForSeconds(5/12f);
			// end
			foreach (Animator anim in GetComponentsInChildren<Animator>())
				anim.Play("Retract");
			yield return new WaitForSeconds(2/12f);
			foreach (BoxCollider2D col in GetComponentsInChildren<BoxCollider2D>())
				col.enabled = false;
		}
	}
}