using UnityEngine;
using System.Collections;

public class Movement:MonoBehaviour
{
	float jumpForce;
	float gravityForce;
	float amountOfBounce;
	float drag;
	float currentRotation;
	float stableRotation;

	void start()
	{
		gameObject.collider2D.sharedMaterial.bounciness = 1;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if(gameObject.rigidbody2D.velocity.y > 0.1)
		{
			Debug.Log("rising");
			if(gameObject.transform.localPosition.y < 3)
			{
				drag = 0;
			}
			else
			{
				drag = ((Mathf.Abs(gameObject.transform.position.y - 23) / 23) + 1f);
			}

			gameObject.GetComponent<Rigidbody2D>().drag = drag;
			Debug.Log("drag is: " + drag);
		}
		else if(gameObject.rigidbody2D.velocity.y < 0.1)
		{
			Debug.Log("Falling");
			drag = 0;
			gameObject.GetComponent<Rigidbody2D>().drag = drag;
		}

		//gameObject.GetComponent<Rigidbody2D> ().mass = (10 + (gameObject.transform.position.y * 20));


		//currentRotation = gameObject.transform.eulerAngles.z;
		//Debug.Log (rotation);
		//if (currentRotation > 170 || currentRotation < 70) 
		//{
		//}
	}

	void Update()
	{
		jumpForce = 100 - (gameObject.transform.position.y * 5); //tapers off amount of force added as player goes higher
		//gravityForce = (((gameObject.transform.position.y * 2) * (gameObject.transform.position.y * 2)) * -1 ); //adds more and more downward force as player rises

		if(Input.GetKey(KeyCode.LeftArrow))
		{
			gameObject.transform.position += new Vector3(-.1f, 0, 0);
		}

		if(Input.GetKey(KeyCode.RightArrow))
		{
			gameObject.transform.position += new Vector3(.1f, 0, 0);
		}
		//if (Input.GetKey (KeyCode.DownArrow)) 
		//{
		//    gameObject.GetComponent<Rigidbody2D>().drag += 10;
		//}

		if(gameObject.rigidbody2D.velocity.y > 0.1)//if not falling you can add height to your jump
		{

			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				if(gameObject.transform.position.y < 6)
				{
					for(int i = 0; i <= 10; i++)
					{
						gameObject.rigidbody2D.AddForce(new Vector2(0, jumpForce + 200));
					}
				}
				else
				{
					gameObject.rigidbody2D.AddForce(new Vector2(0, jumpForce));
				}

			}
		}


	}

	//void OnCollisionEnter2D(Collision2D collision)
	//{
	//if (collision.gameObject.tag == "Player") 
	//{
	//    Debug.Log ("players collided");
	//    collision.GetComponent
	//}
	//}


}