using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AboutUs : ScriptableObject
{
	[BoxGroup("AboutUs")]
	[HorizontalGroup("AboutUs/Split", 80)]
	[VerticalGroup("AboutUs/Split/Left")]
	[HideLabel, PreviewField(80, ObjectFieldAlignment.Center)]
	public Texture Icon;

	[HorizontalGroup("AboutUs/Split", LabelWidth = 70)]

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("框架名称")]
	[GUIColor(2, 6, 6, 1)]
	public string Name = "YouYouFramework";

	[PropertySpace(10)]
	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("版本号")]
	public string Version = "1.1";

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("作者")]
	public string Author = "边涯";

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("联系方式")]
	public string Contact = "http://www.u3dol.com";

}
