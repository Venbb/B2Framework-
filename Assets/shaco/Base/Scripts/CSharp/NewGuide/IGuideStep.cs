using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	/// <summary>
	/// 新手引导步骤类，记录每一步新手引导信息和执行逻辑
	/// </summary>
    public interface IGuideStep : ISetting
    {
		/// <summary>
		/// 步骤id
		/// </summary>
		string guideStepID { get; set; }

        /// <summary>
        /// 第一步引导步骤
        /// </summary>
        string firstStepID { get; set; }

        /// <summary>
        /// 引导是否开放
        /// </summary>
        bool isOpen { get; set; }

        /// <summary>
        /// 是否已经引导结束
        /// </summary>
        bool isEnd { get; set; }

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        void Execute();

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        void End();
    }
}