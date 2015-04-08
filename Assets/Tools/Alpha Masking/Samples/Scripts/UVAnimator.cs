using UnityEngine;
using System.Collections;

public class UVAnimator : MonoBehaviour
{
	public float USpeed = 0;
	public float VSpeed = 0;

	void Start ()
	{
	
	}

	void Update ()
	{
		Vector2 offset = renderer.material.mainTextureOffset + new Vector2(USpeed * Time.deltaTime, VSpeed * Time.deltaTime);
		offset.x = offset.x % 1f;
		offset.y = offset.y % 1f;
		renderer.material.mainTextureOffset = offset;
	}
}
