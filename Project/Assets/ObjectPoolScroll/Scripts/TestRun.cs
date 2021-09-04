using UnityEngine;

/// <summary>
/// スクロールテスト
/// </summary>
public class TestRun : MonoBehaviour
{
    [SerializeField] private StringinfiniteScroll[] scrolls = null;
    [SerializeField] [Min(0)] private int itemCount = 100;

    // Start is called before the first frame update
    void Start()
    {
        if (scrolls == null) { return; }
        if (itemCount == 0) { return; }

        string[] itemParams = new string[itemCount];
        for (int i = 0; i < itemCount; i++) { itemParams[i] = (i + 1).ToString(); }
        foreach (StringinfiniteScroll scroll in scrolls)
        {
            if (scroll != null) { scroll.AddItems(itemParams); }
        }
    }
}
