using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
// 3. 管理器类：负责存储和提供列表
public static class MonsterRegistry
{
    // 这里存储所有子类的类名
    public static readonly List<string> AllPowerNames;

    // 可选：同时存储 Type 对象，方便后续直接实例化
    public static readonly Dictionary<string, Type> PowerTypeMap;

    // 静态构造函数：只在第一次访问时运行一次
    static MonsterRegistry()
    {
        var assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集

        AllPowerNames = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(MonsterModel)) && !t.IsAbstract) // 筛选条件：是子类且非抽象
            .Select(t => t.Name) // 只要类名
            .OrderBy(n => n)     // 按字母排序 (可选)
            .ToList();

        // 额外福利：建立一个 名字->Type 的映射，方便以后 new 出来
        PowerTypeMap = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(MonsterModel)) && !t.IsAbstract)
            .ToDictionary(t => t.Name, t => t);
    }
}