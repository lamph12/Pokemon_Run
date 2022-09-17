using System.Collections.Generic;
using UnityEngine;

public class InappBox : BaseBox
{
    private static InappBox instance;

    [SerializeField] private List<IAPItem> lsPackItem;

    public static InappBox Setup()
    {
        if (instance == null) instance = Instantiate(Resources.Load<InappBox>(PathPrefabs.SHOP_BOX));

        return instance;
    }

    protected override void OnStart()
    {
        base.OnStart();

        InitData();
    }

    public override void Show()
    {
        base.Show();
        GameController.Instance.admobAds.DestroyBanner();
        OnCloseBox = () =>
        {
            GameController.Instance.admobAds.ShowBanner();
            OnCloseBox = null;
        };
    }

    private void InitData()
    {
        for (var i = 0; i < lsPackItem.Count; i++) lsPackItem[i].Init();
    }
}