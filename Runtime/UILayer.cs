namespace Cowart.UIFramework
{
    /// <summary>
    /// 定义 UI 面板所在的显示层级，枚举顺序与 Hierarchy 中的渲染顺序一致。
    /// </summary>
    public enum UILayer
    {
        /// <summary>背景层，用于不参与主要交互的常驻背景。</summary>
        Background,

        /// <summary>普通层，用于页面和常规功能面板。</summary>
        Normal,

        /// <summary>固定层，用于 HUD、导航栏等常驻界面。</summary>
        Fixed,

        /// <summary>弹窗层，用于对话框、确认框及其遮罩。</summary>
        Popup,

        /// <summary>引导层，用于新手引导和输入拦截。</summary>
        Guide,

        /// <summary>顶层，用于 Loading、Toast 等最高优先级界面。</summary>
        Top
    }
}
