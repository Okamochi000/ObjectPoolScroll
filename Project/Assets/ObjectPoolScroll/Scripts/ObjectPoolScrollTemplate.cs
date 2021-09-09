using System.Collections.Generic;

/// <summary>
/// �A�C�e�����e���v���[�g�������I�u�W�F�N�g�v�[���X�N���[��
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPoolScrollTemplate<T> : ObjectPoolScroll
{
    public List<T> ItemParamList { get; private set; } = new List<T>();

    /// <summary>
    /// �A�C�e����ǉ�����
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(T itemParam)
    {
        ItemParamList.Add(itemParam);
        SetItemCount(ItemParamList.Count);
    }

    /// <summary>
    /// �A�C�e���z���ǉ�����
    /// </summary>
    /// <param name="itemParams"></param>
    public void AddItems(T[] itemParams)
    {
        ItemParamList.AddRange(itemParams);
        SetItemCount(ItemParamList.Count);
    }

    /// <summary>
    /// �A�C�e����}������
    /// </summary>
    /// <param name="index"></param>
    /// <param name="itemParam"></param>
    public void Insertitem(int index, T itemParam)
    {
        int prevTopIndex = TopIndex;
        if (index > ItemParamList.Count) { index = ItemParamList.Count; }
        ItemParamList.Insert(index, itemParam);
        SetItemCount(ItemParamList.Count);

        // �X�N���[���ʒu���ς��Ȃ��悤�ɂ���
        if (IsItemActive(index) || index < TopIndex)
        {
            SeekTopIndex(prevTopIndex + 1);
        }
    }

    /// <summary>
    /// �V�����A�C�e�����X�g�ɍ����ւ���
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
