using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FishSpawner : MonoBehaviour
{
    public static FishSpawner Instance { get; set; }

    [SerializeField] private GameObject[] fishSpawn;

    [SerializeField] private Sprite[] fishImages;

    [SerializeField] private string[] fishTags;

    [SerializeField] private float spawningFrequency = 2;
    [SerializeField] private float gameDuration = 300f;
    [SerializeField] private float fishSwitchDuration = 60f;
    [SerializeField] private Vector2 randomSpawnAngle = new Vector2(-10, 10);

    [SerializeField] private CanvasGroup wrongFishCanvasGroup;
    [SerializeField] private Image wrongFishImage;
    [SerializeField] private Image wrongFishImage2;

    [SerializeField] private AudioClip goodSound;
    [SerializeField] private AudioClip badSound;

    private Collider spawningCol;

    private bool gameFinished = false;

    private AudioSource audioSource;
    
    private int wrongFishIndex;
    private float wrongFishTime;
    public static string WrongTag;

    private float startTime;

    private float spawnTime;
    private int wrongFishLayer;
    
    [SerializeField]
    private CanvasGroup goodCanvasGroup;
    
    [SerializeField]
    private CanvasGroup badCanvasGroup;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spawningCol = GetComponent<Collider>();
        Instance = this;
        startTime = Time.time;

        wrongFishLayer = LayerMask.NameToLayer("WrongFish");
        ShowNewWrongFish();
        StartCoroutine(SpawnCoroutine());
    }

    private void Update()
    {
        if (gameFinished)
            return;

        if (Time.time - wrongFishTime >= fishSwitchDuration)
            ShowNewWrongFish();

        if (Time.time - startTime >= gameDuration)
            GameOver(true);
    }
    public void GameOver(bool success)
    {
        gameFinished = true;

        Time.timeScale = 0;

        if (success)
        {
            audioSource.PlayOneShot(goodSound);
            goodCanvasGroup.blocksRaycasts = true;
            goodCanvasGroup.DOFade(1, 1).SetUpdate(true);
        }
        else
        {
            audioSource.PlayOneShot(badSound);
            badCanvasGroup.blocksRaycasts = true;
            badCanvasGroup.DOFade(1, 1).SetUpdate(true);
        }

    }
    private void ShowNewWrongFish()
    {
        StartCoroutine(ShowNewWrongFishCoroutine());
    }
    private IEnumerator ShowNewWrongFishCoroutine()
    {
        Time.timeScale = 0;

        wrongFishTime = Time.time;
        wrongFishIndex = Random.Range(0, fishImages.Length);
        wrongFishImage.sprite = wrongFishImage2.sprite = fishImages[wrongFishIndex];
        WrongTag = fishTags[wrongFishIndex];

        yield return wrongFishCanvasGroup.DOFade(1, 1).SetUpdate(true).WaitForCompletion();

        yield return new WaitForSecondsRealtime(3);

        yield return wrongFishCanvasGroup.DOFade(0, 1).SetUpdate(true).WaitForCompletion();
        Time.timeScale = 1;

    }
    private IEnumerator SpawnCoroutine()
    {
        var wait = new WaitForSeconds(1 / spawningFrequency);

        while (!gameFinished)
        {
            yield return wait;

            var i = Random.Range(0, fishImages.Length);

            var spawnPos = spawningCol.ClosestPoint(Random.insideUnitSphere * 100);
            spawnPos.y = 0;

            var go = Instantiate(fishSpawn[i], spawnPos,
                Quaternion.LookRotation(-spawnPos) *
                Quaternion.Euler(0, Random.Range(randomSpawnAngle.x, randomSpawnAngle.y), 0));
        }
    }
}
