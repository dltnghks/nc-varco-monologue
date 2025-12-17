using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Tutorial Seqeunce")]
public class TutorialSequence : ScriptableObject
{
    public List<TutorialStep> tutorialSteps;
}
