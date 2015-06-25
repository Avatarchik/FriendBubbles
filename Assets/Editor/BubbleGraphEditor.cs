using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BubbleGraph))]
public class BubbleGraphEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		BubbleGraph bubbleGraph = (BubbleGraph)this.target;
		
		// Draw the default inspector stuff first
		DrawDefaultInspector();
	
		if(GUILayout.Button("Refresh"))
			bubbleGraph.Refresh();

		if(GUILayout.Button("Self Destruct"))
			bubbleGraph.SelfDestruct();
	}
}
