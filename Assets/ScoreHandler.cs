using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHandler : MonoBehaviour
{
    public static ScoreHandler Instance { get; private set; }

    private float bonusMultiplier = 1;
    private float lastFishEatenTime = -1000;

    private int score;

    [SerializeField] private int scorePerFish = 50;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI badScoreText;
    [SerializeField] private TextMeshProUGUI goodScoreText;
    
    [SerializeField] private float bonusTime = 4f;

    [SerializeField] private GameObject bonus;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private Slider bonusSlider;

    [SerializeField] private float bonusMultiplierIncrements = 0.5f;
    [SerializeField] private int lives = 4;

    [SerializeField] private Image[] livesImages;


    public bool IsInBonus => Time.time - lastFishEatenTime <= bonusTime;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (IsInBonus && bonusMultiplier > 1)
        {
            bonus.SetActive(true);
            bonusSlider.value = 1 - (Time.time - lastFishEatenTime) / bonusTime;
        }
        else
        {
            bonus.SetActive(false);
            bonusMultiplier = 1;
        }

        for (int i = 0; i < livesImages.Length; i++)
        {
            livesImages[i].enabled = i < lives;
        }
        
        scoreText.text = $"SCORE : {score:D4}";
        badScoreText.text = $"SCORE : {score:D4}";
        goodScoreText.text = $"SCORE : {score:D4}";
        bonusText.text = $"BONUS! {bonusMultiplier:F1}X";
    }

    public void OnFishEaten()
    {
        if (IsInBonus)
        {
            bonusMultiplier += bonusMultiplierIncrements;
            bonus.transform.DOKill(true);
            bonus.transform.DOPunchScale(Vector3.one * .2f, 0.5f);
        }

        lastFishEatenTime = Time.time;
        score += (int)(scorePerFish * bonusMultiplier);
    }
    public void OnWrongFishEaten()
    {
        lives--;
        bonusMultiplier = 1;
        lastFishEatenTime = Time.time;

        if (lives <= 0)
            FishSpawner.Instance.GameOver(false);
    }
}
