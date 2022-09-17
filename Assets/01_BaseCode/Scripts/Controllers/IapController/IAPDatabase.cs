using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

public enum TypePackIAP
{
    NoAdsCoinPack = 0,
    NoAdsHeartPack = 1,
    NoAdsPack = 2,
    BirdSkin_1_0_Pack = 3,
    BirdSkin_2_0_Pack = 4,
    BirdSkin_3_0_Pack = 5,
    BirdSkin_4_0_Pack = 6,
    BirdSkin_5_0_Pack = 7,
    BirdSkin_6_0_Pack = 8,
    BirdSkin_7_0_Pack = 9,
    BirdSkin_8_0_Pack = 10,
    BirdSkin_9_0_Pack = 11,
    BirdSkin_10_0_Pack = 12,
    BirdSkin_1_1_Pack = 13,
    BirdSkin_2_1_Pack = 14,
    BirdSkin_3_1_Pack = 15,
    BirdSkin_4_1_Pack = 16,
    BirdSkin_5_1_Pack = 17,
    BirdSkin_6_1_Pack = 18,
    BirdSkin_7_1_Pack = 19,
    BirdSkin_8_1_Pack = 20,
    BirdSkin_9_1_Pack = 21,
    BirdSkin_10_1_Pack = 22,
    BranchSkin_0_Pack = 23,
    BranchSkin_1_Pack = 24,
    BranchSkin_2_Pack = 25,
    BranchSkin_3_Pack = 26,
    BranchSkin_4_Pack = 27,
    BranchSkin_5_Pack = 28,
    BranchSkin_6_Pack = 29,
    BranchSkin_7_Pack = 30,
    BranchSkin_8_Pack = 31,
    BranchSkin_9_Pack = 32,
    BranchSkin_10_Pack = 33,
    Theme0Pack = 34,
    Theme1Pack = 35,
    Theme2Pack = 36,
    Theme3Pack = 37,
    Theme4Pack = 38,
    Theme5Pack = 39,
    Theme6Pack = 40,
    Theme7Pack = 41,
    Theme8Pack = 42,
    Theme9Pack = 43,
    Theme10Pack = 44
}


[CreateAssetMenu(menuName = "ScriptableObject/IAPDatabase", fileName = "IAPDatabase.asset")]
public class IAPDatabase : SerializedScriptableObject
{
    public List<IAPPack> lstPacksInapp;
    public List<IAPPack> lstPacksNotInapp;

    public IAPPack GetPack(TypePackIAP type)
    {
        Debug.Log("========= Pack ====== " + lstPacksInapp.Count);
        for (var i = 0; i < lstPacksInapp.Count; i++)
        {
            Debug.Log("========= lstPacksInapp[i].type ====== " + lstPacksInapp[i].type);
            if (lstPacksInapp[i].type != type) continue;

            return lstPacksInapp[i];
        }

        return null;
    }


    public IAPPack GetPackNotInapp(TypePackIAP type)
    {
        for (var i = 0; i < lstPacksNotInapp.Count; i++)
        {
            if (lstPacksNotInapp[i].type != type) continue;

            return lstPacksNotInapp[i];
        }

        return null;
    }

    public IAPPack GetPackAll(TypePackIAP type)
    {
        Debug.Log("==========type ======== " + type);
        var pack = GetPack(type);
        if (pack == null)
            pack = GetPackNotInapp(type);

        return pack;
    }
}

public class IAPPack
{
    private UnityAction actClaimDone;
    //                Kiểu     Số lượng

    [HideIf("typeBuy", TypeBuy.Free)] public string defaultPrice;
    public Sprite icon;
    [ShowIf("isSale")] public string idSale;

    public bool isSale;

    public Dictionary<GiftType, int> itemsResult; //Các Item nhận được sau khi mua Pack
    public string namePack;
    [ShowIf("isSale")] public float percentSale;

    [ShowIf("isNotInappPack")] [HideIf("typeBuy", TypeBuy.Free)]
    public int price;

    public ProductType productType;
    public string shortID;
    public string tittle;
    public TypePackIAP type;
    public TypeBuy typeBuy;
    [HideInInspector] public bool isNotInappPack => typeBuy != TypeBuy.Inapp ? true : false;

    public string ProductID => string.Format("{0}.{1}", Config.package_name, shortID);

    public UnityAction ActClaimDone
    {
        set => actClaimDone = value;
    }

    public void Claim(bool isClaimDailyIap = false)
    {
        var value = 0;
        var typeItem = GiftType.Coin;
        foreach (var item in itemsResult)
            switch (type)
            {
                case TypePackIAP.NoAdsCoinPack:

                case TypePackIAP.NoAdsHeartPack:
                case TypePackIAP.NoAdsPack:
                    GameController.Instance.useProfile.IsRemoveAds = true;
                    //GameController.Instance.admobAds.DestroyBanner();
                    EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.REMOVE_ADS);
                    return;
                    break;
            }

        if (typeBuy == TypeBuy.Video)
        {
            var lstReward = new List<GiftRewardShow>();
            foreach (var item in itemsResult)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(item.Key, item.Value);

                var rw = new GiftRewardShow();
                rw.type = item.Key;
                rw.amount = item.Value;

                lstReward.Add(rw);
            }


            RewardIAPBox.Setup2().Show(lstReward, () => { actClaimDone?.Invoke(); });
        }
        else
        {
            var lstReward = new List<GiftRewardShow>();
            foreach (var item in itemsResult)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(item.Key, item.Value);

                var rw = new GiftRewardShow();
                rw.type = item.Key;
                rw.amount = item.Value;

                lstReward.Add(rw);
            }


            RewardIAPBox.Setup2().Show(lstReward, () => { actClaimDone?.Invoke(); });
        }
    }

    public int GetAmount(GiftType itmName)
    {
        var amount = 0;
        if (itemsResult.TryGetValue(itmName, out amount)) return amount;

        return amount;
    }
}