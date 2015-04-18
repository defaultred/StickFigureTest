using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnlinePlayMovement : MonoBehaviour 
{
	float jumpForce;
	float gravityForce;
	float amountOfBounce;
	float drag;
	float currentRotation;
	float stableRotation;
	bool isMoving;
	int dashDirection;
	bool dashing;
	float dashPower;
	float oldPosition;
	float movementDirection;
	bool collisionHappened;
	public Sprite Idle;
	public Sprite DashLeft;
	public Sprite DashRight;
	public Sprite Down;
	//public GameObject player1Score;
	//public GameObject player2Score;
	int P1Score = 0;
	int P2Score = 0;
	public GameObject childtoChangeArtOn;
	
	
	
	void LateUpdate()
	{
		
		//player1Score.GetComponent<Text> ().text = P1Score.ToString ();
		//player2Score.GetComponent<Text> ().text = P2Score.ToString ();
		
		controlSpeed ();
		controlHeight ();
		watchForPlayerMovement ();
		checkIfDashing ();
		checkDirectionPlayerIsMoving ();
		checkifGroundPounding ();
		
		//Debug.Log (movementDirection);
		//Debug.Log (dashPower);
		
		if(movementDirection < 0)
		{
			if(dashPower > 6)
			{
				dashPower = 6;
			}
			else if(dashPower < 3)
			{
				dashPower = 3;
			}
			else
			{
				dashPower = (gameObject.transform.localPosition.y/3) * (-1);
			}
			//Debug.Log ("Push other player left");
		}
		else if(movementDirection > 0)
		{
			if(dashPower > 6)
			{
				dashPower = 6;
			}
			else if(dashPower < 3)
			{
				dashPower = 3;
			}
			else
			{
				dashPower = (gameObject.transform.localPosition.y/3) * (1);
			}
			//Debug.Log ("Push other player right");
		}
		
		
	}
	
	void checkDirectionPlayerIsMoving()
	{
		movementDirection = transform.position.x - oldPosition;
		oldPosition = transform.position.x;
	}
	
	void OnCollisionStay2D(Collision2D collisionStay)
	{
		if(collisionStay.gameObject.tag == "Player")
		{
			//float enemyPosition;
			//float myPosition;
			//float direction;
			
			Debug.Log ("collisionStay");
			//enemyPosition = collisionStay.transform.position.x;
			//myPosition = transform.position.x;
			//direction = (enemyPosition - myPosition);
			//gameObject.transform.LookAt(collisionStay.transform);
			//iTween.Stop (gameObject);
			//iTween.Stop (collisionStay.gameObject);
			//iTween.MoveAdd(collisionStay.gameObject, new Vector3(dashPower, 0 ,0), 1);
			//iTween.MoveAdd(gameObject, new Vector3(0, 0 ,0), 2);
			//iTween.MoveAdd(gameObject, Vector3.forward, 2);
			//collisionHappened = true;
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "Wall")
		{
			iTween.Stop (gameObject);
		}
		if (collision.gameObject.tag == "Stage") 
		{
			//Debug.Log("Hit Stage");
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
		}
		if (collision.gameObject.tag == "Player")
		{
			
			Debug.Log ("collision");	
			//iTween.Stop (gameObject);
			//iTween.Stop (collision.gameObject);
			iTween.MoveAdd(collision.gameObject, new Vector3(dashPower, 0 ,0), 2);
			iTween.MoveAdd(gameObject, new Vector3(0, 0 ,0), 2);
			collisionHappened = true;
			//gameObject.GetComponent<Rigidbody2D>().interpolation
		}
		if(collision.gameObject.tag == "out of bounds")
		{
			//add score
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
		}
	}
	
	void checkIfDashing()
	{
		if (gameObject.GetComponent<iTween> () == null) 
		{
			dashing = false;
			childtoChangeArtOn.GetComponent<SpriteRenderer>().sprite = Idle;
		} 
		else 
		{
			dashing = true;
		}
	}
	
	void checkifGroundPounding()
	{
		if(gameObject.GetComponent<Rigidbody2D>().gravityScale == 10)
		{
			childtoChangeArtOn.GetComponent<SpriteRenderer>().sprite = Down;
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
		if (gameObject.rigidbody2D.velocity.x > 10 || gameObject.rigidbody2D.velocity.y > 50 || gameObject.rigidbody2D.angularVelocity > 20) 
		{ 
			iTween.Stop(gameObject);
			gameObject.rigidbody2D.velocity = new Vector2(0,0);
			gameObject.transform.position = transform.localPosition;
		}
	}
	
	void playerDash(int dashDirection, float dashSpeed)
	{
		if(gameObject.GetComponent<iTween>() == null || collisionHappened == false)
		{
	
			iTween.MoveAdd(gameObject, new Vector3(0,dashDirection,0),dashSpeed);	
		}
		else
		{
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
		}
	}
	void watchForPlayerMovement()
	{
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				collisionHappened = false;
				childtoChangeArtOn.GetComponent<SpriteRenderer>().sprite = DashLeft;
				playerDash(2,.5f);
			}
			if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				collisionHappened = false;
				childtoChangeArtOn.GetComponent<SpriteRenderer>().sprite = DashRight;
				playerDash(-5,.5f);
			}
			if(Input.GetKeyDown (KeyCode.DownArrow))
			{
				collisionHappened = false;
				gameObject.GetComponent<Rigidbody2D>().gravityScale = 10;
				childtoChangeArtOn.GetComponent<SpriteRenderer>().sprite = Down;
			}

	}
	
	
}