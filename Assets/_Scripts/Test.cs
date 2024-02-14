using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] Image img;
    public void Timer()
    {
        img.fillAmount = Random.Range(0f, 1f);
    }
}
