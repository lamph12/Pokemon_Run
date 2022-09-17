using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Org.BouncyCastle.Utilities;
using Spine;
using Spine.Unity;
using UnityEngine;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;

public static class SpineExtensions
{
    private static readonly Dictionary<ISkeletonAnimation, AnimInfo> animDic =
        new Dictionary<ISkeletonAnimation, AnimInfo>();

    public static void Initialize(this IAnimationStateComponent Anim, bool isOverride = false)
    {
        if (Anim is SkeletonAnimation)
            ((SkeletonAnimation)Anim).Initialize(isOverride);
        else
            ((SkeletonGraphic)Anim).Initialize(isOverride);
    }


    public static AnimInfo SetAnimation(this ISkeletonAnimation anim, string nameAnim,
        bool loop = false, Action onComplete = null, int priority = 0,float timeScale=1f,AnimationState.TrackEntryEventDelegate e=null)
    {
        if (animDic.ContainsKey(anim))
        {
            if (animDic[anim].animName == nameAnim)
                return animDic[anim];
            if (animDic[anim].priority <= priority)
                animDic[anim] = new AnimInfo(nameAnim, loop, priority);
            else
                return animDic[anim];
        }
        else
        {
            animDic.Add(anim, new AnimInfo(nameAnim, loop, priority));
        }
        
        var animState = (IAnimationStateComponent)anim;
        var spine = animState.AnimationState.SetAnimation(1, nameAnim, loop);
        spine.Event += e;
        spine.Complete += entry =>
        {
            animDic.Remove(anim);
            onComplete?.Invoke();
        };
        
        animDic[anim].duration = spine.Animation.Duration;
        animState.AnimationState.TimeScale = timeScale;
        return animDic[anim];
    }
    
    public static AnimInfo FastSetAnimation(this SkeletonGraphic body, string nameAnim,
        bool loop = false, Action onComplete = null, int priority = 0, float timeScale=1f)
    {
        var sk = body.Skeleton.Skin;
        body.Initialize(true);
        body.SetMaterialDirty();
        body.AnimationState.ClearTracks();
        body.Skeleton.SetSkin(sk);
        body.Skeleton.SetSlotsToSetupPose();

        return SetAnimation(body, nameAnim, loop, onComplete, priority);
    }
    // ReSharper disable Unity.PerformanceAnalysis
    public static void FastSetAnimation(this SkeletonGraphic body, AnimData animData,
        bool loop = false, Action onComplete = null, int priority = 0,float? overrideTimeScale=null)
    {
        
        body.AnimationState.ClearTracks();
        var sk = body.Skeleton.Skin;
        body.Initialize(true);
        body.Skeleton.SetSkin(sk);
        body.Skeleton.SetSlotsToSetupPose();
        body.SetMaterialDirty();
        body.AnimationState.SetAnimation(0, animData.Name, loop);
        body.AnimationState.Event += animData.Event;
        body.AnimationState.Complete += entry => onComplete?.Invoke();
        body.timeScale = overrideTimeScale ?? animData.TimeScale;
    }

    public static float GetDuration(this SkeletonGraphic body, AnimData animData)
    {
        return body.SkeletonData.FindAnimation(animData.Name).Duration * animData.TimeScale;
    }
    public static float GetDuration(this SkeletonGraphic body, string animName,float animTimeScale=1f)
    {
        return body.SkeletonData.FindAnimation(animName).Duration * animTimeScale;
    }

    public static bool IsExists(this SkeletonGraphic body, AnimData animData)
    {
        return body.SkeletonData.FindAnimation(animData.Name)!=null;
    }


    private static void SetColor(this ISkeletonAnimation anim, Color color)
    {
        var skeleton = anim.Skeleton;
        foreach (var slot in skeleton.Slots.Where(s => s.Data.Name.StartsWith("Stickman/")))
            slot.SetColor(color);
        foreach (var slot in skeleton.Slots.Where(s => s.Data.Name.StartsWith("swordsman/stickman")))
            slot.SetColor(color);
    }

    public static void SetAnimation(this ISkeletonAnimation anim, string nameAnim, List<string> skinMix,
        bool loop = false, Action onComplete = null)
    {
        var skeleton = anim.Skeleton;
        var animState = (IAnimationStateComponent)anim;
        var skeletonData = skeleton.Data;
        var mixAndMatchSkin = new Skin(skinMix[0]);
        foreach (var skinName in skinMix)
            mixAndMatchSkin.AddSkin(skeletonData.FindSkin(skinName));
        skeleton.SetSkin(mixAndMatchSkin);
        skeleton.SetSlotsToSetupPose();
        var spine = animState.AnimationState.SetAnimation(1, nameAnim, loop);
        spine.Complete += entry => onComplete?.Invoke();
    }

    public static void SetAnimation(this ISkeletonAnimation anim, List<string> skinMix)
    {
        var skeleton = anim.Skeleton;
        // var animState = (IAnimationStateComponent) (anim);
        var skeletonData = skeleton.Data;
        var mixAndMatchSkin = new Skin(skinMix[0]);
        foreach (var skinName in skinMix)
            mixAndMatchSkin.AddSkin(skeletonData.FindSkin(skinName));
        skeleton.SetSkin(mixAndMatchSkin);
        skeleton.SetSlotsToSetupPose();
    }
    

    public static void SetAnimation(this ISkeletonAnimation anim, List<string> skinMix, Color color)
    {
        anim.SetAnimation(skinMix);
        anim.SetColor(color);
    }

    [Serializable]
    public class AnimInfo
    {
        public string animName;
        public bool loop;
        public int priority;
        public float duration;

        public AnimInfo(string animName, bool loop, int priority)
        {
            this.animName = animName;
            this.loop = loop;
            this.priority = priority;
        }
    }
}

public struct AnimData
{
    public readonly string Name;
    public readonly float TimeScale;
    public AnimationState.TrackEntryEventDelegate Event;
    public AnimData(string animName, float timeScale, AnimationState.TrackEntryEventDelegate e = null)
    {
        Name = animName;
        TimeScale = timeScale;
        Event = e;
    }
}