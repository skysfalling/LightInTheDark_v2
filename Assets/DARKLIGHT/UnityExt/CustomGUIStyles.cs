namespace Darklight.UnityExt
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class CustomGUIStyles
    {
        public static GUIStyle TitleHeaderStyle => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40
        };

        public static GUIStyle Header1Style => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40
        };

        public static GUIStyle Header2Style => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40
        };

        public static GUIStyle LeftAlignedStyle => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft
        };

        public static GUIStyle CenteredStyle => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        public static GUIStyle RightAlignedStyle => new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight
        };

        public static GUIStyle BoldStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        public static GUIStyle SmallTextStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 10
        };

        public static GUIStyle NormalTextStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 12
        };
    }
}