using System;
using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float speed;

    [Header("Flee")]
    [SerializeField] private float fleeSpeed;
    [SerializeField] private float fleeTurnSpeed;
    [SerializeField] private float fleeThreshold;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        var sharkPos = SharkController.Instance.transform.position;
        var distance = Vector3.Distance(sharkPos, rb.position);

        if (distance > fleeThreshold)
            rb.velocity = transform.forward * speed;
        else
        {
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(transform.position - sharkPos), fleeTurnSpeed * Time.deltaTime);
            rb.velocity = transform.forward * Mathf.Lerp(speed, fleeSpeed, 1 - (distance / fleeThreshold));
        }
    }
}
