using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// �I�u�W�F�N�g�v�[���X�N���[��
/// </summary>
public class ObjectPoolScroll : UIBehaviour
{
    public int ItemCount { get; private set; } = 0;
    public int TopIndex { get { return topItemIndex_; } }
    public Action<int, RectTransform> updatedItemCallback = null;

    [SerializeField] private ScrollRect scrollRect = null;
    [SerializeField] private RectTransform itemBase = null;

    private HorizontalOrVerticalLayoutGroup layoutGroup_ = null;
    private RectTransform emptyItemFirst_ = null;
    private RectTransform emptyItemLast_ = null;
    private List<RectTransform> poolItemList_ = new List<RectTransform>();
    private List<GameObject> destroyList_ = new List<GameObject>();
    private int poolCount_ = 0;
    private int topItemIndex_ = 0;
    private bool isVertical_ = false;
    private bool isInitialize_ = false;

    protected override void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// ������
    /// </summary>
    public void Initialize()
    {
        if (isInitialize_) { return; }
        if (scrollRect == null) { return; }
        if (scrollRect.content == null) { return; }
        if (itemBase == null) { return; }

        // ���C�A�E�g�擾
        RectTransform content = scrollRect.content;
        if (!content.TryGetComponent<HorizontalOrVerticalLayoutGroup>(out layoutGroup_)) { return; }

        // ���C�A�E�g�ݒ�
        if (layoutGroup_ is VerticalLayoutGroup)
        {
            // �c�X�N���[��
            isVertical_ = true;
            content.anchorMin = new Vector2(content.anchorMin.x, 1.0f);
            content.anchorMax = new Vector2(content.anchorMax.x, 1.0f);
            content.pivot = new Vector2(content.pivot.x, 1.0f);
        }
        else if (layoutGroup_ is HorizontalLayoutGroup)
        {
            // ���X�N���[��
            isVertical_ = false;
            content.anchorMin = new Vector2(0.0f, content.anchorMin.y);
            content.anchorMax = new Vector2(0.0f, content.anchorMax.y);
            content.pivot = new Vector2(0.0f, content.pivot.y);
        }
        else
        {
            // �G���[
            layoutGroup_ = null;
            return;
        }

        // �R�s�[����\��
        itemBase.gameObject.SetActive(false);

        // ��A�C�e������
        GameObject emptyItemFirst = new GameObject("EmptyItemFirst");
        emptyItemFirst.SetActive(false);
        emptyItemFirst.transform.SetParent(itemBase.parent);
        emptyItemFirst_ = emptyItemFirst.AddComponent<RectTransform>();
        Vector2 firstPivot = emptyItemFirst_.pivot;
        if (isVertical_) { firstPivot.y = 1.0f; }
        else { firstPivot.x = 0.0f; }
        emptyItemFirst_.pivot = firstPivot;
        emptyItemFirst_.sizeDelta = Vector2.zero;
        emptyItemFirst.transform.SetAsFirstSibling();

        // ��A�C�e������
        GameObject emptyItemLast = new GameObject("EmptyItemLast");
        emptyItemLast.SetActive(false);
        emptyItemLast.transform.SetParent(itemBase.parent);
        emptyItemLast_ = emptyItemLast.AddComponent<RectTransform>();
        Vector2 lastPivot = emptyItemFirst_.pivot;
        if (isVertical_) { lastPivot.y = 0.0f; }
        else { lastPivot.x = 1.0f; }
        emptyItemLast_.pivot = lastPivot;
        emptyItemLast_.sizeDelta = Vector2.zero;
        emptyItemLast_.transform.SetAsLastSibling();

        // �������t���O�𗧂Ă�
        isInitialize_ = true;

        // �X�V
        UpdatePoolCount();
        UpdateItemAcive();
        UpdateContentSize();
        UpdateItemPosition();
    }

