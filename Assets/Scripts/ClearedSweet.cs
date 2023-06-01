using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearedSweet : MonoBehaviour
{
    public AudioClip destoryAudio;

    private bool isClearing;

    public bool IsClearing { get => isClearing;}

    public AnimationClip clearAnimation;
    protected GameSweet sweet;


    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }
    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {  
            anim.Play(clearAnimation.name);
            //玩家得分
            GameManager.Instance.playerScore++;
            //声音播放
            AudioSource.PlayClipAtPoint(destoryAudio, transform.position);

            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
