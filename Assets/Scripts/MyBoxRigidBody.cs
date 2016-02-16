using UnityEngine;
using System.Collections;

public class MyBoxRigidBody : MyRigidBody {

    // the centre of the box
    public Vector3 CentrePosition;

    // the half widths of the sides of the box
    public Vector3 Widths;

    // holds the normalised axes for the box
    public Vector3[] Axes = { 
                                new Vector3(1.0f, 0.0f, 0.0f), 
                                new Vector3(0.0f, 1.0f, 0.0f), 
                                new Vector3(0.0f, 0.0f, 1.0f)
                            };

    // Use this for initialization
    void Start()
    {
        base.Start();
    }

    void FixedUpdate()
    {
        // call the base version of the fixed update to handle physics for the object
        base.FixedUpdate();

        // check for collisions against other game objects in the scene
        foreach (var obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
        {
            if (obj.GetComponent<MyPlaneRigidBody>() != null)
            {
                var collision = CheckCollision((MyRigidBody)obj.GetComponent<MyPlaneRigidBody>());
                if (collision != null)
                {
                    // we have collided so we need to do something about it
                    ResolveCollision(collision);
                }
            }
            if (obj.GetComponent<MySphereRigidBody>() != null)
            {
                var collision = CheckCollision((MyRigidBody)obj.GetComponent<MySphereRigidBody>());
                if (collision != null)
                {
                    // we have collided so we need to do something about it
                    ResolveCollision(collision);
                }
            }
            if (obj.GetComponent<MyBoxRigidBody>() != null)
            {
                var collision = CheckCollision((MyRigidBody)obj.GetComponent<MyBoxRigidBody>());
                if (collision != null)
                {
                    // we have collided so we need to do something about it
                    ResolveCollision(collision);
                }
            }
        }
    }

    /// <summary>
    /// Check to see if a collision has occured with any other rigid bodies
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public override MyCollision CheckCollision(MyRigidBody other)
    {
        // declare null MyCollision for return
        MyCollision collision = null;

        // check for collision with other spheres
        if (other is MySphereRigidBody && other.name != name)
        {
            //// combined radius of both spheres
            //float combinedRadius = this.radius + ((MySphereRigidBody)other).radius;

            //// the vector between the centres of the spheres
            //Vector3 direction = other.transform.position - transform.position;

            //// distance between centres
            //float centreDistance = direction.magnitude;

            //// normalise the direction
            //direction /= centreDistance;

            //// check interection
            //if (centreDistance < combinedRadius)
            //{
            //    // create a new collision object, containing the vector of the normal and the distance from the sphere
            //    collision = new MyCollision(this, other, direction, centreDistance);
            //}
        }
        else if (other is MyPlaneRigidBody) //check for collision with a plane
        {
            // get the mesh filter for the plane
            MeshFilter filter = other.gameObject.GetComponent<MeshFilter>();

            // make sure it has a mesh filter
            if (filter && filter.mesh.normals.Length > 0)
            {
                // get one of the vertext normals --- first one should do as the are all the same
                var normal = filter.transform.TransformDirection(filter.mesh.normals[0]);

                float radius = Widths[0] *  Mathf.Abs(Vector3.Dot(normal, Axes[0])) + 
                                Widths[1] *  Mathf.Abs(Vector3.Dot(normal, Axes[1])) +
                                Widths[2] *  Mathf.Abs(Vector3.Dot(normal, Axes[2]));
                
                // the distance of the box's centre from the plane
                float distance = Mathf.Abs(Vector3.Dot(normal, transform.position)) - (other as MyPlaneRigidBody).distance;

                // check for intersection
                if (Mathf.Abs(distance) <= radius)
                {
                    // create a new collision object, containing the vector of the normal and the distance from the plane
                    collision = new MyCollision(this, other, normal, distance);
                }
            }
        }

        // return the collision
        return collision;
    }
}
