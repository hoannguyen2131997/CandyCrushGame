using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{
    private TextMeshProUGUI txtScore;
    private TextMeshProUGUI txtHighScore;

    private int displayScore;
    private int displayHighScore;
   
    void setScore(int _score)
    {
        displayScore = _score;
        updateDisplay();
        updateDisplayHighScore();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void updateDisplay()
    {
        
    }

    void updateDisplayHighScore()
    {
        
    }
}
