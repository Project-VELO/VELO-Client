using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    public GameObject hitEffect_perfect;
    public GameObject hitEffect_great;
    public List<GameObject> hitpads;
    public List<UIFader> TrapezoidList;

    public void PlayHitEffect(int lane, EJudgement judgement)
    {
        int index = lane - 1;
        GameObject spawnEffect;
        if (judgement == EJudgement.PERFECT)
        {
            spawnEffect = hitEffect_perfect;
        }
        else
        {
            spawnEffect = hitEffect_great;
        }
        GameObject effect_pad = Instantiate(spawnEffect, hitpads[index].transform);
        effect_pad.transform.position = hitpads[index].transform.position;
        Destroy(effect_pad, 1.0f);

        TrapezoidList[index].ResetAndShow();
    }
}
