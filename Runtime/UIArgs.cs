namespace Cowart.UIFramework
{
    /// <summary>
    /// 所有 UI 面板打开参数的基类，具体面板可通过继承该类定义专用参数。
    /// </summary>
    public abstract class UIArgs
    {
        /// <summary>
        /// 将当前参数实例转换为指定的 UIArgs 子类型。
        /// 类型不匹配时会立即抛出转换异常，不会返回 null。
        /// </summary>
        /// <typeparam name="T">需要转换到的 UIArgs 子类型。</typeparam>
        /// <returns>转换后的当前参数实例。</returns>
        public T As<T>() where T : UIArgs
        {
            return (T)this;
        }
    }
}
