using UnityEngine;
using System.Collections;

public class MyPlaneRigidBody : MyRigidBody {

    public Vector3 normal;
    public float distance;

	// Use this for initialization
	void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        CheckCollision(GameObject.Find("Sphere1").GetComponent<MySphereRigidBody>());
	}

    public override MyCollision CheckCollision(MyRigidBody other)
    {
        // declare null MyCollision for return
        MyCollision collision = null;

        // check for collision with other spheres
        //if (other is MySphereRigidBody)
        //{
        //    // distance of sphere centre from plane
        //    float distanceFromSphereCentre = Mathf.Abs(Vector3.Dot(normal, other.transform.position)) + distance;

        //    // distance of sphere from plane
        //    float distanceFromSphere = distanceFromSphereCentre - (other as MySphereRigidBody).radius;

        //    // check intersection
        //    if (distanceFromSphere < 0)
        //    {
        //        Debug.LogError("Intersected");
        //    }
        //}

        return collision;
    }
}