    protected virtual void Update()
    {
        // ���W�X�V
        UpdateItemPosition();

        // �폜�\��I�u�W�F�N�g�폜
        if (destroyList_.Count > 0)
        {
            foreach (GameObject itemObj in destroyList_) { Destroy(itemObj); }
            destroyList_.Clear();
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (!isInitialize_) { return; }

        UpdatePoolCount();
        UpdateItemAcive();
        UpdateContentSize();
        UpdateItemPosition();
    }

    /// <summary>
    /// �ő�A�C�e������ݒ肷��
    /// </summary>
    /// <param name="count"></param>
    public void SetItemCount(int count)
    {
        if (count < 0) { return; }

        ItemCount = count;
        UpdateContentSize();
        UpdateItemAcive();
        UpdateItemPosition();
    }

    /// <summary>
    /// �A�C�e���S�̂��X�V����
    /// </summary>
    public void Apply()
    {
        if (!isInitialize_) { return; }

        // �S�ẴA�C�e�����A�N�e�B�u�ɂ���
        for (int i = 0; i < poolItemList_.Count; i++) { poolItemList_[i].gameObject.SetActive(false); }

        // �K�v�ȕ������A�N�e�B�u
        UpdateItemAcive();
    }

    /// <summary>
    /// �w��A�C�e�����X�V����
    /// </summary>
    /// <param name="index"></param>
    public void Apply(int index)
    {
        if (!isInitialize_) { return; }

        // �\�����ł���΍X�V����
        if (index >= topItemIndex_ && index < (topItemIndex_ + poolItemList_.Count))
        {
            RectTransform item = poolItemList_[(index - topItemIndex_)];
            if (item.gameObject.activeSelf)
            {
                OnUpdateItem(index, item);
                if (updatedItemCallback != null) { updatedItemCallback(index, item); }
            }
        }
    }

    /// <summary>
    /// �w��A�C�e�����\�����ł��邩
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool IsItemActive(int index)
    {
        if (index < topItemIndex_) { return false; }
        if (index >= (topItemIndex_ + poolItemList_.Count)) { return false; }

        RectTransform item = poolItemList_[(index - topItemIndex_)];
        return item.gameObject.activeSelf;
    }

    /// <summary>
    /// �w��A�C�e���̈ʒu��擪�ɂ���
    /// </summary>
    /// <param name="indeex"></param>
    public void SeekTopIndex(int index)
    {
        if (!isInitialize_) { return; }

        if (ItemCount <= poolCount_) { return; }
        if (index > (ItemCount - poolCount_)) { index = ItemCount - poolCount_; }
        if (topItemIndex_ == index) { return; }

        RectTransform content = scrollRect.content;
        Vector2 offsetMin = content.offsetMin;
        Vector2 offsetMax = content.offsetMax;
        Vector2 itemSize = Vector2.zero;
        if (isVertical_) { itemSize.y = itemBase.sizeDelta.y + layoutGroup_.spacing; }
        else { itemSize.x = itemBase.sizeDelta.x + layoutGroup_.spacing; }
        if (index < topItemIndex_)
        {
            Vector2 size = (topItemIndex_ - index) * itemSize;
            offsetMin -= size;
            offsetMax -= size;
        }
        else
        {
            Vector2 size = (index - topItemIndex_) * itemSize;
            offsetMin += size;
            offsetMax += size;
        }
        content.offsetMin = offsetMin;
        content.offsetMax = offsetMax;
        UpdateItemPosition();
        Apply();
    }

    /// <summary>
    /// �A�C�e�����X�V���ꂽ
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetObject"></param>
    protected virtual void OnUpdateItem(int index, RectTransform targetObject) { }

    /// <summary>
    /// �v�[�������X�V����
    /// </summary>
    private void UpdatePoolCount()
    {
        poolCount_ = 1;
        RectTransform content = scrollRect.content;
        RectTransform contentParent = content.parent.GetComponent<RectTransform>();
        if (isVertical_)
        {
            float itemSize = itemBase.sizeDelta.y + layoutGroup_.spacing;
            if (itemSize > 0.0f) { poolCount_ = (int)(Mathf.Max(contentParent.rect.size.y - layoutGroup_.spacing, 0.0f) / itemSize) + 2; }
        }
        else
        {
            float itemSize = itemBase.sizeDelta.x + layoutGroup_.spacing;
            if (itemSize > 0.0f) { poolCount_ = (int)(Mathf.Max(contentParent.rect.size.x - layoutGroup_.spacing, 0.0f) / itemSize) + 2; }
        }
    }

    /// <summary>
    /// �R���e���c�T�C�Y�X�V
    /// </summary>
    private void UpdateContentSize()
    {
        if (!isInitialize_) { return; }

        RectTransform content = scrollRect.content;
        RectTransform contentParent = content.parent.GetComponent<RectTransform>();
        Vector2 contentSize = content.sizeDelta;
        if (isVertical_)
        {
            float height = layoutGroup_.padding.top + layoutGroup_.padding.bottom;
            height += itemBase.sizeDelta.y * ItemCount;
            if (ItemCount > 1) { height += layoutGroup_.spacing * (ItemCount - 1); }
            if (ItemCount > 0 && contentParent.rect.size.y < height) { contentSize.y = height; }
            else { contentSize.y = contentParent.rect.size.y; }
        }
        else
        {
            float width = layoutGroup_.padding.left + layoutGroup_.padding.right;
            width += itemBase.sizeDelta.x * ItemCount;
            if (ItemCount > 1) { width += layoutGroup_.spacing * (ItemCount - 1); }
            if (ItemCount > 0 && contentParent.rect.size.x < width) { contentSize.x = width; }
            else { contentSize.x = contentParent.rect.size.x; }
        }

        content.sizeDelta = contentSize;
    }

    /// <summary>
    /// �A�C�e���̕\����Ԑؑ�
    /// </summary>
    private void UpdateItemAcive()
    {
        if (!isInitialize_) { return; }

        if (poolCount_ < poolItemList_.Count)
        {
            // �v�[������A�C�e���������ꍇ�͔j��
            RectTransform[] items = poolItemList_.ToArray();
            for (int i = poolCount_; i < poolItemList_.Count; i++)
            {
                GameObject itemObj = items[i].gameObject;
                itemObj.SetActive(false);
                destroyList_.Add(itemObj);
            }
            poolItemList_.RemoveRange(poolCount_, (poolItemList_.Count - poolCount_));
            emptyItemFirst_.transform.SetAsFirstSibling();
            emptyItemLast_.transform.SetAsLastSibling();
        }
        else if (poolCount_ > poolItemList_.Count)
        {
            // �v�[������A�C�e�������Ȃ��ꍇ�͒ǉ�
            while (poolCount_ > poolItemList_.Count)
            {
                GameObject copy = GameObject.Instantiate(itemBase.gameObject, itemBase.parent);
                copy.name = "ItemPool";
                RectTransform itemRectTransfrom = copy.GetComponent<RectTransform>();
                poolItemList_.Add(itemRectTransfrom);
                int itemIndex = topItemIndex_ + (poolItemList_.Count - 1);
                if (itemIndex < ItemCount)
                {
                    copy.SetActive(true);
                    OnUpdateItem(itemIndex, itemRectTransfrom);
                    if (updatedItemCallback != null) { updatedItemCallback(itemIndex, itemRectTransfrom); }
                }
            }
            emptyItemFirst_.transform.SetAsFirstSibling();
            emptyItemLast_.transform.SetAsLastSibling();
        }

        // �\����Ԑؑ�
        int activeCount = Mathf.Min(ItemCount, poolCount_);
        for (int i = 0; i < activeCount; i++)
        {
            GameObject itemObj = poolItemList_[i].gameObject;
            if (!itemObj.activeSelf)
            {
                itemObj.SetActive(true);
                int itemIndex = topItemIndex_ + i;
                OnUpdateItem(itemIndex, poolItemList_[i]);
                if (updatedItemCallback != null) { updatedItemCallback(itemIndex, poolItemList_[i]); }
            }
        }
        for (int i = activeCount; i < poolItemList_.Count; i++)
        {
            GameObject itemObj = poolItemList_[i].gameObject;
            if (itemObj.activeSelf) { itemObj.SetActive(false); }
        }
    }

    /// <summary>
    /// �A�C�e�����W���X�V����
    /// </summary>
    private void UpdateItemPosition()
    {
        if (!isInitialize_) { return; }

        if (ItemCount < poolCount_)
        {
            if (topItemIndex_ > 0)
            {
                topItemIndex_ = 0;
                for (int i = 0; i < poolItemList_.Count; i++) { Apply(i); }
            }

            emptyItemFirst_.gameObject.SetActive(false);
            emptyItemLast_.gameObject.SetActive(false);
            return;
        }

        emptyItemFirst_.gameObject.SetActive(true);
        emptyItemLast_.gameObject.SetActive(true);

        int prevTopItemIndex = topItemIndex_;
        if (isVertical_)
        {
            float activeItemHeight = itemBase.sizeDelta.y * poolItemList_.Count;
            activeItemHeight += layoutGroup_.spacing * (poolItemList_.Count - 1);
            RectTransform content = scrollRect.content;

            // ��A�C�e���̃T�C�Y�X�V(��)
            int topOverItemCount = (int)(content.offsetMax.y / (itemBase.sizeDelta.y + layoutGroup_.spacing));
            if (topOverItemCount < 0) { topOverItemCount = 0; }
            else if (topOverItemCount > (ItemCount - poolCount_)) { topOverItemCount = ItemCount - poolCount_; }
            Vector2 topSize = emptyItemFirst_.sizeDelta;
            topSize.y = -layoutGroup_.spacing;
            if (topOverItemCount > 0)
            {
                topSize.y += itemBase.sizeDelta.y * (float)topOverItemCount;
                topSize.y += layoutGroup_.spacing * (float)topOverItemCount;
            }
            emptyItemFirst_.sizeDelta = topSize;

            // ��A�C�e���̃T�C�Y�X�V(��)
            int bottomOverItemCount = ItemCount - poolCount_ - topOverItemCount;
            Vector2 bottomSize = emptyItemLast_.sizeDelta;
            bottomSize.y = -layoutGroup_.spacing;
            if (bottomOverItemCount > 0)
            {
                bottomSize.y += itemBase.sizeDelta.y * (float)bottomOverItemCount;
                bottomSize.y += layoutGroup_.spacing * (float)bottomOverItemCount;
            }
            emptyItemLast_.sizeDelta = bottomSize;

            // �Q�ƈʒu�X�V
            topItemIndex_ = topOverItemCount;
        }
        else
        {
            float activeItemWidth = itemBase.sizeDelta.x * poolItemList_.Count;
            activeItemWidth += layoutGroup_.spacing * (poolItemList_.Count - 1);
            RectTransform content = scrollRect.content;

            // ��A�C�e���̃T�C�Y�X�V(��)
            int leftOverItemCount = (int)(-content.offsetMin.x / (itemBase.sizeDelta.x + layoutGroup_.spacing));
            if (leftOverItemCount < 0) { leftOverItemCount = 0; }
            else if (leftOverItemCount > (ItemCount - poolCount_)) { leftOverItemCount = ItemCount - poolCount_; }
            Vector2 leftSize = emptyItemFirst_.sizeDelta;
            leftSize.x = -layoutGroup_.spacing;
            if (leftOverItemCount > 0)
            {
                leftSize.x += itemBase.sizeDelta.x * (float)leftOverItemCount;
                leftSize.x += layoutGroup_.spacing * (float)leftOverItemCount;
            }
            emptyItemFirst_.sizeDelta = leftSize;

            // ��A�C�e���̃T�C�Y�X�V(�E)
            int rightOverItemCount = ItemCount - poolCount_ - leftOverItemCount;
            Vector2 rightSize = emptyItemLast_.sizeDelta;
            rightSize.x = -layoutGroup_.spacing;
            if (rightOverItemCount > 0)
            {
                rightSize.x += itemBase.sizeDelta.x * (float)rightOverItemCount;
                rightSize.x += layoutGroup_.spacing * (float)rightOverItemCount;
            }
            emptyItemLast_.sizeDelta = rightSize;

            // �Q�ƈʒu�X�V
            topItemIndex_ = leftOverItemCount;
        }

        // �\���A�C�e���X�V
        if (prevTopItemIndex < topItemIndex_)
        {
            int replaceCount = topItemIndex_ - prevTopItemIndex;
            replaceCount = Mathf.Min(replaceCount, poolCount_);
            for (int i = 0; i < replaceCount; i++)
            {
                // ����ւ�
                RectTransform tempItem = poolItemList_[0];
                RectTransform lastItem = poolItemList_[(poolItemList_.Count - 1)];
                poolItemList_.RemoveAt(0);
                poolItemList_.Add(tempItem);
                tempItem.SetSiblingIndex(lastItem.GetSiblingIndex());
                //  �R�[���o�b�N
                int itemIndex = topItemIndex_ + poolCount_ - replaceCount + i;
                OnUpdateItem(itemIndex, tempItem);
                if (updatedItemCallback != null) { updatedItemCallback(itemIndex, tempItem); }
            }
        }
        else if (prevTopItemIndex > topItemIndex_)
        {
            int replaceCount = prevTopItemIndex - topItemIndex_;
            replaceCount = Mathf.Min(replaceCount, poolCount_);
            for (int i = 0; i < replaceCount; i++)
            {
                // ����ւ�
                RectTransform tempItem = poolItemList_[(poolItemList_.Count - 1)];
                RectTransform firstItem = poolItemList_[0];
                poolItemList_.RemoveAt((poolItemList_.Count - 1));
                poolItemList_.Insert(0, tempItem);
                tempItem.SetSiblingIndex(firstItem.GetSiblingIndex());
                // �R�[���o�b�N
                int itemIndex = topItemIndex_ + replaceCount - i - 1;
                OnUpdateItem(itemIndex, tempItem);
                if (updatedItemCallback != null) { updatedItemCallback(itemIndex, tempItem); }
            }
        }

        // ���C�A�E�g�X�V
        if (isVertical_) { layoutGroup_.SetLayoutVertical(); }
        else { layoutGroup_.SetLayoutHorizontal(); }
    }
}
