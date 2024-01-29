using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header(" ==== Level 1.1 ==== ")]
    public List<string> witness_lifeFlowerTip;
    public List<string> witness_end_1_1;
    [Header(" ==== Level 1.2 ==== ")]
    public List<string> witness_start_1_2;
    public List<string> witness_end_1_2;
    [Space(10)]
    public List<string> witness_darkLightTip;
    public List<string> witness_goldenOrbTip;
    [Space(10)]
    public List<string> witness_start_1_2_2;
    public List<string> witness_darklightSubmit;
    public List<string> witness_startSoulPanic;
    public List<string> witness_end_1_2_2;

    [Header(" ==== Level 1.3 ==== ")]
    public List<string> witness_totem_introduction;
    public List<string> witness_throwing_introduction;
    public List<string> witness_totem_door_introduction;
    public List<string> witness_flower_introduction;


    [Header("General")]
    public List<string> flowerWorry;
    public List<string> flowerExclamations;

    [Space(10)]
    public List<string> witness_onFail;

}
