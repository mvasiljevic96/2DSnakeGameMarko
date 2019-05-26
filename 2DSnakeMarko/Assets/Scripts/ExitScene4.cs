using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ExitScene4 : MonoBehaviour
{
    
    public void GoToMainMenu()
    {
        
       
        DBManager.LogOut();
        SceneManager.LoadScene(0);       
    }

  
}
