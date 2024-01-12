/*/

makes GameObject spin

dir = 1 for counterclock wise, -1 for clock wise
speed = degrees/second

/*/

using System.Collections;
using UnityEngine;

public class Spin : MonoBehaviour
{
	public float speed;
	public int dir;
	public void Start()
	{
		speed = 15f;
		dir = -1;
	}

	void Update()
	{
		transform.Rotate(0, 0, dir*speed * Time.deltaTime, Space.World); //rotates 50 degrees per second around z axis
	}
}