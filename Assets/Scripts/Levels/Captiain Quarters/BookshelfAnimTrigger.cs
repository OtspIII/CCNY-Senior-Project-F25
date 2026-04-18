using UnityEngine;

public class BookshelfAnimTrigger : MonoBehaviour
{
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayAnimation()
    {
        anim.SetTrigger("Fall");
    }
}
