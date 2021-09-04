using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �����^�A�C�e���X�N���[��
/// </summary>
public class StringinfiniteScroll : ObjectPoolScrollTemplate<string>
{
    /// <summary>
    /// �A�C�e�����X�V���ꂽ
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetObject"></param>
    protected override void OnUpdateItem(int index, RectTransform targetObject)
    {
        string itemParam = ItemParamList[index];
        targetObject.GetComponentInChildren<Text>().text = itemParam;
    }
}
