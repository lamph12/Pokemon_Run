using UnityEngine;
using UnityEngine.UI;

public class RewardIAPBox : PopupRewardBase
{
    public static RewardIAPBox instance;
    public Button bgButton;

    public static RewardIAPBox Setup2()
    {
        if (instance == null) instance = Instantiate(Resources.Load<RewardIAPBox>(PathPrefabs.REWARD_IAP_BOX));

        return instance;
    }
}