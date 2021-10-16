using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 文字型アイテムスクロール
/// </summary>
public class StringinfiniteScroll : ObjectPoolScrollTemplate<string>
{
    /// <summary>
    /// アイテムが更新された
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetObject"></param>
    protected override void OnUpdateItem(int index, RectTransform targetObject)
    {
        string itemParam = ItemParamList[index];
        targetObject.GetComponentInChildren<Text>().text = itemParam;
    }
}
