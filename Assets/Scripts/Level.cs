using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace TDA.BlastTest
{
    [CreateAssetMenu(fileName = "Level", menuName = "GJGBlast/Add level", order = 1)]
    public class Level : ScriptableObject
    {
        public int num;
        public int moveCount = 20;
        public int targetScore = 100;
        [Range(3, 6)] public int colorSize = 4;
        public int iconPhase1 = 4;
        public int iconPhase2 = 7;
        public int iconPhase3 = 10;
        public Vector2Int colRows = Vector2Int.one * 10;
        [Space(20)]
        [HideInInspector] int sunzero;
        public void Randomize()
        {
            moveCount = Random.Range(20, 40);
            targetScore = Random.Range(20, 40) * 20;
            colorSize = Random.Range(3, 7);
            colRows.x = Random.Range(4, 12);
            colRows.y = Random.Range(4, 12);
        }
    }
    [CustomEditor(typeof(Level))]
    public class RandomLevelButton: Editor
    {

        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            Level lvl = (Level)target;
            if(GUILayout.Button("Set Random"))
            {
                lvl.Randomize();
            }
        }
    }
}