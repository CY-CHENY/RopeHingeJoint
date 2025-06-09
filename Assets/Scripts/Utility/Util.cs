using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public enum SceneID
    {
        Login = 0,
        Loading = 1,
        Game = 2,
        Main = 3,
    }

    public enum ItemColor
    {
        None = -1,
        Red = 0,
        Blue,
        Green,
        // Cyan,
        Orange,
        Violet,
        Yellow,
        White,
        Black
    }

    // public class LevelConfig
    // {
    //     public string Name;
    //     public int Level;
    //     public string PrefabName;
    //     public string BoxConfig;
    //     public string iconPath;
    //     public string iconGrayPath;
    //     public string iconCollectPath;
    //     public string iconCollectGrayPath;
    // }

    public static class Util
    {
        // public static List<LevelConfig> LevelConfigs = new List<LevelConfig>()
        // {
        //     new LevelConfig(){Name = "花", Level = 1, PrefabName = "1",  BoxConfig ="LevelConfig_1", iconPath = "y2", iconGrayPath = "y1",iconCollectPath = "level1N",iconCollectGrayPath = "level1G"},
        //     new LevelConfig(){Name = "蓝莓", Level = 2, PrefabName = "2",BoxConfig ="LevelConfig_2", iconPath = "l2", iconGrayPath = "l1",iconCollectPath = "level2N",iconCollectGrayPath = "level2G"},
        //     new LevelConfig(){Name = "橘子", Level = 3, PrefabName = "3",BoxConfig ="LevelConfig_3", iconPath = "j2", iconGrayPath = "j1",iconCollectPath = "level3N",iconCollectGrayPath = "level3G"},
        //     new LevelConfig(){Name = "蛋糕", Level = 4, PrefabName = "4",BoxConfig ="LevelConfig_4", iconPath = "d2", iconGrayPath = "d1",iconCollectPath = "level4N",iconCollectGrayPath = "level4G"},
        //     new LevelConfig(){Name = "沙拉", Level = 5, PrefabName = "5",BoxConfig ="LevelConfig_5", iconPath = "s2", iconGrayPath = "s1",iconCollectPath = "level5N",iconCollectGrayPath = "level5G"},
        //     new LevelConfig(){Name = "烧烤", Level = 6, PrefabName = "6",BoxConfig ="LevelConfig_6", iconPath = "kc2", iconGrayPath = "kc1",iconCollectPath = "level6N",iconCollectGrayPath = "level6G"},
        //     new LevelConfig(){Name = "哈密瓜", Level = 7, PrefabName = "7",BoxConfig ="LevelConfig_7", iconPath = "hmg2", iconGrayPath = "hmg1",iconCollectPath = "level7N",iconCollectGrayPath = "level7G"},
        //     new LevelConfig(){Name = "寿司组合", Level = 8, PrefabName = "8",BoxConfig ="LevelConfig_8", iconPath = "ss2", iconGrayPath = "ss1",iconCollectPath = "level8N",iconCollectGrayPath = "level8G"},
        //     new LevelConfig(){Name = "葡萄", Level = 9, PrefabName = "9",BoxConfig ="LevelConfig_9", iconPath = "pt2", iconGrayPath = "pt1",iconCollectPath = "level9N",iconCollectGrayPath = "level9G"},
        //     new LevelConfig(){Name = "海鲜拼盘", Level = 10, PrefabName = "10",BoxConfig ="LevelConfig_10", iconPath = "hx2", iconGrayPath = "hx1",iconCollectPath = "level10N",iconCollectGrayPath = "level10G"}
        // };

        public static Dictionary<ItemColor, Color> ColorMapping = new Dictionary<ItemColor, Color>()
    {
        {ItemColor.None,        new Color(128/255f,     128 / 255f,     128 / 255f)},
        {ItemColor.Red,         new Color(254/255f,     127 /255f,      194 / 255f)},
        {ItemColor.Green,       new Color(37/255f,      227 /255f,      78  / 255f)},
        { ItemColor.Blue,       new Color(55/255.0f,    163 / 255f,     255 / 255f) },
        { ItemColor.Orange,     new Color(244/255.0f,   164 / 255f,     50  / 255f) },
        { ItemColor.Violet,     new Color(233/255.0f,   150 / 255f,     253 / 255f) },
        { ItemColor.Yellow,     new Color(255/255.0f,   243 / 255f,     13  / 255f) },
        { ItemColor.White,      new Color(255/255.0f,   255 / 255f,     255 / 255f) },
        { ItemColor.Black,      new Color(89f/255.0f,   89f / 255f,     89f / 255f) },
    };

        public static List<SignInEntity> SignInConfig = new List<SignInEntity>()
        {
            new SignInEntity(){Id = 1,Type = "1000;1"},
            new SignInEntity(){Id = 2,Type = "1001;1"},
            new SignInEntity(){Id = 3,Type = "1002;1"},
            new SignInEntity(){Id = 4,Type = "1000;2"},
            new SignInEntity(){Id = 5,Type = "1001;2"},
            new SignInEntity(){Id = 6,Type = "1002;2"},
            new SignInEntity(){Id = 7,Type = "1000;5|1001;5|1002;5"},
        };

        public static List<ItemEntity> ItemConfig = new List<ItemEntity>()
        {
            new ItemEntity() {Id = 1000, Name = "加孔", Type = "增加一个孔位", iconPath = "item1000"},
            new ItemEntity() {Id = 1001, Name = "清理", Type = "清理待清除的所有毛线", iconPath = "item1001"},
            new ItemEntity() {Id = 1002, Name = "超级盒子", Type = "立即填满一个盒子", iconPath = "item1002"},
        };

        public static List<PropBase> CollectGameAward = new List<PropBase>()
        {
            new PropBase(id: 1000, amount: 1),
            new PropBase(id: 1001, amount: 1),
            new PropBase(id: 1002, amount: 1),
        };


        public static string basePageUrl = "Assets/GameResources/Prefabs/UIPage/";
        public static string pageSuffix = ".prefab";

        public static ItemColor FindClosestColor(Color targetColor)
        {
            if (targetColor == null)
                return ItemColor.None;
            const float maxAllowedDistance = 0.1f; // 可调整的阈值

            ItemColor closestColor = ItemColor.None;
            float minDistance = float.MaxValue;

            foreach (var entry in ColorMapping)
            {
                Color color = entry.Value;

                // 计算颜色距离（使用欧几里得平方距离优化性能）
                float distance =
                    Mathf.Pow(color.r - targetColor.r, 2) +
                    Mathf.Pow(color.g - targetColor.g, 2) +
                    Mathf.Pow(color.b - targetColor.b, 2);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColor = entry.Key;
                }
            }

            // 检查是否在允许范围内
            return minDistance <= (maxAllowedDistance * maxAllowedDistance)
                ? closestColor
                : ItemColor.None;
        }

         public static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                // 生成一个从 0 到 i 的随机索引
                int r = UnityEngine.Random.Range(0, i + 1);

                // 交换 list[i] 和 list[r]
                T temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }
        }
        public static void DelayExecuteWithSecond(float sec, Action func)
        {
            CoroutineController.Instance.StartCoroutine(DelayExecutingWithSecond(sec, func));
        }

        private static IEnumerator DelayExecutingWithSecond(float sec, Action func)
        {
            yield return new WaitForSeconds(sec);
            func?.Invoke();
        }

        public static GameObject CreateAnimationModel(Transform transform)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            //获取所有MeshRenderer 包括子物体
            var mrs = transform.GetComponentsInChildren<MeshRenderer>(true);
            Vector3 center = Vector3.zero;
            for (int i = 0; i < mrs.Length; i++)
            {
                center += mrs[i].bounds.center;
                //Encapsulate函数重新计算bounds
                bounds.Encapsulate(mrs[i].bounds);
            }
            center /= mrs.Length;
            //创建一个新物体作为空父级
            GameObject obj = new GameObject();
            obj.name = transform.name;
            obj.transform.position = center;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.SetParent(transform.parent);
            transform.SetParent(obj.transform);

            return obj;
        }
    }
}