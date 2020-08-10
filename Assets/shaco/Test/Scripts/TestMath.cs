using UnityEngine;
using System.Collections;

namespace shaco.Test
{
    public class TestMath : MonoBehaviour
    {

        void OnGUI()
        {
            if (TestMainMenu.DrawButton("simple"))
            {
                var value = shaco.Base.Operator.CalculateDouble("3.5 + 4 * 2.6");
                Debug.Log("value=" + value);
            }

            if (TestMainMenu.DrawButton("pow"))
            {
                var value = shaco.Base.Operator.CalculateDouble("(4 ^ 2) / 8");
                Debug.Log("value=" + value);
            }

            if (TestMainMenu.DrawButton("complex"))
            {
                var value = shaco.Base.Operator.CalculateDouble("-5.0 * (3.2 + 4 / 2) - -10.9");
                Debug.Log("value=" + value);
            }
            
            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}