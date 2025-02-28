//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(CHESSBOARD))]
//public class CHESSBOARDMANAGEREEDITOR : Editor
//{
//    private SerializedProperty chessPieceSOProperty;

//    private void OnEnable()
//    {
//        // Cache the serialized property for chessPieceSO
//        chessPieceSOProperty = serializedObject.FindProperty("chessPieceSO");
//    }

//    public override void OnInspectorGUI()
//    {
//        CHESSBOARD manager = (CHESSBOARD)target;

//        // Update serialized object
//        serializedObject.Update();

//        // Display the chessPieceSO array in the Inspector
//        EditorGUILayout.PropertyField(chessPieceSOProperty, new GUIContent("Chess Piece SO"), true);

//        // Handle chessboard grid logic
//        var chessBoard = manager.ChessBoardBoxManagers;

//        if (chessBoard == null || chessBoard.Count == 0)
//        {
//            EditorGUILayout.HelpBox("Chessboard is not initialized. Resetting to 8x8.", MessageType.Warning);
//            manager.Reset();
//            return;
//        }

//        int rows = chessBoard.Count;
//        int cols = chessBoard[0].columns.Count;

//        // Draw grid
//        for (int row = 0; row < rows; row++)
//        {
//            EditorGUILayout.BeginHorizontal();

//            for (int col = 0; col < cols; col++)
//            {
//                chessBoard[row].columns[col] = (CHESSBOARDBOXMANAGER)EditorGUILayout.ObjectField(
//                    chessBoard[row].columns[col], typeof(CHESSBOARDBOXMANAGER), true, GUILayout.Width(70));
//            }

//            EditorGUILayout.EndHorizontal();
//        }

//        // Apply any modifications to the serialized object
//        serializedObject.ApplyModifiedProperties();

//        // Apply changes
//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(manager);
//        }
//    }
//}
