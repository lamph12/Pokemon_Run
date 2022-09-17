using UnityEngine;
using UnityEngine.UI;

public class HomeScene : BaseScene
{
    [SerializeField] private Button playPVEBtn;
    [SerializeField] private Button playPVPBtn;

    public void Init()
    {
        playPVEBtn.onClick.AddListener(OnClickPlayPVE);
        playPVPBtn.onClick.AddListener(OnClickPlayPVP);
    }

    public void OnClickPlayPVE()
    {
        GameController.Instance.LoadScene(SceneName.GAME_PLAY);
    }

    public void OnClickPlayPVP()
    {
    }

    /// <summary>
    ///     Hàm gọi khi stack Box = 0
    /// </summary>
    public override void OnEscapeWhenStackBoxEmpty()
    {
        //Hiển thị popup bạn có muốn thoát game ko?
    }
}