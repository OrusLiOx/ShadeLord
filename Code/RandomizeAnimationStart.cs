using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAnimationStart : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
		StartCoroutine(go());
    }

	IEnumerator go()
	{
		yield return new WaitForSeconds(Random.Range(0, 100) / 100f);
		gameObject.GetComponent<Animator>().Play("ShortAnim");
	}
}
