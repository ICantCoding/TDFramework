


/*
游戏平台配置
*/

namespace TDFramework {

    using System;

    //游戏平台类型
    public enum GamePlatform
    {
        GamePlatform_Editor = 0,    //编辑器模式
        GamePlatform_PC = 1,        //PC模式
        GamePlatform_Mobbile = 2,   //移动端
    }
    public class GameConfig
    {
        //指定游戏平台
        public static GamePlatform gamePlatform = GamePlatform.GamePlatform_Editor; //默认在编辑器平台下
    }
}