using UnityEngine;
using System.Collections;

public class LocalPlayMovement : MonoBehaviour 
{
		float jumpForce;
		float gravityForce;
		float amountOfBounce;
		float drag;
		float currentRotation;
		float stableRotation;
		bool isMoving;
		public int playerNumber = 0;
		int dashDirection;
		bool dashing;
	
	void Update()
	{

	controlSpeed ();
	controlHeight ();
	preventWallPassThrough ();
	watchForPlayerMovement ();
	checkIfDashing ();
	dashDamage ();

	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Stage") 
		{
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
		}
		if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player2")
		{
			Debug.Log ("hit player");
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
			iTween.Stop (collision.gameObject);
		}
	}
	
	void dashDamage()//adds mass if dashing
	{
		if(gameObject.GetComponent<iTween>() == null)
		{
			gameObject.GetComponent<Rigidbody2D>().mass = 10;
		}
		else
		{
			gameObject.GetComponent<Rigidbody2D>().mass = 50;
		}
	}

	void checkIfDashing()
	{
		if (gameObject.GetComponent<iTween> () == null) 
		{
			dashing = false;
		} 
		else 
		{
			dashing = true;
		}
	}

	void controlHeight()
	{
		if(gameObject.rigidbody2D.velocity.y > 0.1) //if rising
		{
			if(gameObject.transform.localPosition.y < 3)
			{
				drag = 0;
			}
			else
			{
				drag = ((Mathf.Abs(gameObject.transform.position.y - (gameObject.transform.position.y * gameObject.transform.position.y)) / 23) + .25f);
			}
			
			gameObject.GetComponent<Rigidbody2D>().drag = drag;
			//Debug.Log("drag is: " + drag);
		}
		else if(gameObject.rigidbody2D.velocity.y < 0.1)//if falling
		{
			drag = 0;
			gameObject.GetComponent<Rigidbody2D>().drag = drag;
		}
	}
	
	void controlSpeed()
	{
		if (gameObject.rigidbody2D.velocity.x > 20 || gameObject.rigidbody2D.velocity.y > 50 || gameObject.rigidbody2D.angularVelocity > 20) 
		{ 
			gameObject.rigidbody2D.velocity = new Vector2(0,0);
		}
	}
	
	void preventWallPassThrough()
	{
		if (playerNumber == 1) 
		{
			if(gameObject.transform.position.x >= 13.4) //right wall hit
			{
				if(Input.GetKey(KeyCode.LeftArrow))
				{
					iTween.MoveAdd(gameObject, new Vector3(-10,0,0),1);
				}
				else
				{
					iTween.Stop (gameObject);
				}	
			}
			if(gameObject.transform.position.x <= -13.6) //left wall hit
			{
				if(Input.GetKey(KeyCode.RightArrow))
				{
					iTween.MoveAdd(gameObject, new Vector3(10,0,0),1);
				}
				else
				{
					iTween.Stop (gameObject);
				}	
			}
		}
		if (playerNumber == 2)
		{
			if(gameObject.transform.position.x >= 13.4) //right wall hit
			{
				if(Input.GetKey(KeyCode.A))
				{
					iTween.MoveAdd(gameObject, new Vector3(-10,0,0),1);
				}
				else
				{
					iTween.Stop (gameObject);
				}	
			}
			if(gameObject.transform.position.x <= -13.6) //left wall hit
			{
				if(Input.GetKey(KeyCode.D))
				{
					iTween.MoveAdd(gameObject, new Vector3(10,0,0),1);
				}
				else
				{
					iTween.Stop (gameObject);
				}	
			}
		}
	}

	void playerDash(int dashDirection, int dashSpeed)
	{
		//preventWallPassThrough ();

		if(gameObject.GetComponent<iTween>() == null)
		{
			iTween.MoveAdd(gameObject, new Vector3(dashDirection,0,0),dashSpeed);	
		}
		else
		{
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
			gameObject.GetComponent<Rigidbody2D>().mass = 50;
		}
	}
	void watchForPlayerMovement()
	{
		if (playerNumber == 1) 
		{
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				playerDash(-5,1);
			}
			if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				playerDash(5,1);
			}
			if(Input.GetKeyDown (KeyCode.DownArrow))
			{
				gameObject.GetComponent<Rigidbody2D>().gravityScale = 10;
			}
		}

		if(playerNumber == 2)
		{
			if(Input.GetKey(KeyCode.A))
			{
				playerDash(-5,1);
			}
			if(Input.GetKey(KeyCode.D))
			{
				playerDash(5,1);
			}
			if(Input.GetKeyDown (KeyCode.S))
			{
				gameObject.GetComponent<Rigidbody2D>().gravityScale = 10;
			}
		}
	}
	
	
}