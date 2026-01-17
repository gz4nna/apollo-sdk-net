using Apollo.SDK.NET.Models;

namespace Apollo.SDK.NET.Interfaces;

/// <summary>
/// 定义 Apollo 客户端的接口
/// </summary>
public interface IApolloClient
{
    #region 核心方法
    /// <summary>
    /// 判断是否符合
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合开关条件</returns>
    bool IsToggleAllowed(string key, ApolloContext context);
    #endregion

    #region 存在性检查
    /// <summary>
    /// 判断开关是否存在
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    bool IsToggleExist(string key);

    /// <summary>
    /// 判断群组是否存在
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    bool IsAudienceExist(string toggleKey, string audienceId);

    /// <summary>
    /// 判断规则是否存在
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <param name="ruleId">规则 ID</param>
    /// <returns></returns>
    bool IsRuleExist(string toggleKey, string audienceId, string ruleId);
    #endregion

    #region 状态检查
    /// <summary>
    /// 获取开关状态
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    bool GetToggleStatus(string key);
    #endregion

    #region 数量检查
    /// <summary>
    /// 获取开关数量
    /// </summary>
    /// <returns></returns>
    int GetToggleCount();

    /// <summary>
    /// 获取群组数量
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <returns></returns>
    int GetAudienceCount(string toggleKey);

    /// <summary>
    /// 获取规则数量
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    int GetRuleCount(string toggleKey, string audienceId);
    #endregion

    #region 列表获取
    /// <summary>
    /// 获取所有开关 Key
    /// </summary>
    /// <returns></returns>
    List<string> GetAllToggleKeys();

    /// <summary>
    /// 获取指定开关的所有群组 ID
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <returns></returns>
    List<string> GetAudienceIds(string toggleKey);

    /// <summary>
    /// 获取指定开关和群组的所有规则 ID
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    List<string> GetRuleIds(string toggleKey, string audienceId);
    #endregion

    #region 模型获取
    /// <summary>
    /// 获取指定开关模型
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    Toggle GetToggle(string key);

    /// <summary>
    /// 获取指定开关和群组模型
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关或群组不存在时抛出</exception>
    Audience GetAudience(string toggleKey, string audienceId);

    /// <summary>
    /// 获取指定开关、群组和规则模型
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <param name="ruleId">规则 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关、群组或规则不存在时抛出</exception>
    Rule GetRule(string toggleKey, string audienceId, string ruleId);
    #endregion
}
