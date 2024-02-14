using UnityEngine;

public class GrabLayer : MonoBehaviour
{
    Material default_Material;
    private void Awake() 
    { 
        if (GetComponent<Renderer>() != null) {
            default_Material = GetComponent<Renderer>().material;
        }
    }
    public void ChMaterial(Material NewMaterial)=>GetComponent<Renderer>().material = NewMaterial;
    public void ToDefault()=>GetComponent<Renderer>().material=default_Material;
    public void ChLayer(int NewLayer)=>gameObject.layer = NewLayer;
}