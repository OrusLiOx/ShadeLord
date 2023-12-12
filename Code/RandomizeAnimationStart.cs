/*/ 31-10-2023

Randomly wait from 0 to 1 second before playing void tendril animation 

Attach this script to all void tendrils

/*/

using System.Collections;
using UnityEngine;

public class RandomizeAnimationStart : MonoBehaviour
{
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
