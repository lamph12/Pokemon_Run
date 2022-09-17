using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AppsFlyerObjectScript))]
[CanEditMultipleObjects]
public class AppsFlyerObjectEditor : Editor
{
    private SerializedProperty appID;

    private SerializedProperty devKey;
    private SerializedProperty getConversionData;
    private SerializedProperty isDebug;
    private SerializedProperty UWPAppID;


    private void OnEnable()
    {
        devKey = serializedObject.FindProperty("devKey");
        appID = serializedObject.FindProperty("appID");
        UWPAppID = serializedObject.FindProperty("UWPAppID");
        isDebug = serializedObject.FindProperty("isDebug");
        getConversionData = serializedObject.FindProperty("getConversionData");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUILayout.Box((Texture)AssetDatabase.LoadAssetAtPath("Assets/AppsFlyer/Editor/logo.png", typeof(Texture)),
            GUILayout.Width(600));

        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox(
            "Set your devKey and appID to init the AppsFlyer SDK and start tracking. You must modify these fields and provide:\ndevKey - Your application devKey provided by AppsFlyer.\nappId - For iOS only. Your iTunes Application ID.\nUWP app id - For UWP only. Your application app id",
            MessageType.Info);

        EditorGUILayout.PropertyField(devKey);
        EditorGUILayout.PropertyField(appID);
        EditorGUILayout.PropertyField(UWPAppID);
        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox("Enable get conversion data to allow your app to recive deeplinking callbacks",
            MessageType.None);
        EditorGUILayout.PropertyField(getConversionData);
        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox(
            "Debugging should be restricted to development phase only.\n Do not distribute the app to app stores with debugging enabled",
            MessageType.Warning);
        EditorGUILayout.PropertyField(isDebug);
        EditorGUILayout.Separator();

        EditorGUILayout.HelpBox("For more information on setting up AppsFlyer check out our relevant docs.",
            MessageType.None);


        if (GUILayout.Button("AppsFlyer Unity Docs", GUILayout.Width(200)))
            Application.OpenURL(
                "https://support.appsflyer.com/hc/en-us/articles/213766183-Unity-SDK-integration-for-developers");

        if (GUILayout.Button("AppsFlyer Android Docs", GUILayout.Width(200)))
            Application.OpenURL(
                "https://support.appsflyer.com/hc/en-us/articles/207032126-Android-SDK-integration-for-developers");

        if (GUILayout.Button("AppsFlyer iOS Docs", GUILayout.Width(200)))
            Application.OpenURL(
                "https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS");

        if (GUILayout.Button("AppsFlyer Deeplinking Docs", GUILayout.Width(200)))
            Application.OpenURL(
                "https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-deep-linking-guide#Setups");

        if (GUILayout.Button("AppsFlyer Windows Docs", GUILayout.Width(200)))
            Application.OpenURL(
                "https://support.appsflyer.com/hc/en-us/articles/207032026-Windows-and-Xbox-SDK-integration-for-developers");


        serializedObject.ApplyModifiedProperties();
    }
}