using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SharkController : MonoBehaviour
{
    public static SharkController Instance { get; private set; }

    private Rigidbody rb;
    private Animator anim;

    private AudioSource audioSource;

    [SerializeField] private AudioClip[] sounds;

    [SerializeField] private AudioClip wrongSound;
    
    [SerializeField]
    private float turnSpeed;

    [SerializeField]
    private float speed;

    [SerializeField] private float minSpeed;

    private Vector3 targetPos;
    [SerializeField]
    private float moveThreshold = 0.5f;
    private static readonly int speedID = Animator.StringToHash("Speed");

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Instance = this;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        UpdateTargetPosition();
    }

    private void FixedUpdate()
    {
        var dist = Vector3.Distance(targetPos, rb.position);

        if (dist > moveThreshold)
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(targetPos - rb.position), Mathf.Sqrt(dist) * turnSpeed * Time.deltaTime);

        var velocity = transform.forward * (speed * Mathf.Max(minSpeed, Mathf.Sqrt(dist)));
        anim.SetFloat(speedID, velocity.magnitude);
        rb.velocity = velocity;
    }
    private void UpdateTargetPosition()
    {

        var plane = new Plane(Vector3.up, Vector3.zero);

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        plane.Raycast(ray, out float dist);
        targetPos = ray.GetPoint(dist);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);

        if (other.CompareTag(FishSpawner.WrongTag))
        {
            ScoreHandler.Instance.OnWrongFishEaten();
            audioSource.PlayOneShot(wrongSound);
        }
        else
        {
            anim.SetTrigger("Bite");
            audioSource.PlayOneShot(sounds[Random.Range(0,sounds.Length)]);
            ScoreHandler.Instance.OnFishEaten();
        }
    }
}
