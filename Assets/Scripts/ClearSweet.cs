using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSweet : MonoBehaviour {

    public AnimationClip clearAnimation;

    private bool isClearing;

    public AudioClip destoryAudio;

    public bool IsClearing
    {
        get
        {
            return isClearing;
        }
    }

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
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.Play(clearAnimation.name);

            GameManager.Instance.playerScore += 10;
            AudioSource.PlayClipAtPoint(destoryAudio, transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
