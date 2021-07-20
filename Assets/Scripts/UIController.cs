using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController
{
    //Get information about the current system
    public Text generation, population;
    public int Gen = 1, Pop = 1, totalPop = 85;

    // Update is called once per frame
    public void changeDisplay()
    {
        //display the information to the screen
        generation.text = Gen.ToString();
        population.text = Pop.ToString() + "  /  " + totalPop.ToString();
    }
}
