using UnityEngine;

public class GemInteractions : MonoBehaviour
{
    public LightReflection lightReflection;
    public virtual void Start() { }
    public virtual void Update() { /*Debug.Log(LightTool());*/ }
    protected virtual LightReflection LightTool()
    {
        return lightReflection;
    }
}
