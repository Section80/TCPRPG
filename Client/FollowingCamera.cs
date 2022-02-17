using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public static FollowingCamera instance;

    public Entity entity;

    public bool shouldFollow = true;

    public float deltaY;
    public float deltaZ;

    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<FollowingCamera>();
        CheckEntity();
    }

    // Update is called once per frame
    void Update()
    {
        if (entity != null && shouldFollow)
        {
            if (!entity.target.Equals(new Vector3(-666.0f, -666.0f, -666.0f)))
            {
                Vector3 target = entity.target + new Vector3(0, deltaY, deltaZ);

                if (entity.isFirstPos)
                {
                    transform.position = target;
                }

                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 10.0f);
            }
        } else
        {
            CheckEntity();
        }
    }

    public void CheckEntity()
    {
        if (User.instance)
        {
            entity = User.instance.entity;
        }
    }
}
