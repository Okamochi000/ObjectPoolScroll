using System.Collections.Generic;

/// <summary>
/// アイテムをテンプレート化したオブジェクトプールスクロール
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPoolScrollTemplate<T> : ObjectPoolScroll
{
    public List<T> ItemParamList { get; private set; } = new List<T>();

    /// <summary>
    /// アイテムを追加する
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(T itemParam)
    {
        ItemParamList.Add(itemParam);
        SetItemCount(ItemParamList.Count);
    }

    /// <summary>
    /// アイテム配列を追加する
    /// </summary>
    /// <param name="itemParams"></param>
    public void AddItems(T[] itemParams)
    {
        ItemParamList.AddRange(itemParams);
        SetItemCount(ItemParamList.Count);
    }

    /// <summary>
    /// アイテムを挿入する
    /// </summary>
    /// <param name="index"></param>
    /// <param name="itemParam"></param>
    public void Insertitem(int index, T itemParam)
    {
        int prevTopIndex = TopIndex;
        if (index > ItemParamList.Count) { index = ItemParamList.Count; }
        ItemParamList.Insert(index, itemParam);
        SetItemCount(ItemParamList.Count);

        // スクロール位置が変わらないようにする
        if (IsItemActive(index) || index < TopIndex)
        {
            SeekTopIndex(prevTopIndex + 1);
        }
    }

    /// <summary>
    /// 新しいアイテムリストに差し替える
    /// </summary>
    /// <param name="itemParams"></param>
    public void Replaceitems(T[] itemParams)
    {
        ItemParamList.Clear();
        if (itemParams != null && itemParams.Length > 0) { ItemParamList.AddRange(itemParams); }
        SetItemCount(0);
        SetItemCount(ItemParamList.Count);
    }
}
