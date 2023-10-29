using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
	private string anim1,anim2;
	// Start is called before the first frame update

	private void OnEnable()
	{
		if (gameObject.name.Contains("Blast"))
		{
			anim1 = "Nothing";
			anim2 = "BeamBlast";
		}
		else
		{
			anim2 = "Beam";
			if (gameObject.name.Contains("("))
				anim1 = "Windup";
			else
				anim1 = "WindupStart";
		}
		if (!gameObject.name.Contains("Blast"))
			GetComponent<BoxCollider2D>().enabled = false;
		StartCoroutine(go());
	}

	IEnumerator go()
	{
		gameObject.GetComponent<Animator>().Play(anim1);
		yield return new WaitForSeconds(1f);
		if (!gameObject.name.Contains("Blast"))
			GetComponent<BoxCollider2D>().enabled = true;
		gameObject.GetComponent<Animator>().Play(anim2);
	}
}
