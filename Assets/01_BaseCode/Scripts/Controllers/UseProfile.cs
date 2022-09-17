using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.NiceVibrations;

public class UseProfile : MonoBehaviour
{
    public static int CurrentLevel
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL, 1);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL, value);
            PlayerPrefs.Save();
        }
    }

    public int CurrentLevelPlay
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PLAY, 1);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PLAY, value);
            PlayerPrefs.Save();
        }
    }

    public bool IsRemoveAds
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.REMOVE_ADS, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.REMOVE_ADS, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool OnVibration
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.ONOFF_VIBRATION, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.ONOFF_VIBRATION, value ? 1 : 0);
            MMVibrationManager.SetHapticsActive(value);
            PlayerPrefs.Save();
        }
    }

    public bool OnSound
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.ONOFF_SOUND, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.ONOFF_SOUND, value ? 1 : 0);
            GameController.Instance.musicManager.SetSoundVolume(value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool OnMusic
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.ONOFF_MUSIC, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.ONOFF_MUSIC, value ? 1 : 0);
            GameController.Instance.musicManager.SetMusicVolume(value ? 0.7f : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool IsFirstTimeInstall
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.FIRST_TIME_INSTALL, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.FIRST_TIME_INSTALL, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static int RetentionD
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.RETENTION_D, 0);
        }
        set
        {
            if (value < 0)
                value = 0;

            PlayerPrefs.SetInt(StringHelper.RETENTION_D, value);
            PlayerPrefs.Save();
        }
    }

    public static int DaysPlayed
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.DAYS_PLAYED, 1);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.DAYS_PLAYED, value);
            PlayerPrefs.Save();
        }
    }

    public static int PayingType
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.PAYING_TYPE, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.PAYING_TYPE, value);
            PlayerPrefs.Save();
        }
    }


    public static DateTime FirstTimeOpenGame
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.FIRST_TIME_OPEN_GAME))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.FIRST_TIME_OPEN_GAME));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.FIRST_TIME_OPEN_GAME, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.FIRST_TIME_OPEN_GAME, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }

    public static DateTime LastTimeOpenGame
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_OPEN_GAME))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_OPEN_GAME));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_OPEN_GAME, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_OPEN_GAME, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }
    public static DateTime LastTimeResetSalePackShop
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_RESET_SALE_PACK_SHOP))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_RESET_SALE_PACK_SHOP));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_RESET_SALE_PACK_SHOP, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_RESET_SALE_PACK_SHOP, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }


    public static bool CanShowRate
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CAN_SHOW_RATE, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CAN_SHOW_RATE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsTutedReturn
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_TUTED_RETURN, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.IS_TUTED_RETURN, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public int CurrentNumReturn
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_RETURN, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_RETURN, 2));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_RETURN, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumAddBranch
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_ADD_STAND, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_ADD_BRANCH, 1));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_ADD_STAND, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumRemoveBomb
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_BOMB, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_BOMB, 0));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_BOMB, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumRemoveCage
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_CAGE, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_CAGE, 0));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_CAGE, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumRemoveEgg
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_EGG, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_EGG, 0));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_EGG, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumRemoveSleep
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_SLEEP, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_SLEEP, 0));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_SLEEP, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentNumRemoveJail
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_JAIL, RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_JAIL, 0));
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_JAIL, value);
            PlayerPrefs.Save();
        }
    }

    public bool IsTutedBuyStand
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_TUTED_BUY_STAND, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.IS_TUTED_BUY_STAND, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public string CurrentBirdSkin
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.CURRENT_BIRD_SKIN, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.CURRENT_BIRD_SKIN, value);
            PlayerPrefs.Save();
        }
    }

    public string OwnedBirdSkin
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.OWNED_BIRD_SKIN, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.OWNED_BIRD_SKIN, value);
            PlayerPrefs.Save();
        }
    }
    public string RandomBirdSkinInShop
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_BIRD_SKIN_IN_SHOP, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_BIRD_SKIN_IN_SHOP, value);
            PlayerPrefs.Save();
        }
    }
    public string RandomBirdSkinSaleWeekend1
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_BIRD_SKIN_SALE_WEEKEND_1, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_BIRD_SKIN_SALE_WEEKEND_1, value);
            PlayerPrefs.Save();
        }
    }
    public string RandomBranchSaleWeekend2
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_BRANCH_SALE_WEEKEND_2, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_BRANCH_SALE_WEEKEND_2, value);
            PlayerPrefs.Save();
        }
    }
    public string RandomThemeSaleWeekend2
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_THEME_SALE_WEEKEND_2, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_THEME_SALE_WEEKEND_2, value);
            PlayerPrefs.Save();
        }
    }


    public int CurrentBranchSkin
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_BRANCH_SKIN, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_BRANCH_SKIN, value);
            PlayerPrefs.Save();
        }
    }

    public string OwnedBranchSkin
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.OWNED_BRANCH_SKIN, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.OWNED_BRANCH_SKIN, value);
            PlayerPrefs.Save();
        }
    }
    public string RandomBranchInShop
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_BRANCH_IN_SHOP, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_BRANCH_IN_SHOP, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentTheme
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_THEME, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_THEME, value);
            PlayerPrefs.Save();
        }
    }
    public string OwnedThemeSkin
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.OWNED_THEME, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.OWNED_THEME, value);
            PlayerPrefs.Save();
        }
    }

    public string RandomThemeInShop
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.RANDOM_THEME_IN_SHOP, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.RANDOM_THEME_IN_SHOP, value);
            PlayerPrefs.Save();
        }
    }

    public string CurrentRandomBird
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.CURRENT_RANDOM_BIRD_SKIN, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.CURRENT_RANDOM_BIRD_SKIN, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentRandomBranch
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_RANDOM_BRANCH_SKIN, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_RANDOM_BRANCH_SKIN, value);
            PlayerPrefs.Save();
        }
    }
    public int CurrentRandomTheme
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_RANDOM_THEME, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_RANDOM_THEME, value);
            PlayerPrefs.Save();
        }
    }

    public int NumShowedAccumulationRewardRandom//Khi có chim mới => bản mới sẽ NumShowedAccumulationRewardRandom = 0
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_SHOWED_ACCUMULATION_REWARD_RANDOM, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.NUM_SHOWED_ACCUMULATION_REWARD_RANDOM, value);
            PlayerPrefs.Save();
        }
    }

    public static bool StarterPackIsCompleted
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.STARTER_PACK_IS_COMPLETED, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.STARTER_PACK_IS_COMPLETED, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool HasPackInWeekToday
    {
         get
        {
            return PlayerPrefs.GetInt(StringHelper.HAS_PACK_IN_WEEK_TODAY, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.HAS_PACK_IN_WEEK_TODAY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static bool HasPackWeekendToday
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.HAS_PACK_WEEKEND_TODAY, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.HAS_PACK_WEEKEND_TODAY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static string CurrentPackInWeek
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.CURRENT_PACK_IN_WEEK, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.CURRENT_PACK_IN_WEEK, value);
            PlayerPrefs.Save();
        }
    }
    public static string CurrentPackWeekend
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.CURRENT_PACK_WEEKEND, "");
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.CURRENT_PACK_WEEKEND, value);
            PlayerPrefs.Save();
        }
    }
    public static int NumberOfAdsInPlay;
    public static int NumberOfAdsInDay
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUMBER_OF_ADS_IN_DAY, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.NUMBER_OF_ADS_IN_DAY, value);
            PlayerPrefs.Save();
        }
    }

    public static DateTime LastTimeOnline
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_ONLINE))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_ONLINE));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_ONLINE, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_ONLINE, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }
}

