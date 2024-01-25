using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [HideInInspector]
    public UniversalInputManager inputManager;
    [HideInInspector]
    public SoundManager soundManager;
    [HideInInspector]
    public LevelManager levelManager;
    [HideInInspector]
    public CameraManager camManager;
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public DialogueManager dialogueManager;
    [HideInInspector]
    public EffectManager effectManager;

    public bool sceneReady;
    public LevelState levelSavePoint;

    [Header("Scenes")]
    public SceneObject menuScene;

    [Header("Cutscenes")]
    public SceneObject introCutscene;

    [Header("Tutorial")]
    public SceneObject level_1_1;
    public SceneObject level_1_2;
    public SceneObject level_1_3;

    [Header("Levels")]
    public SceneObject level_2;
    public SceneObject level_3;

    [Header("The Witness")]
    public SceneObject witness;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inputManager = GetComponent<UniversalInputManager>();
        soundManager = GetComponent<SoundManager>();
        gameConsole = GetComponent<GameConsole>();
        dialogueManager = GetComponent<DialogueManager>();
        camManager = GetComponentInChildren<CameraManager>();
        uiManager = GetComponentInChildren<UIManager>();
        effectManager = GetComponent<EffectManager>();
    }


    #region <<<< SCENE MANAGEMENT >>>>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += NewSceneReset;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= NewSceneReset;
    }


    public void NewSceneReset(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("New scene loaded: " + scene.name);

        StartCoroutine(SceneSetup());
    }

    IEnumerator SceneSetup()
    {
        sceneReady = false;

        gameConsole.Clear();

        // get new level manager
        while (levelManager == null)
        {
            try
            {
                // check for level manager
                levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();

                if (levelManager == null)
                {
                    // check for cutscene
                    levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<IntroCutsceneManager>();
                }
            }
            catch
            {

            }



            yield return null;
        }

        Debug.Log("Level Manager found");

        levelManager.gameConsole = GetComponent<GameConsole>();
        levelManager.soundManager = GetComponent<SoundManager>();
        levelManager.dialogueManager = GetComponent<DialogueManager>();
        levelManager.camManager = GetComponentInChildren<CameraManager>();
        levelManager.uiManager = GetComponentInChildren<UIManager>();

        // set uiManager
        uiManager.levelManager = levelManager;

        // setup camera
        camManager.NewSceneReset();
        camManager.currTarget = levelManager.camStart;

        while (!camManager.IsCamAtTarget(camManager.currTarget, 2))
        {
            yield return null;
        }

        sceneReady = true;

        Debug.Log("Scene Is Ready");

        levelManager.StartLevelFromPoint(levelSavePoint);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(menuScene);
        soundManager.backgroundMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Destroy(this.gameObject);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(introCutscene);
    }

    public void LoadScene(SceneObject scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadLevel(int level)
    {
        if (level == 2)
        {
            SceneManager.LoadScene(level_2);

        }

        if (level == 3)
        {
            SceneManager.LoadScene(level_3);

        }
    }

    public void RestartLevelFromSavePoint()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    #region >> SCENE OBJECT (( allows for drag / dropping scenes into inspector ))
    [System.Serializable]
    public class SceneObject
    {
        [SerializeField]
        private string m_SceneName;

        public static implicit operator string(SceneObject sceneObject)
        {
            return sceneObject.m_SceneName;
        }

        public static implicit operator SceneObject(string sceneName)
        {
            return new SceneObject() { m_SceneName = sceneName };
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectEditor : PropertyDrawer
    {
        protected SceneAsset GetSceneObject(string sceneObjectName)
        {
            if (string.IsNullOrEmpty(sceneObjectName))
                return null;

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.path.IndexOf(sceneObjectName) != -1)
                {
                    return AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset)) as SceneAsset;
                }
            }

            Debug.Log("Scene [" + sceneObjectName + "] cannot be used. Add this scene to the 'Scenes in the Build' in the build settings.");
            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneObj = GetSceneObject(property.FindPropertyRelative("m_SceneName").stringValue);
            var newScene = EditorGUI.ObjectField(position, label, sceneObj, typeof(SceneAsset), false);
            if (newScene == null)
            {
                var prop = property.FindPropertyRelative("m_SceneName");
                prop.stringValue = "";
            }
            else
            {
                if (newScene.name != property.FindPropertyRelative("m_SceneName").stringValue)
                {
                    var scnObj = GetSceneObject(newScene.name);
                    if (scnObj == null)
                    {
                        Debug.LogWarning("The scene " + newScene.name + " cannot be used. To use this scene add it to the build settings for the project.");
                    }
                    else
                    {
                        var prop = property.FindPropertyRelative("m_SceneName");
                        prop.stringValue = newScene.name;
                    }
                }
            }
        }
    }
    #endif
    #endregion


#endregion
}
